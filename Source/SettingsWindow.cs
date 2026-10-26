using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace Aurora;
public sealed class SettingsWindow : Window
{
    private readonly SidebarWindow sidebar;
    private Preferences P => sidebar.Prefs;
    private readonly StackPanel page = new() { Margin = new Thickness(20,16,20,16) };
    private readonly TextBlock subtitle = Ui.Text("",13,"Muted");
    private readonly TextBox search = new() { Padding = new Thickness(10,8,10,8), Margin = new Thickness(0,0,0,12) };
    private readonly ListView sensorList = new() { MinHeight = 200, MaxHeight = 300 };
    private readonly TextBlock diagnostics = Ui.Text("",12,"Muted");
    private readonly ListBox nav = new() { Background = System.Windows.Media.Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(12,4,12,4) };
    private readonly ScrollViewer pageScroll=new() {VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
    private CheckBox? startupCheck;
    private Button? pinAction;
    private readonly ComboBox pinTarget=new(){MinWidth=240,HorizontalAlignment=HorizontalAlignment.Stretch};
    private bool syncingTarget;
    private readonly TextBlock targetInfo=Ui.Text("Sensor auswählen",12,"Muted");
    private readonly TextBlock pinnedSummary=Ui.Text("",12,"Muted");
    private string? selectedDeviceId, pendingTarget;
    private StackPanel? deviceDetails;
    private string current = "Position";
    public SettingsWindow(SidebarWindow sidebar)
    {
        this.sidebar = sidebar;
        SettingsText.CurrentLanguage=P.SettingsLanguage;
        foreach(var pair in new[]{("Back","#10141C"),("Panel","#181E29"),("Edge","#2B3544"),("Text","#ECF2FA"),("Muted","#AAB6C7"),("Accent","#6ACCEB")})Resources[pair.Item1]=Ui.Brush(pair.Item2);
        void CompactStyle(Type type,params Setter[] setters){var style=new Style(type,(Style)FindResource(type));foreach(var setter in setters)style.Setters.Add(setter);Resources[type]=style;}
        CompactStyle(typeof(Button),new Setter(PaddingProperty,new Thickness(9,5,9,5)),new Setter(FontSizeProperty,12d),new Setter(HorizontalAlignmentProperty,HorizontalAlignment.Left));
        CompactStyle(typeof(ComboBox),new Setter(MarginProperty,new Thickness(0)),new Setter(FontSizeProperty,12d));
        CompactStyle(typeof(CheckBox),new Setter(MarginProperty,new Thickness(0,3,0,3)),new Setter(FontSizeProperty,12d));
        CompactStyle(typeof(Slider),new Setter(MarginProperty,new Thickness(0,2,0,2)));
        SetResourceReference(BackgroundProperty,"Back"); SetResourceReference(ForegroundProperty,"Text");
        Title = "Sidebar by NemeSIS · Einstellungen"; Width = 860; Height = 650; MinWidth = 800; MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var layout = new Grid();layout.SetResourceReference(Panel.BackgroundProperty,"Back"); layout.ColumnDefinitions.Add(new() { Width = new GridLength(195) }); layout.ColumnDefinitions.Add(new() { Width = new GridLength(1,GridUnitType.Star) });
        var left = new DockPanel(); left.SetResourceReference(Panel.BackgroundProperty,"Panel");
        var logo = new StackPanel { Margin = new Thickness(23,20,12,10) };
        logo.Children.Add(Ui.Text("Sidebar by NemeSIS",15,"Accent")); logo.Children.Add(Ui.Text("DEIN SYSTEM. IM BLICK.",9,"Muted")); DockPanel.SetDock(logo,Dock.Top); left.Children.Add(logo);
        var badge = Ui.Text("Änderungen werden automatisch gespeichert.",11,"Muted");badge.TextWrapping=TextWrapping.Wrap; badge.Margin = new Thickness(23,12,18,14); DockPanel.SetDock(badge,Dock.Bottom); left.Children.Add(badge);
        foreach(var entry in new[]{("Position","Position & Größe"),("Darstellung","Design & Schrift"),("Farben","Text & Diagramme"),("Geräte","Geräte & Werte"),("Karten","Karten & Reihenfolge"),("Grenzen","Warngrenzen"),("Voraussetzungen","Einrichtung"),("System","Systeminfo")})nav.Items.Add(new ListBoxItem{Tag=entry.Item1,Content=entry.Item2,Padding=new Thickness(13,8,10,8),Margin=new Thickness(0,1,0,1)});
        var languagePanel=new StackPanel{Margin=new Thickness(12,12,12,12)};languagePanel.Children.Add(Ui.Text("Sprache",11,"Muted"));
        var languages=new ComboBox{ItemsSource=SettingsText.Languages,SelectedItem=SettingsText.Languages.FirstOrDefault(l=>l.Key==P.SettingsLanguage)??SettingsText.Languages[0],MinWidth=150};languagePanel.Children.Add(languages);System.Windows.Automation.AutomationProperties.SetAutomationId(languages,"SettingsLanguage");
        languages.SelectionChanged+=(_,_)=>{if(languages.SelectedItem is SettingsLanguage choice){P.SettingsLanguage=choice.Key;SettingsText.CurrentLanguage=choice.Key;P.Save();Navigate(current);}};DockPanel.SetDock(languagePanel,Dock.Bottom);left.Children.Add(languagePanel);
        nav.SelectionChanged += (_,_) => { if(nav.SelectedItem is ListBoxItem i && i.Tag is string key) Navigate(key); };
        left.Children.Add(nav); layout.Children.Add(left);
        pageScroll.Content=page;Grid.SetColumn(pageScroll,1);layout.Children.Add(pageScroll);Content=layout;
        var gridView = new GridView();
        var pinFactory=new FrameworkElementFactory(typeof(CheckBox));pinFactory.SetBinding(CheckBox.IsCheckedProperty,new System.Windows.Data.Binding("Pinned"){Mode=System.Windows.Data.BindingMode.TwoWay,UpdateSourceTrigger=System.Windows.Data.UpdateSourceTrigger.PropertyChanged});pinFactory.SetValue(CheckBox.HorizontalAlignmentProperty,HorizontalAlignment.Center);pinFactory.SetValue(CheckBox.ToolTipProperty,"Häkchen = in Eigene Werte hinzugefügt. Zum Entfernen Häkchen ausschalten.");
        gridView.Columns.Add(new GridViewColumn {Header="Hinzugefügt",Width=88,CellTemplate=new DataTemplate{VisualTree=pinFactory}});
        gridView.Columns.Add(new GridViewColumn { Header = "Gerät", Width = 185, DisplayMemberBinding = new System.Windows.Data.Binding("Device") });
        gridView.Columns.Add(new GridViewColumn { Header = "Sensor", Width = 190, DisplayMemberBinding = new System.Windows.Data.Binding("Name") });
        gridView.Columns.Add(new GridViewColumn { Header = "Wert", Width = 85, DisplayMemberBinding = new System.Windows.Data.Binding("Display") });
        gridView.Columns.Add(new GridViewColumn { Header = "Widget", Width = 125, DisplayMemberBinding = new System.Windows.Data.Binding("Widget") });
        sensorList.View = gridView;
        pinTarget.SelectionChanged+=(_,_)=>{if(syncingTarget||sensorList.SelectedItem is not SensorDisplay selected||pinTarget.SelectedItem is not SensorChoice choice)return;P.PinTargets[selected.Id]=choice.Id;if(P.Pins.Contains(selected.Id))RevealTarget(selected.Id);sidebar.Apply();UpdateSensors();};
        sensorList.SizeChanged+=(_,_)=>{double remaining=Math.Max(140,sensorList.ActualWidth-308);gridView.Columns[1].Width=remaining*.35;gridView.Columns[2].Width=remaining*.65;gridView.Columns[3].Width=75;gridView.Columns[4].Width=125;};
        sensorList.SelectionChanged+=(_,_)=>UpdatePinAction();
        search.TextChanged += (_,_) => UpdateSensors();
        SelectPage("Position");
    }
    private void Heading(string title, string description)
    {
        page.Children.Clear();
        var heading = Ui.Text(title,22); heading.FontWeight = FontWeights.SemiBold; heading.Margin = new Thickness(0,0,0,4); page.Children.Add(heading);
        subtitle.FontSize=12;subtitle.Text = description; subtitle.TextWrapping = TextWrapping.Wrap; subtitle.Margin = new Thickness(0,0,0,12); page.Children.Add(subtitle);
    }
    private void Label(string text) { var t = Ui.Text(text,12,"Muted");t.TextWrapping=TextWrapping.Wrap; t.Margin = new Thickness(0,10,0,3); page.Children.Add(t); }
    private void Note(string text)
    {
        var t = Ui.Text(text,12,"Muted"); t.TextWrapping = TextWrapping.Wrap; t.Margin = new Thickness(0,5,0,6); t.LineHeight = 17; page.Children.Add(t);
    }
    private CheckBox Check(string text, bool value, Action<bool> change)
    {
        var c = new CheckBox { Content=value?"Ein":"Aus", IsChecked = value }; c.Click += (_,_) => {c.Content=c.IsChecked==true?"Ein":"Aus"; change(c.IsChecked==true); sidebar.Apply();SettingsText.Apply(this,P.SettingsLanguage); }; SettingRow(text,c);return c;
    }
    private void Select<T>(string label, IEnumerable<T> values, T selected, Action<T> change)
    {
        var c = new ComboBox { ItemsSource = values, SelectedItem = selected, ItemTemplate=SettingsText.ChoiceTemplate(),HorizontalAlignment = HorizontalAlignment.Stretch,MaxWidth=440 };
        c.SelectionChanged += (_,_) => { if(c.SelectedItem is T item) { change(item); sidebar.Apply(); } }; SettingRow(label,c);
    }
    private void Slider(string label, double min, double max, double value, double tick, string format, Action<double> change)
    {
        var t = Ui.Text(value.ToString(format),12);t.HorizontalAlignment=HorizontalAlignment.Right;t.VerticalAlignment=VerticalAlignment.Center;
        var s = new Slider { Minimum = min, Maximum = max, Value = value, TickFrequency = tick, IsSnapToTickEnabled = true, VerticalAlignment=VerticalAlignment.Center };
        var controls=new Grid();controls.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});controls.ColumnDefinitions.Add(new(){Width=new GridLength(54)});controls.Children.Add(s);Grid.SetColumn(t,1);controls.Children.Add(t);
        s.ValueChanged += (_,_) => { t.Text = s.Value.ToString(format); change(s.Value); sidebar.Apply(); }; SettingRow(label,controls);
    }
    private void SettingRow(string label,FrameworkElement control)
    {
        var row=new Grid {Margin=new Thickness(0,3,0,3),MinHeight=32};row.ColumnDefinitions.Add(new(){Width=new GridLength(185)});row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star),MaxWidth=440});
        var text=Ui.Text(label,12,"Muted");text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,12,0);text.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(text);Grid.SetColumn(control,1);control.HorizontalAlignment=HorizontalAlignment.Stretch;control.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(control);page.Children.Add(row);
    }
    private void Navigate(string target)
    {
        current = target;
        pageScroll.ScrollToTop();
        if(target is "Sensoren" or "Eigene Werte")search.Clear();
        switch(target)
        {
            case "Übersicht":
                Heading("Einstellungen", "Passe die Sidebar an. Jede Änderung wird sofort übernommen und gespeichert.");
                var shortcuts=new StackPanel();
                foreach(var entry in new[]{("Position","Position & Größe","Monitor, Breite, Autostart und Einpassen"),("Darstellung","Design & Schrift","Layout, Farben und Lesbarkeit"),("Geräte","Geräte & Werte","Geräte, sichtbare Werte und eigene Sensoren"),("Karten","Karten & Reihenfolge","Sichtbare Karten und ihre Reihenfolge"),("Voraussetzungen","Sensoren einrichten","Treiber und Sensorzugriff prüfen")})
                {
                    var content=new StackPanel();var title=Ui.Text(entry.Item2,13);title.FontWeight=FontWeights.SemiBold;content.Children.Add(title);var description=Ui.Text(entry.Item3,11,"Muted");description.TextWrapping=TextWrapping.Wrap;description.Margin=new Thickness(0,2,0,0);content.Children.Add(description);
                    var button=Ui.Button("",()=>SelectPage(entry.Item1));button.Content=content;button.HorizontalAlignment=HorizontalAlignment.Stretch;button.HorizontalContentAlignment=HorizontalAlignment.Left;button.Margin=new Thickness(0,0,0,5);button.Padding=new Thickness(12,7,12,7);shortcuts.Children.Add(button);
                }
                page.Children.Add(shortcuts);
                Card("Aktuelle Sidebar",()=>Note("Layout: "+CardLayouts.Get(P.Layout).Name+" · "+P.Columns+" Spalte(n) · "+P.ColumnWidth.ToString("0")+" px pro Spalte\nAutostart: "+(ScheduledAutostart.Enabled?"Ein":"Aus")));
                break;
            case "Position":
                Heading("Position & Größe", "Lege fest, wo die Sidebar sitzt und wie viel Platz sie benötigt.");
                Card("Bildschirm",()=> {
                var monitors = Forms.Screen.AllScreens.Select((s,i) => new MonitorChoice(s.DeviceName,$"Monitor {i+1} · {s.Bounds.Width} × {s.Bounds.Height}"+(s.Primary ? " · Hauptmonitor" : ""))).ToList();
                var selected = monitors.FirstOrDefault(m=>m.Id == P.Monitor) ?? monitors.FirstOrDefault(m=>Forms.Screen.PrimaryScreen?.DeviceName == m.Id) ?? monitors[0];
                Select("Monitor", monitors,selected, m=>P.Monitor=m.Id);
                var identify=Ui.Button("Monitore identifizieren",()=> {
                    foreach(var (s,i) in Forms.Screen.AllScreens.Select((s,i)=>(s,i)))
                    {
                        var w = new Window { Title = "Monitor "+(i+1), Width = 200, Height = 140, WindowStyle = WindowStyle.None, Topmost = true, ShowInTaskbar = false, Content = new TextBlock { Text = (i+1).ToString(), FontSize = 70, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
                        w.SourceInitialized += (_,_) => { var source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(w).Handle); var dpi = source?.CompositionTarget?.TransformFromDevice ?? System.Windows.Media.Matrix.Identity; var center = dpi.Transform(new Point(s.Bounds.Left+s.Bounds.Width/2,s.Bounds.Top+s.Bounds.Height/2)); w.Left=center.X-100; w.Top=center.Y-70; };
                        w.Show(); var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) }; timer.Tick += (_,_)=>{ timer.Stop(); w.Close(); }; timer.Start();
                    }
                });identify.Margin=new Thickness(185,2,0,4);page.Children.Add(identify);
                Select("Bildschirmseite", new[]{"Rechts","Links"},P.Side,s=>P.Side=s);
                });
                Card("Größe & Anordnung",()=> {
                Slider("Breite pro Spalte (px)",240,440,P.ColumnWidth,10,"0",v=>P.ColumnWidth=v);
                Select("Karten nebeneinander",new[]{1,2,3,4},P.Columns,v=>P.Columns=v);
                Check("Alle Karten auf eine Ansicht einpassen",P.Fit,v=>P.Fit=v);
                Note("Einpassen aus: Karten behalten ihre Größe und lassen sich scrollen.");
                });
                Card("Fenster & Autostart",()=> {
                Check("Platz am Bildschirmrand reservieren",P.ReserveSpace,v=>P.ReserveSpace=v);
                Check("Immer im Vordergrund",P.Topmost,v=>P.Topmost=v);
                StartupCheckbox();
                });
                break;
            case "Darstellung":
                Heading("Design & Schrift","Layout verändert den Aufbau. Das Farbschema bestimmt die Farben deiner Sidebar.");
                Card("Darstellung",()=>{LayoutPicker();DesignPicker();});
                Card("Schrift & Anzeige",()=> {
                Slider("Schriftgröße (%)",85,140,P.FontScale*100,5,"0",v=>P.FontScale=v/100);
                Slider("Widget-Hintergrund (%)",0,100,P.Opacity*100,5,"0",v=>P.Opacity=v/100);
                Select("Temperatureinheit",new[]{"°C","°F"},P.Fahrenheit?"°F":"°C",v=>P.Fahrenheit=v=="°F");
                });
                break;
            case "Farben":
                Heading("Text & Diagramme","Textfarben sowie Farben und Stärke der Balken und Instrumente anpassen.");
                Card("Textfarben",()=>{
                    ColorSetting("Überschriften","TextColor",()=>P.TextColor,v=>P.TextColor=v,()=>DesignCatalog.Get(P.Theme).Text);
                    ColorSetting("Sensorbezeichnungen","LabelColor",()=>P.LabelColor,v=>P.LabelColor=v,()=>DesignCatalog.Get(P.Theme).Muted);
                    ColorSetting("Hardware-Namen","HardwareNameColor",()=>P.HardwareNameColor,v=>P.HardwareNameColor=v,()=>DesignCatalog.ColorOr(P.TextColor,DesignCatalog.Get(P.Theme).Text));
                    ColorSetting("Messwerte","ValueColor",()=>P.ValueColor,v=>P.ValueColor=v,()=>DesignCatalog.ColorOr(P.TextColor,DesignCatalog.Get(P.Theme).Text));
                });
                Card("Balken & Instrumente",()=>{
                    var categories=new[]{"CPU","RAM","GPU","Drives","Network","Mainboard","Controller","Custom"};
                    var category=new ComboBox{ItemsSource=categories,SelectedIndex=0};System.Windows.Automation.AutomationProperties.SetAutomationId(category,"GraphCategory");SettingRow("Widget",category);
                    string SelectedCategory()=>category.SelectedItem as string??"CPU";
                    var graphBox=ColorSetting("Akzentfarbe","GraphColor",()=>P.GraphColors.TryGetValue(SelectedCategory(),out var color)?color:"",v=>{if(v.Length==0)P.GraphColors.Remove(SelectedCategory());else P.GraphColors[SelectedCategory()]=v;},()=>DesignCatalog.CategoryColor(DesignCatalog.Get(P.Theme),SelectedCategory()));
                    category.SelectionChanged+=(_,_)=>RefreshColor(graphBox);
                    ColorSetting("Hintergrundspur","GraphTrackColor",()=>P.GraphTrackColor,v=>P.GraphTrackColor=v,()=>DesignCatalog.Get(P.Theme).Track);
                    ColorSetting("Zweiter Ring","SecondaryGraphColor",()=>P.SecondaryGraphColor,v=>P.SecondaryGraphColor=v,()=>"#78B9FC");
                    Slider("Linien- / Balkenstärke (%)",50,160,P.GraphScale*100,10,"0",v=>P.GraphScale=v/100);
                });
                Note("Leer = Farbe des Farbschemas. Eigene Farben bleiben beim Designwechsel erhalten.");
                var resetColors=Ui.Button("Farbanpassungen zurücksetzen",()=>{P.TextColor=P.LabelColor=P.HardwareNameColor=P.ValueColor=P.GraphTrackColor=P.SecondaryGraphColor="";P.GraphColors.Clear();P.GraphScale=1;sidebar.Apply();Navigate("Farben");});System.Windows.Automation.AutomationProperties.SetAutomationId(resetColors,"ResetColors");page.Children.Add(resetColors);
                break;
            case "CPU":
                Heading("CPU-Temperatur auswählen","Automatisch wird die CPU-/Socket-Temperatur des Mainboards bevorzugt. Du kannst auch einen bestimmten Sensor fest auswählen.");
                if(sidebar.Latest==null){Note("Die Sensorerkennung läuft. Diese Seite danach erneut öffnen.");break;}
                foreach(var cpu in sidebar.Latest.Devices.Where(d=>d.Category=="CPU"))
                {
                    var choices=new List<SensorChoice>{new("","Automatisch: CPU / Socket bevorzugen")};
                    choices.AddRange(sidebar.Latest.Sensors.Where(s=>s.Kind=="Temperature" && (s.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController" || s.Id.StartsWith(cpu.Id+"/",StringComparison.Ordinal))).Select(s=>new SensorChoice(s.Id,s.Device+" · "+s.Name+" · "+new Metric(s.Id,s.Name,s.Value,s.Unit).Display(P.Fahrenheit))));
                    P.CpuTemperatureSources.TryGetValue(cpu.Id,out string? cpuSelected);
                    Select(cpu.Name,choices,choices.FirstOrDefault(c=>c.Id==cpuSelected)??choices[0],choice=>{if(choice.Id.Length==0)P.CpuTemperatureSources.Remove(cpu.Id);else P.CpuTemperatureSources[cpu.Id]=choice.Id;});
                    Note("Aktuelle Quelle: "+cpu.Metrics.FirstOrDefault(m=>m.Id.EndsWith("/display-temperature"))?.Source);
                }
                page.Children.Add(Ui.Button("CPU-Kartenwerte ein-/ausblenden",()=>SelectPage("Werte")));
                Note("CPU, CPU Diode und CPU Package sind verschiedene Messstellen. Fehlt ein CPU-/Socket-Sensor, wird eine verfügbare Package-/Die-Quelle ausdrücklich als solche beschriftet.");
                break;
            case "Karten":
                Heading("Karten & Reihenfolge", "Häkchen schalten Karten ein oder aus. Mit den Pfeilen änderst du ihre Reihenfolge.");
                foreach(var category in P.Order.ToArray())
                {
                    var row = new DockPanel { Margin = new Thickness(0,4,0,4) };
                    var up = Ui.Button("↑",()=>Move(category,-1)); var down = Ui.Button("↓",()=>Move(category,1));
                    DockPanel.SetDock(down,Dock.Right); DockPanel.SetDock(up,Dock.Right); down.Margin = new Thickness(5,0,0,0); row.Children.Add(down); row.Children.Add(up);
                    var check = new CheckBox { Content = Ui.Icon(category)+"  "+category, IsChecked = !P.HiddenCategories.Contains(category) };
                    check.Click += (_,_)=>{ Toggle(P.HiddenCategories,category,check.IsChecked!=true); sidebar.Apply(); }; row.Children.Add(check);var frame=new Border{Child=row,Padding=new Thickness(14,5,14,5),Margin=new Thickness(0,0,0,7),CornerRadius=new CornerRadius(9),BorderThickness=new Thickness(1)};frame.SetResourceReference(Border.BackgroundProperty,"Panel");frame.SetResourceReference(Border.BorderBrushProperty,"Edge");page.Children.Add(frame);
                }
                break;
            case "Geräte":
            case "Werte":
                Heading("Geräte & Werte", "Links ein Gerät wählen. Rechts Anzeige, Werte und eigene Sensoren bearbeiten.");
                DevicePage();break;
            case "Sensoren":
            case "Eigene Werte":
                Heading(target == "Sensoren" ? "Sensoren hinzufügen" : "Eigene Werte",target=="Sensoren"?"Sensor suchen und Häkchen setzen. Eigene Werte erscheinen automatisch im zugehörigen Hardware-Widget.":"Zusätzliche Werte in deinen Hardware-Widgets. Sensor auswählen, um sein Ziel-Widget zu ändern; Häkchen ausschalten entfernt ihn.");
                pinnedSummary.Margin=new Thickness(0,0,0,10);page.Children.Add(pinnedSummary);
                Label("Suchen nach Gerät, Sensor oder Typ"); page.Children.Add(search); page.Children.Add(sensorList);
                Label("Ziel-Widget für den ausgewählten Sensor");page.Children.Add(pinTarget);targetInfo.TextWrapping=TextWrapping.Wrap;page.Children.Add(targetInfo);System.Windows.Automation.AutomationProperties.SetAutomationId(pinTarget,"PinTargetWidget");
                var buttons = new WrapPanel { Margin = new Thickness(0,15,0,0) };
                pinAction=Ui.Button("Sensor auswählen",()=>{if(sensorList.SelectedItem is SensorDisplay s)ChangePin(s.Id,!P.Pins.Contains(s.Id));});System.Windows.Automation.AutomationProperties.SetAutomationId(pinAction,"PinSelectedSensor");buttons.Children.Add(pinAction);
                var switchPage=Ui.Button("Zurück zu Geräte & Werte",()=>SelectPage("Geräte"));switchPage.Margin=new Thickness(8,0,0,0);buttons.Children.Add(switchPage);
                page.Children.Add(buttons); Note("Ziel-Widget auswählen. Häkchen hinzufügen oder entfernen.");
                UpdateSensors(); break;
            case "Grenzen":
                Heading("Warngrenzen", "Karten werden bei Überschreitung rot umrandet.");
                Slider("Temperaturgrenze (°C)",40,110,P.TempLimit,1,"0",v=>P.TempLimit=v);
                Slider("Auslastungsgrenze (%)",50,100,P.LoadLimit,1,"0",v=>P.LoadLimit=v);
                Note("Die Warnung ist nur optisch.");
                break;
            case "Voraussetzungen":
                Heading("Sensoren einrichten", "Status prüfen und fehlende Komponenten gezielt einrichten. Es wird nichts automatisch installiert.");
                Label("1 · Laufzeit");Note(System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory,"coreclr.dll"))?".NET-Laufzeit ist enthalten – keine zusätzliche Installation nötig.":".NET "+Environment.Version+" ist verfügbar.");
                Label("2 · Sensortreiber für CPU und Mainboard");Note(SensorAccess.Describe());
                var install=Ui.Button("PawnIO installieren / aktualisieren",()=>{});
                install.Click+=async (_,_)=>{install.IsEnabled=false;try{await Prerequisites.InstallPawnIO(this);}catch(Exception error){MessageBox.Show(this,error.Message,"PawnIO-Einrichtung");}finally{install.IsEnabled=true;}};page.Children.Add(install);
                Note("Für CPU- und Mainboard-Sensoren wird PawnIO benötigt.");
                Label("3 · Automatischer Sensorzugriff");Note(ScheduledAutostart.Describe());
                page.Children.Add(Ui.Button("Sensorzugriff einmalig einrichten",()=>{try{ScheduledAutostart.RequestAccess();}catch(System.ComponentModel.Win32Exception error)when(error.NativeErrorCode==1223){}catch(Exception error){MessageBox.Show(this,error.Message,"Sensorzugriff");}}));
                Note("Die Einrichtung benötigt einmalig Administratorrechte.");
                Label("4 · Grafiktreiber (falls GPU-Werte fehlen)");
                var drivers=new WrapPanel();foreach(var vendor in new[]{("NVIDIA","https://www.nvidia.com/Download/index.aspx"),("AMD","https://www.amd.com/en/support"),("Intel","https://www.intel.com/content/www/us/en/download-center/home.html")})drivers.Children.Add(Ui.Button(vendor.Item1+" Treiber",()=>Process.Start(new ProcessStartInfo(vendor.Item2){UseShellExecute=true})));page.Children.Add(drivers);
                Note("Grafiktreiber werden beim Hersteller heruntergeladen.");
                Select("Aktualisierung (Millisekunden)",new[]{500,1000,2000,5000,10000},P.Interval,v=>P.Interval=v);
                break;
            case "System":
                Heading("Systeminfo", "Unterstützung hängt von Hardware, Firmware und Treibern ab.");
                Note("Autostart wird über die Windows-Aufgabenplanung ausgeführt.");
                diagnostics.TextWrapping = TextWrapping.Wrap; diagnostics.Margin = new Thickness(0,20,0,20); page.Children.Add(diagnostics);
                page.Children.Add(Ui.Button("Konfiguration öffnen",()=>Process.Start(new ProcessStartInfo("notepad.exe", "\""+Preferences.Path+"\"") { UseShellExecute = true })));
                Note("Einstellungen werden neben der EXE gespeichert.");
                Refresh(sidebar.Latest); break;
        }
        SettingsText.Apply(this,P.SettingsLanguage);
    }
    private void DevicePage()
    {
        if(sidebar.Latest==null){Note("Sensoren werden erkannt …");return;}
        var devices=sidebar.Latest.Devices.ToList();var separate=PinPlacement.Compose(sidebar.Latest,P).FirstOrDefault(d=>d.Id=="custom");if(separate!=null)devices.Add(separate);
        var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(190)});grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var list=new ListBox{BorderThickness=new Thickness(1),MaxHeight=440,Margin=new Thickness(0,0,12,0),VerticalAlignment=VerticalAlignment.Top,HorizontalContentAlignment=HorizontalAlignment.Stretch};ScrollViewer.SetHorizontalScrollBarVisibility(list,ScrollBarVisibility.Disabled);list.SetResourceReference(BackgroundProperty,"Panel");list.SetResourceReference(BorderBrushProperty,"Edge");System.Windows.Automation.AutomationProperties.SetAutomationId(list,"DeviceList");
        var ordinal=new Dictionary<string,int>();
        foreach(var device in devices.OrderBy(d=>{int index=P.Order.IndexOf(d.Category);return index<0?99:index;}))
        {ordinal.TryGetValue(device.Category,out int index);ordinal[device.Category]=index+1;string category=device.Category+(devices.Count(d=>d.Category==device.Category)>1?" "+(index+1):"");var label=Ui.Text(category+" · "+device.Name,12);label.TextTrimming=TextTrimming.CharacterEllipsis;list.Items.Add(new ListBoxItem{Tag=device.Id,Content=label,ToolTip=device.Name+"\n"+device.Id,HorizontalContentAlignment=HorizontalAlignment.Stretch,Padding=new Thickness(8,9,8,9)});}
        deviceDetails=new StackPanel();Grid.SetColumn(deviceDetails,1);grid.Children.Add(list);grid.Children.Add(deviceDetails);page.Children.Add(grid);
        list.SelectionChanged+=(_,_)=>{if(list.SelectedItem is ListBoxItem item){selectedDeviceId=(string)item.Tag;RenderDeviceDetails();}};
        list.SelectedItem=list.Items.OfType<ListBoxItem>().FirstOrDefault(i=>(string)i.Tag==selectedDeviceId)??list.Items.OfType<ListBoxItem>().FirstOrDefault();
    }
    private void RenderDeviceDetails()
    {
        if(deviceDetails==null||sidebar.Latest==null||selectedDeviceId==null)return;
        var device=PinPlacement.Compose(sidebar.Latest,P).FirstOrDefault(d=>d.Id==selectedDeviceId);if(device==null)return;deviceDetails.Children.Clear();int start=page.Children.Count;
        var title=Ui.Text(device.Name,15);title.FontWeight=FontWeights.SemiBold;title.TextWrapping=TextWrapping.Wrap;title.Margin=new Thickness(0,0,0,8);page.Children.Add(title);
        var enabledControl=Check("Gerät anzeigen",!P.HiddenDevices.Contains(device.Id)&&!P.HiddenCategories.Contains(device.Category),enabled=>{Toggle(P.HiddenDevices,device.Id,!enabled);if(enabled)P.HiddenCategories.RemoveAll(s=>s==device.Category);});System.Windows.Automation.AutomationProperties.SetAutomationId(enabledControl,"DeviceVisible:"+device.Id);
        Label("Sichtbare Werte");
        var own=P.Pins.Where(id=>(PinPlacement.Target(sidebar.Latest,P,id)?.Id??"custom")==device.Id).Distinct().ToHashSet();
        MetricChecks(device with{Metrics=device.Metrics.Where(m=>!own.Contains(m.Id)).ToList()});
        Label("Eigene Sensoren");
        if(own.Count==0)Note("Keine eigenen Sensoren hinzugefügt.");
        foreach(string id in own)
        {
            var metric=device.Metrics.FirstOrDefault(m=>m.Id==id)??new Metric(id,id,null,"");
            var controls=new StackPanel{Orientation=Orientation.Horizontal};var visible=new CheckBox{Content=metric.Display(P.Fahrenheit),IsChecked=P.IsMetricVisible(device.Id,id),VerticalAlignment=VerticalAlignment.Center};visible.Click+=(_,_)=>{P.SetMetricVisible(device.Id,id,visible.IsChecked==true);sidebar.Apply();};controls.Children.Add(visible);
            var remove=Ui.Button("Entfernen",()=>{PinPlacement.SetPinned(sidebar.Latest,P,id,false);sidebar.Apply();RenderDeviceDetails();});remove.Margin=new Thickness(8,0,0,0);System.Windows.Automation.AutomationProperties.SetAutomationId(remove,"RemovePin:"+id);controls.Children.Add(remove);SettingRow(metric.Label,controls);
        }
        var add=Ui.Button("Sensor hinzufügen",()=>{pendingTarget=device.Id;Navigate("Sensoren");});add.Margin=new Thickness(0,6,0,8);page.Children.Add(add);
        if(device.Category=="CPU")
        {
            Label("CPU-Temperaturquelle");var choices=new List<SensorChoice>{new("","Automatisch: CPU / Socket bevorzugen")};choices.AddRange(sidebar.Latest.Sensors.Where(s=>s.Kind=="Temperature"&&(s.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController"||s.Id.StartsWith(device.Id+"/",StringComparison.Ordinal))).Select(s=>new SensorChoice(s.Id,s.Device+" · "+s.Name)));P.CpuTemperatureSources.TryGetValue(device.Id,out string? selected);
            Select("Temperatursensor",choices,choices.FirstOrDefault(c=>c.Id==selected)??choices[0],choice=>{if(choice.Id.Length==0)P.CpuTemperatureSources.Remove(device.Id);else P.CpuTemperatureSources[device.Id]=choice.Id;});
        }
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){page.Children.Remove(child);deviceDetails.Children.Add(child);}SettingsText.Apply(this,P.SettingsLanguage);
    }
    private void Card(string title,Action build)
    {
        int start=page.Children.Count;build();var body=new StackPanel();var heading=Ui.Text(title,13);heading.FontWeight=FontWeights.SemiBold;heading.Margin=new Thickness(0,0,0,5);body.Children.Add(heading);
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){page.Children.Remove(child);body.Children.Add(child);}
        var frame=new Border{Child=body,Padding=new Thickness(12,9,12,9),Margin=new Thickness(0,0,0,8),CornerRadius=new CornerRadius(6),BorderThickness=new Thickness(1)};frame.SetResourceReference(Border.BackgroundProperty,"Panel");frame.SetResourceReference(Border.BorderBrushProperty,"Edge");page.Children.Add(frame);
    }
    private void Section(string title,bool expanded,Action build)
    {
        int start=page.Children.Count;build();var body=new StackPanel {Margin=new Thickness(6,3,6,5)};
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){page.Children.Remove(child);body.Children.Add(child);}
        var caption=Ui.Text(title,13);caption.FontWeight=FontWeights.SemiBold;caption.TextWrapping=TextWrapping.Wrap;
        var expander=new Expander {Header=caption,IsExpanded=expanded,Content=body,Padding=new Thickness(7)};
        var frame=new Border{Child=expander,CornerRadius=new CornerRadius(6),BorderThickness=new Thickness(1),Margin=new Thickness(0,0,0,6)};frame.SetResourceReference(Border.BackgroundProperty,"Panel");frame.SetResourceReference(Border.BorderBrushProperty,"Edge");page.Children.Add(frame);
    }
    private void Move(string category,int delta)
    {
        int i=P.Order.IndexOf(category), j=i+delta;
        if(j<0 || j>=P.Order.Count) return;
        (P.Order[i],P.Order[j])=(P.Order[j],P.Order[i]); sidebar.Apply(); Navigate("Karten");
    }
    private void SelectPage(string name){if(name is "Werte" or "Eigene Werte")name="Geräte";var item=nav.Items.OfType<ListBoxItem>().FirstOrDefault(i=>i.Tag as string==name);if(item!=null){nav.SelectedItem=item;if(current!=name)Navigate(name);}else Navigate(name);}
    private void MetricChecks(Device device)
    {
        foreach(var metric in device.Metrics)
        {
            var check=new CheckBox{Content=metric.Display(P.Fahrenheit),IsChecked=P.IsMetricVisible(device.Id,metric.Id),ToolTip=metric.Source??metric.Id};
            System.Windows.Automation.AutomationProperties.SetAutomationId(check,"Metric:"+device.Id+"|"+metric.Id);
            check.Click+=(_,_)=>{P.SetMetricVisible(device.Id,metric.Id,check.IsChecked==true);sidebar.Apply();};SettingRow(metric.Label,check);
        }
    }
    private void ChangePin(string id,bool added)
    {
        if(added&&pendingTarget!=null&&!P.PinTargets.ContainsKey(id))P.PinTargets[id]=pendingTarget;PinPlacement.SetPinned(sidebar.Latest,P,id,added);if(added)RevealTarget(id);sidebar.Apply();UpdatePinAction();pinnedSummary.Text=P.Pins.Count+" hinzugefügte Werte";
        Dispatcher.BeginInvoke(new Action(UpdateSensors));if(added)sidebar.RevealPinnedValues(id);
    }
    private void UpdatePinAction()
    {
        if(pinTarget.IsDropDownOpen)return;
        syncingTarget=true;
        try
        {
            var choices=new List<SensorChoice>{new("","Automatisch · zum zugehörigen Widget"),new("custom","Separate Karte · Eigene Werte")};
            if(sidebar.Latest!=null)choices.AddRange(sidebar.Latest.Devices.Select(d=>new SensorChoice(d.Id,d.Category+" · "+d.Name)));
            string target="";if(sensorList.SelectedItem is SensorDisplay row)P.PinTargets.TryGetValue(row.Id,out target!);
            target??=pendingTarget??"";if(target.Length>0&&!choices.Any(c=>c.Id==target))choices.Add(new(target,"Widget derzeit nicht verfügbar · "+target));
            pinTarget.ItemsSource=choices;pinTarget.SelectedItem=choices.First(c=>c.Id==target);pinTarget.IsEnabled=sensorList.SelectedItem is SensorDisplay;
            targetInfo.Text=sensorList.SelectedItem is SensorDisplay chosen&&sidebar.Latest!=null?"Angezeigt in: "+(PinPlacement.Target(sidebar.Latest,P,chosen.Id) is Device device?device.Category+" · "+device.Name:"Separate Karte · Eigene Werte"):"Sensor auswählen, um das Ziel-Widget festzulegen.";
        }
        finally{syncingTarget=false;}
        if(pinAction==null)return;
        if(sensorList.SelectedItem is SensorDisplay selected){pinAction.IsEnabled=true;pinAction.Content=P.Pins.Contains(selected.Id)?"Aus eigenen Werten entfernen":"Zu eigenen Werten hinzufügen";}
        else {pinAction.IsEnabled=false;pinAction.Content="Sensor auswählen";}
    }
    private void RevealTarget(string id)
    {
        if(sidebar.Latest!=null && PinPlacement.Target(sidebar.Latest,P,id) is Device device){P.HiddenDevices.RemoveAll(s=>s==device.Id);P.HiddenCategories.RemoveAll(s=>s==device.Category);P.SetMetricVisible(device.Id,id,true);}
        else{P.HiddenCategories.RemoveAll(s=>s=="Custom");P.HiddenDevices.RemoveAll(s=>s=="custom");}
    }
    private static void RefreshColor(TextBox box){if(box.Tag is Action refresh)refresh();}
    private TextBox ColorSetting(string label,string id,Func<string> get,Action<string> set,Func<string> fallback)
    {
        var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(104)});grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});grid.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        var input=new TextBox{MaxLength=7,Padding=new Thickness(7,4,7,4),VerticalContentAlignment=VerticalAlignment.Center,ToolTip="#RRGGBB"};System.Windows.Automation.AutomationProperties.SetAutomationId(input,id);
        var pick=Ui.Button("Auswählen",()=>{});pick.Margin=new Thickness(6,0,6,0);pick.HorizontalAlignment=HorizontalAlignment.Stretch;var swatch=new Border{Width=13,Height=13,Margin=new Thickness(0,0,6,0),BorderBrush=System.Windows.Media.Brushes.Gray,BorderThickness=new Thickness(1)};
        var content=new StackPanel{Orientation=Orientation.Horizontal};content.Children.Add(swatch);content.Children.Add(Ui.Text("Auswählen",12));pick.Content=content;
        bool refreshing=false;
        void Refresh(){refreshing=true;input.Text=get();swatch.Background=Ui.Brush(DesignCatalog.ColorOr(get(),fallback()));input.ClearValue(BorderBrushProperty);refreshing=false;}
        input.Tag=(Action)Refresh;
        input.TextChanged+=(_,_)=>{if(refreshing)return;string value=input.Text.Trim();if(value.Length>0&&DesignCatalog.ColorOr(value,"").Length==0){input.BorderBrush=Ui.Brush("#F47777");return;}set(value.Length==0?"":DesignCatalog.ColorOr(value,""));input.ClearValue(BorderBrushProperty);swatch.Background=Ui.Brush(DesignCatalog.ColorOr(get(),fallback()));sidebar.Apply();};
        pick.Click+=(_,_)=>{using var dialog=new Forms.ColorDialog{FullOpen=true,Color=System.Drawing.ColorTranslator.FromHtml(DesignCatalog.ColorOr(get(),fallback()))};if(dialog.ShowDialog(new FormsOwner(new System.Windows.Interop.WindowInteropHelper(this).Handle))==Forms.DialogResult.OK)input.Text=$"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";};
        var reset=Ui.Button("↺",()=>{input.Text="";Refresh();});reset.ToolTip="Standardfarbe";System.Windows.Automation.AutomationProperties.SetAutomationId(reset,"Reset:"+id);
        grid.Children.Add(input);Grid.SetColumn(pick,1);grid.Children.Add(pick);Grid.SetColumn(reset,2);grid.Children.Add(reset);Refresh();SettingRow(label,grid);return input;
    }
    private sealed class FormsOwner:Forms.IWin32Window{public IntPtr Handle{get;}public FormsOwner(IntPtr handle){Handle=handle;}}
    private void DesignPicker()
    {
        Select("Farbschema",DesignCatalog.All,DesignCatalog.Get(P.Theme),design=>P.Theme=design.Key);
    }
    private void LayoutPicker()
    {
        Select("Kartenlayout",CardLayouts.All,CardLayouts.Get(P.Layout),layout=>P.Layout=layout.Key);
    }
    private void StartupCheckbox()
    {
        var check=new CheckBox { Content="Mit Windows starten", IsChecked=ScheduledAutostart.Enabled };
        startupCheck=check;
        check.Click+=(_,_)=>{
            try { ScheduledAutostart.Request(check.IsChecked==true); }
            catch(System.ComponentModel.Win32Exception error) when(error.NativeErrorCode==1223) { }
            catch(Exception error) { MessageBox.Show(error.Message,"Autostart konnte nicht geändert werden"); }
            check.IsChecked=ScheduledAutostart.Enabled;
        };
        SettingRow("Autostart",check);
    }
    private static void Toggle(List<string> list,string id,bool present) { list.RemoveAll(s=>s==id); if(present) list.Add(id); }
    public void Refresh(Snapshot? snapshot)
    {
        if(current=="Position" && startupCheck!=null) startupCheck.IsChecked=ScheduledAutostart.Enabled;
        if(current is "Sensoren" or "Eigene Werte") UpdateSensors();
        if(current == "System")
        {
            bool admin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            diagnostics.Text = $"Sidebar by NemeSIS 2.8 · .NET {Environment.Version}\nZugriff: {(admin ? "Administrator" : "Standardbenutzer")}\n"+ScheduledAutostart.Describe()+"\n"+
                (snapshot == null ? "Sensoren werden initialisiert …" : $"{snapshot.Devices.Count} Geräte · {snapshot.Sensors.Count} Messwerte · {snapshot.Time:HH:mm:ss}\n"+string.Join("\n",snapshot.Warnings));
            SettingsText.Apply(this,P.SettingsLanguage);
        }
    }
    private void UpdateSensors()
    {
        if(current is not ("Sensoren" or "Eigene Werte")) return;
        string? selected = (sensorList.SelectedItem as SensorDisplay)?.Id;
        string query = search.Text.Trim();
        IEnumerable<SensorRow> sources=sidebar.Latest?.Sensors??new();
        if(current=="Eigene Werte")sources=P.Pins.Distinct().Select(id=>sources.FirstOrDefault(s=>s.Id==id)??new SensorRow(id,"Nicht verfügbar",id,"",null,"" )).ToList();
        var items = sources.Where(s=>query.Length==0 || (s.Device+" "+s.Name+" "+s.Kind).Contains(query,StringComparison.OrdinalIgnoreCase))
            .OrderBy(s=>s.Device).ThenBy(s=>s.Name)
            .Select(s=>new SensorDisplay(s.Id,s.Device,s.Name,s.Kind,new Metric(s.Id,s.Name,s.Value,s.Unit).Display(P.Fahrenheit),P.Pins.Contains(s.Id),ChangePin,sidebar.Latest!=null&&PinPlacement.Target(sidebar.Latest,P,s.Id) is Device device?device.Category+" · "+device.Name:"Separate Karte")).ToList();
        if (sensorList.ItemsSource is List<SensorDisplay> existing && existing.Select(s=>s.Id+"/"+s.Name).SequenceEqual(items.Select(s=>s.Id+"/"+s.Name)))
        {
            for(int i=0;i<existing.Count;i++){existing[i].Display=items[i].Display;existing[i].Widget=items[i].Widget;existing[i].SyncPinned(items[i].Pinned);}
        }
        else { sensorList.ItemsSource=items; sensorList.SelectedItem=items.FirstOrDefault(s=>s.Id==selected); }
        pinnedSummary.Text=P.Pins.Count+" hinzugefügte Werte";UpdatePinAction();
        SettingsText.Apply(this,P.SettingsLanguage);
    }
    private record MonitorChoice(string Id,string Label) { public override string ToString()=>SettingsText.Translate(Label,SettingsText.CurrentLanguage); }
    private record SensorChoice(string Id,string Label) { public override string ToString()=>SettingsText.Translate(Label,SettingsText.CurrentLanguage); }
    private sealed class SensorDisplay : INotifyPropertyChanged
    {
        public string Id { get; } public string Device { get; } public string Name { get; } public string Kind { get; }
        private string display;
        private string widget;
        public string Widget{get=>widget;set{if(widget==value)return;widget=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Widget)));}}
        private bool pinned;private readonly Action<string,bool> change;
        public bool Pinned {get=>pinned;set{if(pinned==value)return;pinned=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Pinned)));change(Id,value);}}
        public void SyncPinned(bool value){if(pinned==value)return;pinned=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Pinned)));}
        public string Display { get=>display; set { if(display==value) return; display=value; PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Display))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        public SensorDisplay(string id,string device,string name,string kind,string display,bool pinned,Action<string,bool> change,string widget) { Id=id; Device=device; Name=name; Kind=kind; this.display=display;this.pinned=pinned;this.change=change;this.widget=widget; }
    }
}
