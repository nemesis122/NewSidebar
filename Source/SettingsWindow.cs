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
public sealed class SettingsWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly SidebarWindow sidebar;
    private readonly System.Threading.CancellationTokenSource addressCancellation=new();
    private bool addressBusy;
    private readonly NetworkAddressLookup addressLookup;
    private Preferences P => sidebar.Prefs;
    private readonly StackPanel page = new() { Margin = new Thickness(22,8,22,8) };
    private readonly TextBlock subtitle = Ui.Text("",13,"Muted");
    private readonly TextBox search = new() { Padding = new Thickness(10,8,10,8), Margin = new Thickness(0,0,0,12) };
    private readonly ListView sensorList = new() { MinHeight = 200, MaxHeight = 300 };
    private readonly TextBlock sourceStatus=Ui.Text("",12,"Muted");
    private readonly TextBlock diagnostics = Ui.Text("",12,"Muted");
    private readonly Wpf.Ui.Controls.NavigationView nav = new(){PaneDisplayMode=Wpf.Ui.Controls.NavigationViewPaneDisplayMode.Left,OpenPaneLength=218,IsPaneOpen=true,IsPaneToggleVisible=false,IsBackButtonVisible=Wpf.Ui.Controls.NavigationViewBackButtonVisible.Collapsed,HeaderVisibility=Visibility.Collapsed,IsTopSeparatorVisible=false,IsFooterSeparatorVisible=false,FrameMargin=new Thickness(0),TransitionDuration=0};
    private readonly Dictionary<string,string> navigationLabels=new();
    private readonly Wpf.Ui.Controls.InfoBar status=new(){IsOpen=false,IsClosable=true,Margin=new Thickness(14,4,14,8)};
    private bool navigationReady;
    private readonly ScrollViewer pageScroll=new() {VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};
    private Wpf.Ui.Controls.ToggleSwitch? startupCheck;
    private Button? pinAction;
    private readonly ComboBox pinTarget=new(){MinWidth=240,HorizontalAlignment=HorizontalAlignment.Stretch};
    private bool syncingTarget;
    private readonly TextBlock targetInfo=Ui.Text("Sensor auswählen",12,"Muted");
    private readonly TextBlock pinnedSummary=Ui.Text("",12,"Muted");
    private string? selectedDeviceId, pendingTarget;
    private StackPanel? deviceDetails;
    private string current = "Position";
    public SettingsWindow(SidebarWindow sidebar,NetworkAddressLookup? networkLookup=null)
    {
        Resources.MergedDictionaries.Add(SettingsSurface.Resources);ExtendsContentIntoTitleBar=true;WindowBackdropType=Wpf.Ui.Controls.WindowBackdropType.Mica;
        this.sidebar = sidebar;addressLookup=networkLookup??new NetworkAddressLookup();
        nav.Resources.MergedDictionaries.Add(Resources);nav.SetResourceReference(BackgroundProperty,"Back");
        SetResourceReference(FontFamilyProperty,"AppFont");
        Closed+=(_,_)=>addressCancellation.Cancel();
        SettingsText.CurrentLanguage=P.SettingsLanguage;
        foreach(var pair in new[]{("Back","#10141C"),("Panel","#181E29"),("Edge","#2B3544"),("Text","#ECF2FA"),("Muted","#AAB6C7"),("Accent","#6ACCEB")})Resources[pair.Item1]=Ui.Brush(pair.Item2);
        foreach(var pair in new[]{("NavigationViewContentBackground","#10141C"),("NavigationViewItemBackgroundSelected","#263447"),("NavigationViewItemBackgroundPointerOver","#202B3B"),("NavigationViewItemForeground","#ECF2FA"),("NavigationViewItemForegroundPointerOver","#FFFFFF"),("NavigationViewSelectionIndicatorForeground","#6ACCEB")})Resources[pair.Item1]=Ui.Brush(pair.Item2);
        void CompactStyle(Type type,params Setter[] setters){var style=new Style(type,(Style)FindResource(type));foreach(var setter in setters)style.Setters.Add(setter);Resources[type]=style;}
        CompactStyle(typeof(Wpf.Ui.Controls.Button),new Setter(PaddingProperty,new Thickness(9,5,9,5)),new Setter(FontSizeProperty,12d),new Setter(HorizontalAlignmentProperty,HorizontalAlignment.Left));
        CompactStyle(typeof(ComboBox),new Setter(MarginProperty,new Thickness(0)),new Setter(FontSizeProperty,12d));
        // WPF UI 4.3 animates a template Tag on checked boxes; recycling that template can throw.
        // Keep the existing Windows selection boxes; general preferences use native ToggleSwitch controls.
        var selectionStyle=new Style(typeof(CheckBox),Application.Current.TryFindResource(typeof(CheckBox)) as Style);selectionStyle.Setters.Add(new Setter(MarginProperty,new Thickness(0,3,0,3)));selectionStyle.Setters.Add(new Setter(FontSizeProperty,12d));Resources[typeof(CheckBox)]=selectionStyle;
        CompactStyle(typeof(Slider),new Setter(MarginProperty,new Thickness(0,2,0,2)));
        CompactStyle(typeof(Wpf.Ui.Controls.NumberBox),new Setter(FontSizeProperty,12d),new Setter(PaddingProperty,new Thickness(6,4,6,4)));
        
        SetResourceReference(BackgroundProperty,"Back"); SetResourceReference(ForegroundProperty,"Text");
        Title = "Sidebar by NemeSIS · Einstellungen"; Width = 1020; Height = 780; MinWidth = 800; MinHeight = 560;
        ShowInTaskbar=false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var logo=new StackPanel{Margin=new Thickness(20,18,8,14)};logo.Children.Add(Ui.Text("Sidebar by NemeSIS",16,"Text"));logo.Children.Add(Ui.Text("Einstellungen",11,"Muted"));nav.PaneHeader=logo;
        var entries=new[]{("Übersicht","Übersicht",Wpf.Ui.Controls.SymbolRegular.Home24,typeof(OverviewSettingsPage)),("Position","Position & Größe",Wpf.Ui.Controls.SymbolRegular.Desktop24,typeof(PositionSettingsPage)),("Darstellung","Design & Schrift",Wpf.Ui.Controls.SymbolRegular.Board24,typeof(AppearanceSettingsPage)),("Textgrößen","Textgrößen",Wpf.Ui.Controls.SymbolRegular.TextFontSize24,typeof(TypographySettingsPage)),("Farben","Text & Diagramme",Wpf.Ui.Controls.SymbolRegular.Color24,typeof(ColorsSettingsPage)),("Geräte","Geräte & Werte",Wpf.Ui.Controls.SymbolRegular.DesktopPulse24,typeof(DevicesSettingsPage)),("Karten","Karten & Reihenfolge",Wpf.Ui.Controls.SymbolRegular.ArrowSort24,typeof(CardsSettingsPage)),("Grenzen","Warngrenzen",Wpf.Ui.Controls.SymbolRegular.Shield24,typeof(LimitsSettingsPage)),("Voraussetzungen","Einrichtung",Wpf.Ui.Controls.SymbolRegular.Settings24,typeof(SetupSettingsPage)),("System","Systeminfo",Wpf.Ui.Controls.SymbolRegular.Info24,typeof(SystemSettingsPage))};
        foreach(var entry in entries){navigationLabels[entry.Item1]=entry.Item2;var item=new Wpf.Ui.Controls.NavigationViewItem{Tag=entry.Item1,Content=entry.Item2,TargetPageTag=entry.Item1,TargetPageType=entry.Item4,Icon=new Wpf.Ui.Controls.SymbolIcon(entry.Item3),Margin=new Thickness(4,1,4,1)};item.Resources.MergedDictionaries.Add(Resources);nav.MenuItems.Add(item);}
        var footer=new StackPanel{Margin=new Thickness(16,10,16,16)};footer.Resources.MergedDictionaries.Add(Resources);footer.Children.Add(Ui.Text("Sprache",11,"Muted"));var languages=new ComboBox{ItemsSource=SettingsText.Languages,SelectedItem=SettingsText.Languages.FirstOrDefault(l=>l.Key==P.SettingsLanguage)??SettingsText.Languages[0],HorizontalAlignment=HorizontalAlignment.Stretch,Margin=new Thickness(0,5,0,10)};languages.Resources.MergedDictionaries.Add(Resources);languages.Style=(Style)Resources[typeof(ComboBox)];footer.Children.Add(languages);System.Windows.Automation.AutomationProperties.SetAutomationId(languages,"SettingsLanguage");var badge=Ui.Text("Änderungen werden automatisch gespeichert.",10,"Muted");badge.TextWrapping=TextWrapping.Wrap;footer.Children.Add(badge);nav.PaneFooter=footer;
        languages.SelectionChanged+=(_,_)=>{if(languages.SelectedItem is SettingsLanguage choice){P.SettingsLanguage=choice.Key;SettingsText.CurrentLanguage=choice.Key;P.Save();Navigate(current);TranslateNavigation();}};
        nav.SelectionChanged+=(_,_)=>{if(nav.SelectedItem is Wpf.Ui.Controls.NavigationViewItem item&&item.Tag is string key)Navigate(key);};
        pageScroll.Resources.MergedDictionaries.Add(Resources);pageScroll.SetResourceReference(BackgroundProperty,"Back");pageScroll.Content=page;nav.ContentOverlay=pageScroll;nav.SetPageProviderService(new SettingsPageProvider(pageScroll));nav.Loaded+=(_,_)=>{if(navigationReady)return;navigationReady=true;nav.ContentOverlay=null;SelectPage(current);};
        var shell=new Grid();shell.SetResourceReference(Panel.BackgroundProperty,"Back");shell.RowDefinitions.Add(new(){Height=new GridLength(36)});shell.RowDefinitions.Add(new(){Height=new GridLength(1,GridUnitType.Star)});shell.RowDefinitions.Add(new(){Height=GridLength.Auto});var titleBar=new Wpf.Ui.Controls.TitleBar{ButtonsForeground=Ui.Brush("#ECF2FA"),Foreground=Ui.Brush("#ECF2FA"),Title="Sidebar by NemeSIS",ShowMinimize=true,ShowMaximize=true};titleBar.SetResourceReference(Control.BackgroundProperty,"Back");shell.Children.Add(titleBar);Grid.SetRow(nav,1);shell.Children.Add(nav);Grid.SetRow(status,2);shell.Children.Add(status);Content=shell;
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
        SelectPage("Übersicht");
    }
    private void Heading(string title, string description)
    {
        foreach(UIElement child in page.Children)PreserveControlResources(child);
        page.Children.Clear();
        var heading = Ui.Text(title,22); heading.FontWeight = FontWeights.SemiBold; heading.Margin = new Thickness(0,0,0,4); page.Children.Add(heading);
        subtitle.FontSize=12;subtitle.Text = description; subtitle.TextWrapping = TextWrapping.Wrap; subtitle.Margin = new Thickness(0,0,0,9); page.Children.Add(subtitle);
    }
    private void Label(string text) { var t = Ui.Text(text,12,"Muted");t.TextWrapping=TextWrapping.Wrap; t.Margin = new Thickness(0,10,0,3); page.Children.Add(t); }
    private void Note(string text)
    {
        var t = Ui.Text(text,12,"Muted"); t.TextWrapping = TextWrapping.Wrap; t.Margin = new Thickness(0,5,0,6); t.LineHeight = 17; page.Children.Add(t);
    }
    private Wpf.Ui.Controls.ToggleSwitch Check(string text, bool value, Action<bool> change)
    {
        var c = new Wpf.Ui.Controls.ToggleSwitch { OnContent=SettingsText.Translate("Ein",P.SettingsLanguage),OffContent=SettingsText.Translate("Aus",P.SettingsLanguage), IsChecked = value }; c.Click += (_,_) => { change(c.IsChecked==true); sidebar.Apply();SettingsText.Apply(this,P.SettingsLanguage); }; SettingRow(text,c);return c;
    }
    private ComboBox Select<T>(string label, IEnumerable<T> values, T selected, Action<T> change)
    {
        var c = new ComboBox { ItemsSource = values, SelectedItem = selected, ItemTemplate=SettingsText.ChoiceTemplate(),HorizontalAlignment = HorizontalAlignment.Stretch,MaxWidth=440 };
        c.SelectionChanged += (_,_) => { if(c.SelectedItem is T item) { change(item); sidebar.Apply(); } }; SettingRow(label,c);return c;
    }
    private void Slider(string label,double min,double max,double value,double tick,string format,Action<double> change,Func<bool>? shown=null,Action<bool>? setShown=null)
    {
        var input=new Wpf.Ui.Controls.NumberBox{Minimum=min,Maximum=max,Value=value,SmallChange=tick,LargeChange=tick*4,MaxDecimalPlaces=format.Contains('.')?2:0,AcceptsExpression=false,SpinButtonPlacementMode=Wpf.Ui.Controls.NumberBoxSpinButtonPlacementMode.Compact,MinWidth=86,Margin=new Thickness(10,0,0,0),VerticalAlignment=VerticalAlignment.Center};
        System.Windows.Automation.AutomationProperties.SetAutomationId(input,"SettingNumber:"+label);
        var s=new Slider{Minimum=min,Maximum=max,Value=value,TickFrequency=tick,IsSnapToTickEnabled=true,VerticalAlignment=VerticalAlignment.Center};
        if(label=="Hardware-Symbole")System.Windows.Automation.AutomationProperties.SetAutomationId(s,"HardwareIconSize");
        if(label=="Grafikgröße (%)"){System.Windows.Automation.AutomationProperties.SetAutomationId(s,"WholeGraphicSize");s.ToolTip=SettingsText.Translate("Skaliert die ganze Grafik mit Zahl und Einheit. Bei wenig Platz wird die Größe passend begrenzt.",P.SettingsLanguage);}
        var controls=new Grid();controls.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});controls.ColumnDefinitions.Add(new(){Width=new GridLength(96)});controls.Children.Add(s);Grid.SetColumn(input,1);controls.Children.Add(input);
        if(label=="Schriftgröße (%)"){controls.ColumnDefinitions.Add(new(){Width=GridLength.Auto});var resetText=ActionButton("↺",()=>{P.ResetTypography();sidebar.Apply();Navigate("Darstellung");});resetText.Padding=new Thickness(5,3,5,3);resetText.Margin=new Thickness(5,0,0,0);resetText.ToolTip=SettingsText.Translate("Text- und Hardware-Symbolgrößen auf 100 % zurücksetzen",P.SettingsLanguage);System.Windows.Automation.AutomationProperties.SetAutomationId(resetText,"ResetTextSize");Grid.SetColumn(resetText,2);controls.Children.Add(resetText);}
        bool syncing=false;s.ValueChanged+=(_,_)=>{syncing=true;input.Value=s.Value;syncing=false;change(s.Value);sidebar.Apply();};
        input.ValueChanged+=(_,args)=>{if(syncing)return;if(args.NewValue is double number&&double.IsFinite(number)){s.Value=Math.Clamp(number,min,max);}else{syncing=true;input.Value=s.Value;syncing=false;}};
        if(shown!=null&&setShown!=null){var toggle=new Wpf.Ui.Controls.ToggleSwitch{IsChecked=shown(),OnContent=SettingsText.Translate("Anzeigen",P.SettingsLanguage),OffContent=SettingsText.Translate("Anzeigen",P.SettingsLanguage),Margin=new Thickness(12,0,0,0),VerticalAlignment=VerticalAlignment.Center};System.Windows.Automation.AutomationProperties.SetAutomationId(toggle,label=="Hardware-Symbole"?"ShowHardwareIcons":"ShowText:"+label);s.IsEnabled=input.IsEnabled=shown();toggle.Click+=(_,_)=>{bool visible=toggle.IsChecked==true;setShown(visible);s.IsEnabled=input.IsEnabled=visible;sidebar.Apply();};controls.ColumnDefinitions.Add(new(){Width=new GridLength(124)});System.Windows.Automation.AutomationProperties.SetName(toggle,SettingsText.Translate(label,P.SettingsLanguage)+": "+SettingsText.Translate("Anzeigen",P.SettingsLanguage));Grid.SetColumn(toggle,controls.ColumnDefinitions.Count-1);controls.Children.Add(toggle);}
        SettingRow(label,controls);
    }
    private void SettingRow(string label,FrameworkElement control)
    {
        var row=new Grid {Margin=new Thickness(0,2,0,2),MinHeight=32};row.ColumnDefinitions.Add(new(){Width=new GridLength(185)});row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star),MaxWidth=440});
        var text=Ui.Text(label,12,"Muted");text.TextWrapping=TextWrapping.Wrap;text.Margin=new Thickness(0,0,12,0);text.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(text);Grid.SetColumn(control,1);control.HorizontalAlignment=HorizontalAlignment.Stretch;control.VerticalAlignment=VerticalAlignment.Center;row.Children.Add(control);page.Children.Add(row);
    }
    private void Navigate(string target)
    {
        current = target;
        TranslateNavigation();
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
                    var button=ActionButton("",()=>SelectPage(entry.Item1));button.Content=content;button.HorizontalAlignment=HorizontalAlignment.Stretch;button.HorizontalContentAlignment=HorizontalAlignment.Left;button.Margin=new Thickness(0,0,0,5);button.Padding=new Thickness(12,7,12,7);shortcuts.Children.Add(button);
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
                var identify=ActionButton("Monitore identifizieren",()=> {
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
                Slider("Breite pro Spalte (px)",50,440,P.ColumnWidth,10,"0",v=>P.ColumnWidth=v);
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
                FontPicker();
                Slider("Schriftgröße (%)",65,140,P.FontScale*100,5,"0",v=>P.FontScale=v/100);
                Slider("Grafikgröße (%)",50,200,P.InstrumentScale*100,5,"0",v=>P.InstrumentScale=v/100);
                Slider("Widget-Hintergrund (%)",0,100,P.Opacity*100,5,"0",v=>P.Opacity=v/100);
                var expandReadings=Check("Text unter dem Graphen",P.ExpandMultiColumnReadings,v=>P.ExpandMultiColumnReadings=v);System.Windows.Automation.AutomationProperties.SetAutomationId(expandReadings,"ExpandMultiColumnReadings");
                expandReadings.IsEnabled=true;
                expandReadings.ToolTip=SettingsText.Translate("Graph oben, Werte unten – auch bei einer Spalte.",P.SettingsLanguage);
                Select("Temperatureinheit",new[]{"°C","°F"},P.Fahrenheit?"°F":"°C",v=>P.Fahrenheit=v=="°F");
                });
                break;
            case "Textgrößen":
                Heading("Textgrößen","Die Einzelgrößen wirken zusätzlich zur allgemeinen Schriftgröße. 100 % entspricht dem gewählten Design.");

                Card("Textgrößen einzeln (%)",()=>{
                Slider("Hardware-Symbole",50,200,P.HardwareIconScale*100,5,"0",v=>P.HardwareIconScale=v/100,()=>P.ShowHardwareIcons,v=>P.ShowHardwareIcons=v);
                Slider("Widget-Titel",50,200,P.TitleTextScale*100,5,"0",v=>P.TitleTextScale=v/100,()=>P.ShowWidgetTitles,v=>P.ShowWidgetTitles=v);
                Slider("Hardware-Namen",50,200,P.HardwareTextScale*100,5,"0",v=>P.HardwareTextScale=v/100,()=>P.ShowHardwareNames,v=>P.ShowHardwareNames=v);
                Slider("Bezeichnungen",50,200,P.LabelTextScale*100,5,"0",v=>P.LabelTextScale=v/100,()=>P.ShowLabels,v=>P.ShowLabels=v);
                Slider("Messwerte",50,200,P.ValueTextScale*100,5,"0",v=>P.ValueTextScale=v/100,()=>P.ShowReadings,v=>P.ShowReadings=v);
                Slider("Instrument-Zahlen",50,200,P.InstrumentTextScale*100,5,"0",v=>P.InstrumentTextScale=v/100,()=>P.ShowInstrumentNumbers,v=>P.ShowInstrumentNumbers=v);
                Slider("Instrument-Einheiten",50,200,P.UnitTextScale*100,5,"0",v=>P.UnitTextScale=v/100,()=>P.ShowInstrumentUnits,v=>P.ShowInstrumentUnits=v);
                });
                var resetSizes=ActionButton("Textgrößen zurücksetzen",()=>{P.ResetTypography();sidebar.Apply();Navigate("Textgrößen");});resetSizes.ToolTip=SettingsText.Translate("Text- und Hardware-Symbolgrößen auf 100 % zurücksetzen",P.SettingsLanguage);System.Windows.Automation.AutomationProperties.SetAutomationId(resetSizes,"ResetIndividualTextSizes");page.Children.Add(resetSizes);
                var showAll=ActionButton("Alle Texte anzeigen",()=>{P.ShowAllTextAndIcons();sidebar.Apply();Navigate("Textgrößen");});showAll.Margin=new Thickness(0,6,0,0);showAll.ToolTip=SettingsText.Translate("Alle Texte und Hardware-Symbole anzeigen",P.SettingsLanguage);System.Windows.Automation.AutomationProperties.SetAutomationId(showAll,"ShowAllSidebarText");page.Children.Add(showAll);
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
                var resetColors=ActionButton("Farbanpassungen zurücksetzen",()=>{P.TextColor=P.LabelColor=P.HardwareNameColor=P.ValueColor=P.GraphTrackColor=P.SecondaryGraphColor="";P.GraphColors.Clear();P.GraphScale=1;sidebar.Apply();Navigate("Farben");});System.Windows.Automation.AutomationProperties.SetAutomationId(resetColors,"ResetColors");page.Children.Add(resetColors);
                break;
            case "CPU":
                Heading("CPU-Temperatur auswählen","Automatisch wird die CPU-/Socket-Temperatur des Mainboards bevorzugt. Du kannst auch einen bestimmten Sensor fest auswählen.");
                if(sidebar.Latest==null){Note("Die Sensorerkennung läuft. Diese Seite danach erneut öffnen.");break;}
                foreach(var cpu in sidebar.Latest.Devices.Where(d=>d.Category=="CPU"))
                {
                    var choices=new List<SensorChoice>{new("","Automatisch: CPU / Socket bevorzugen")};
                    choices.AddRange(sidebar.Latest.Sensors.Where(s=>s.Kind=="Temperature" && (s.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController" || s.Id.StartsWith(cpu.Id+"/",StringComparison.Ordinal))).Select(s=>new SensorChoice(s.Id,s.Device+" · "+s.Name+" · "+new Metric(s.Id,s.Name,s.Value,s.Unit){IsCpuClock=s.Kind=="Clock"&&s.HardwareKind=="Cpu"}.Display(P.Fahrenheit))));
                    P.CpuTemperatureSources.TryGetValue(cpu.Id,out string? cpuSelected);
                    Select(cpu.Name,choices,choices.FirstOrDefault(c=>c.Id==cpuSelected)??choices[0],choice=>{if(choice.Id.Length==0)P.CpuTemperatureSources.Remove(cpu.Id);else P.CpuTemperatureSources[cpu.Id]=choice.Id;});
                    Note("Aktuelle Quelle: "+cpu.Metrics.FirstOrDefault(m=>m.Id.EndsWith("/display-temperature"))?.Source);
                }
                page.Children.Add(ActionButton("CPU-Kartenwerte ein-/ausblenden",()=>SelectPage("Werte")));
                Note("CPU, CPU Diode und CPU Package sind verschiedene Messstellen. Fehlt ein CPU-/Socket-Sensor, wird eine verfügbare Package-/Die-Quelle ausdrücklich als solche beschriftet.");
                break;
            case "Karten":
                Heading("Karten & Reihenfolge", "Häkchen schalten Karten ein oder aus. Mit den Pfeilen änderst du ihre Reihenfolge.");
                var combined=Check("Alle Werte in einer Kachel (HW Sensors)",P.CombineSensorCards,v=>P.CombineSensorCards=v);
                System.Windows.Automation.AutomationProperties.SetAutomationId(combined,"CombineSensorCards");
                Note("Die Hardwarebereiche behalten ihr Originaldesign mit eigenen Instrumenten und Verläufen. Nur der äußere Rahmen wird zu HW Sensors zusammengefasst.");
                foreach(var category in P.Order.ToArray())
                {
                    var row = new DockPanel { Margin = new Thickness(0,4,0,4) };
                    var up = ActionButton("↑",()=>Move(category,-1)); var down = ActionButton("↓",()=>Move(category,1));
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
                pinAction=ActionButton("Sensor auswählen",()=>{if(sensorList.SelectedItem is SensorDisplay s)ChangePin(s.Id,!P.Pins.Contains(s.Id));});System.Windows.Automation.AutomationProperties.SetAutomationId(pinAction,"PinSelectedSensor");buttons.Children.Add(pinAction);
                var switchPage=ActionButton("Zurück zu Geräte & Werte",()=>SelectPage("Geräte"));switchPage.Margin=new Thickness(8,0,0,0);buttons.Children.Add(switchPage);
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
                Label("Sensorquelle");
                var sourceChoice=Select("Sensorerkennung",SensorSources.All,SensorSources.All.First(s=>s.Key==SensorSources.Normalize(P.SensorSource)),v=>{P.SensorSource=v.Key;sourceStatus.Text=SettingsText.Translate("Sensorquelle wird gewechselt …",P.SettingsLanguage);});
                System.Windows.Automation.AutomationProperties.SetAutomationId(sourceChoice,"SensorSource");
                Note("Beide Optionen sind eingebaut. Der Wechsel erfolgt im Hintergrund. Kein separates Programm erforderlich.");
                Note("Open Hardware Monitor + PawnIO: GPU und SMART über Open Hardware Monitor, CPU und Mainboard über die LibreHardwareMonitor/PawnIO-Brücke. Nicht unterstützte Geräte verwenden Libre als Ersatz.");
                Note("Die Libre-Option ergänzt nicht erkannte Laufwerke automatisch mit dem zweiten eingebauten Leser.");
                Note("Ohne WinRing0 und InpOut. Beide Optionen benötigen PawnIO für erweiterte CPU- und Mainboard-Werte.");
                Note("Beim ersten Wechsel werden passende Geräte und Werte mit deiner bisherigen Auswahl übernommen. Danach bleiben Änderungen je Quelle erhalten. RAM und Netzwerk verwenden Windows.");
                sourceStatus.TextWrapping=TextWrapping.Wrap;sourceStatus.Margin=new Thickness(0,0,0,12);page.Children.Add(sourceStatus);Refresh(sidebar.Latest);
                Label("1 · Laufzeit");Note(System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory,"coreclr.dll"))?".NET-Laufzeit ist enthalten – keine zusätzliche Installation nötig.":".NET "+Environment.Version+" ist verfügbar.");
                Label("2 · Sensortreiber für CPU und Mainboard");Note(SensorAccess.Describe());
                var install=ActionButton("PawnIO installieren / aktualisieren",()=>{});
                install.Click+=async (_,_)=>{install.IsEnabled=false;try{await Prerequisites.InstallPawnIO(this);}catch(Exception error){ShowStatus("PawnIO-Einrichtung",error.Message,Wpf.Ui.Controls.InfoBarSeverity.Error);}finally{install.IsEnabled=true;}};page.Children.Add(install);
                Note("Für CPU- und Mainboard-Sensoren wird PawnIO benötigt.");
                Label("3 · Automatischer Sensorzugriff");Note(ScheduledAutostart.Describe());
                page.Children.Add(ActionButton("Sensorzugriff einmalig einrichten",()=>{try{ScheduledAutostart.RequestAccess();}catch(System.ComponentModel.Win32Exception error)when(error.NativeErrorCode==1223){}catch(Exception error){ShowStatus("Sensorzugriff",error.Message,Wpf.Ui.Controls.InfoBarSeverity.Error);}}));
                Note("Die Einrichtung benötigt einmalig Administratorrechte.");
                Label("4 · Grafiktreiber (falls GPU-Werte fehlen)");
                var drivers=new WrapPanel();foreach(var vendor in new[]{("NVIDIA","https://www.nvidia.com/Download/index.aspx"),("AMD","https://www.amd.com/en/support"),("Intel","https://www.intel.com/content/www/us/en/download-center/home.html")})drivers.Children.Add(ActionButton(vendor.Item1+" Treiber",()=>Process.Start(new ProcessStartInfo(vendor.Item2){UseShellExecute=true})));page.Children.Add(drivers);
                Note("Grafiktreiber werden beim Hersteller heruntergeladen.");
                Select("Aktualisierung (Millisekunden)",new[]{500,1000,2000,5000,10000},P.Interval,v=>P.Interval=v);
                break;
            case "System":
                Heading("Systeminfo", "Unterstützung hängt von Hardware, Firmware und Treibern ab.");
                Note("Autostart wird über die Windows-Aufgabenplanung ausgeführt.");
                diagnostics.TextWrapping = TextWrapping.Wrap; diagnostics.Margin = new Thickness(0,20,0,20); page.Children.Add(diagnostics);
                page.Children.Add(ActionButton("Konfiguration öffnen",()=>Process.Start(new ProcessStartInfo("notepad.exe", "\""+Preferences.Path+"\"") { UseShellExecute = true })));
                Note("Einstellungen werden neben der EXE gespeichert.");
                Refresh(sidebar.Latest); break;
        }
        SettingsText.Apply(this,P.SettingsLanguage);TranslateNavigation();
    }
    public void SelectDevice(string deviceId){selectedDeviceId=deviceId;SelectPage("Geräte");Navigate("Geräte");}
    private void DevicePage()
    {
        if(sidebar.Latest==null){Note("Sensoren werden erkannt …");return;}
        var devices=sidebar.Latest.Devices.ToList();var separate=PinPlacement.Compose(sidebar.Latest,P).FirstOrDefault(d=>d.Id=="custom");if(separate!=null)devices.Add(separate);
        var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(190)});grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var list=new ListBox{BorderThickness=new Thickness(1),MaxHeight=440,Margin=new Thickness(0,0,12,0),VerticalAlignment=VerticalAlignment.Top,HorizontalContentAlignment=HorizontalAlignment.Stretch};ScrollViewer.SetHorizontalScrollBarVisibility(list,ScrollBarVisibility.Disabled);list.SetResourceReference(BackgroundProperty,"Panel");list.SetResourceReference(BorderBrushProperty,"Edge");System.Windows.Automation.AutomationProperties.SetAutomationId(list,"DeviceList");
        var ordinal=new Dictionary<string,int>();
        foreach(var device in devices.OrderBy(d=>{int index=P.Order.IndexOf(d.Category);return index<0?99:index;}))
        {ordinal.TryGetValue(device.Category,out int index);ordinal[device.Category]=index+1;string category=device.Category+(devices.Count(d=>d.Category==device.Category)>1?" "+(index+1):"");var label=Ui.Text(category+" · "+P.HardwareName(device.Id,device.Name),12);label.TextTrimming=TextTrimming.CharacterEllipsis;list.Items.Add(new ListBoxItem{Tag=device.Id,Content=label,ToolTip=device.Name+"\n"+device.Id,HorizontalContentAlignment=HorizontalAlignment.Stretch,Padding=new Thickness(8,9,8,9)});}
        deviceDetails=new StackPanel();Grid.SetColumn(deviceDetails,1);grid.Children.Add(list);grid.Children.Add(deviceDetails);page.Children.Add(grid);
        list.SelectionChanged+=(_,_)=>{if(list.SelectedItem is ListBoxItem item){selectedDeviceId=(string)item.Tag;RenderDeviceDetails();}};
        list.SelectedItem=list.Items.OfType<ListBoxItem>().FirstOrDefault(i=>(string)i.Tag==selectedDeviceId)??list.Items.OfType<ListBoxItem>().FirstOrDefault();
    }
    private void RenderDeviceDetails()
    {
        if(deviceDetails==null||sidebar.Latest==null||selectedDeviceId==null)return;
        var device=PinPlacement.Compose(sidebar.Latest,P).FirstOrDefault(d=>d.Id==selectedDeviceId);if(device==null)return;foreach(UIElement child in deviceDetails.Children)PreserveControlResources(child);deviceDetails.Children.Clear();int start=page.Children.Count;
        var title=Ui.Text(P.HardwareName(device.Id,device.Name),15);System.Windows.Automation.AutomationProperties.SetAutomationId(title,"DeviceNamePreview");title.FontWeight=FontWeights.SemiBold;title.TextWrapping=TextWrapping.Wrap;title.Margin=new Thickness(0,0,0,8);page.Children.Add(title);
        HardwareNameEditor(device);
        var enabledControl=Check("Gerät anzeigen",!P.HiddenDevices.Contains(device.Id)&&!P.HiddenCategories.Contains(device.Category),enabled=>{Toggle(P.HiddenDevices,device.Id,!enabled);if(enabled)P.HiddenCategories.RemoveAll(s=>s==device.Category);});System.Windows.Automation.AutomationProperties.SetAutomationId(enabledControl,"DeviceVisible:"+device.Id);
        if(device.Category=="Network"){
            var refresh=ActionButton(addressBusy?"IP-Adressen werden abgerufen …":"IP-Adressen abrufen / aktualisieren",()=>{});refresh.IsEnabled=!addressBusy;System.Windows.Automation.AutomationProperties.SetAutomationId(refresh,"RefreshNetworkAddresses");
            refresh.Click+=async(_,_)=>{
                if(addressBusy)return;addressBusy=true;refresh.IsEnabled=false;refresh.Content=SettingsText.Translate("IP-Adressen werden abgerufen …",P.SettingsLanguage);
                try{P.NetworkAddresses.TryGetValue(device.Id,out var previous);var result=await addressLookup.FetchAsync(device.Id,previous,P.IsMetricVisible(device.Id,device.Id+"/external-ip"),addressCancellation.Token);if(addressCancellation.IsCancellationRequested)return;P.NetworkAddresses[device.Id]=result.State;sidebar.Apply();if(result.ExternalFailed)ShowStatus("IP","Externe IP nicht erreichbar. Der bisherige Wert bleibt gespeichert.",Wpf.Ui.Controls.InfoBarSeverity.Warning);}
                catch(OperationCanceledException){}
                catch(Exception){if(!addressCancellation.IsCancellationRequested)ShowStatus("IP","IP-Abfrage fehlgeschlagen. Gespeicherte Werte bleiben erhalten.",Wpf.Ui.Controls.InfoBarSeverity.Warning);}
                finally{addressBusy=false;if(!addressCancellation.IsCancellationRequested&&current=="Geräte")RenderDeviceDetails();}
            };
            page.Children.Add(refresh);Note("IP-Adressen werden nur per Button abgerufen und gespeichert. Keine automatische Aktualisierung.");Note("Externe IP: api64.ipify.org über die Standardroute, nicht zwingend über den ausgewählten Adapter.");
        }
        Label("Sichtbare Werte");
        var own=P.Pins.Where(id=>(PinPlacement.Target(sidebar.Latest,P,id)?.Id??"custom")==device.Id).Distinct().ToHashSet();
        MetricChecks(device with{Metrics=device.Metrics.Where(m=>!own.Contains(m.Id)).ToList()});
        Label("Eigene Sensoren");
        if(own.Count==0)Note("Keine eigenen Sensoren hinzugefügt.");
        foreach(string id in own)
        {
            var metric=device.Metrics.FirstOrDefault(m=>m.Id==id)??new Metric(id,id,null,"");
            var controls=new StackPanel{Orientation=Orientation.Horizontal};var visible=new CheckBox{Content=metric.Display(P.Fahrenheit),IsChecked=P.IsMetricVisible(device.Id,id),VerticalAlignment=VerticalAlignment.Center};visible.Click+=(_,_)=>{P.SetMetricVisible(device.Id,id,visible.IsChecked==true);sidebar.Apply();};controls.Children.Add(visible);
            var remove=ActionButton("Entfernen",()=>{PinPlacement.SetPinned(sidebar.Latest,P,id,false);sidebar.Apply();RenderDeviceDetails();});remove.Margin=new Thickness(8,0,0,0);System.Windows.Automation.AutomationProperties.SetAutomationId(remove,"RemovePin:"+id);controls.Children.Add(remove);SettingRow(metric.Label,controls);
        }
        var add=ActionButton("Sensor hinzufügen",()=>{pendingTarget=device.Id;Navigate("Sensoren");});add.Margin=new Thickness(0,6,0,8);page.Children.Add(add);
        if(device.Category=="CPU")
        {
            Label("CPU-Temperaturquelle");var choices=new List<SensorChoice>{new("","Automatisch: CPU / Socket bevorzugen")};choices.AddRange(sidebar.Latest.Sensors.Where(s=>s.Kind=="Temperature"&&(s.HardwareKind is "SuperIO" or "Motherboard" or "EmbeddedController"||s.Id.StartsWith(device.Id+"/",StringComparison.Ordinal))).Select(s=>new SensorChoice(s.Id,s.Device+" · "+s.Name)));P.CpuTemperatureSources.TryGetValue(device.Id,out string? selected);
            Select("Temperatursensor",choices,choices.FirstOrDefault(c=>c.Id==selected)??choices[0],choice=>{if(choice.Id.Length==0)P.CpuTemperatureSources.Remove(device.Id);else P.CpuTemperatureSources[device.Id]=choice.Id;});
        }
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){PreserveControlResources(child);page.Children.Remove(child);deviceDetails.Children.Add(child);}SettingsText.Apply(this,P.SettingsLanguage);
    }
    // Keep scoped templates and their animation resources available when fields are moved or reused.
    private void PreserveControlResources(UIElement child){if(child is FrameworkElement element&&!element.Resources.MergedDictionaries.Contains(Resources))element.Resources.MergedDictionaries.Add(Resources);}
    private void Card(string title,Action build)
    {
        int start=page.Children.Count;build();var body=new StackPanel();var heading=Ui.Text(title,13);heading.FontWeight=FontWeights.SemiBold;heading.Margin=new Thickness(0,0,0,5);body.Children.Add(heading);
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){PreserveControlResources(child);page.Children.Remove(child);body.Children.Add(child);}
        var frame=new Wpf.Ui.Controls.Card{Content=body,Padding=new Thickness(14,8,14,8),Margin=new Thickness(0,0,0,8),BorderThickness=new Thickness(1),HorizontalContentAlignment=HorizontalAlignment.Stretch};frame.SetResourceReference(Control.BackgroundProperty,"Panel");frame.SetResourceReference(Control.BorderBrushProperty,"Edge");page.Children.Add(frame);
    }
    private void Section(string title,bool expanded,Action build)
    {
        int start=page.Children.Count;build();var body=new StackPanel {Margin=new Thickness(6,3,6,5)};
        var children=page.Children.Cast<UIElement>().Skip(start).ToArray();foreach(var child in children){PreserveControlResources(child);page.Children.Remove(child);body.Children.Add(child);}
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
    private void SelectPage(string name){if(name is "Werte" or "Eigene Werte")name="Geräte";var item=nav.MenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>().FirstOrDefault(i=>i.Tag as string==name);if(item!=null&&navigationReady){if(!nav.Navigate(name))Navigate(name);}else{foreach(var entry in nav.MenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>())entry.IsActive=entry==item;Navigate(name);}}
    private void TranslateNavigation(){foreach(var item in nav.MenuItems.OfType<Wpf.Ui.Controls.NavigationViewItem>())if(item.Tag is string key&&navigationLabels.TryGetValue(key,out var label))item.Content=SettingsText.Translate(label,P.SettingsLanguage);}
    private Button ActionButton(string text,Action click){var button=new Wpf.Ui.Controls.Button{Content=text,Padding=new Thickness(10,6,10,6)};button.Click+=(_,_)=>click();return button;}
    private void ShowStatus(string title,string message,Wpf.Ui.Controls.InfoBarSeverity severity){status.Title=SettingsText.Translate(title,P.SettingsLanguage);status.Message=SettingsText.Translate(message,P.SettingsLanguage);status.Severity=severity;status.IsOpen=true;}
    private sealed class SettingsPageProvider:Wpf.Ui.Abstractions.INavigationViewPageProvider{private readonly FrameworkElement content;public SettingsPageProvider(FrameworkElement content){this.content=content;}public object GetPage(Type pageType)=>content;}
    private sealed class OverviewSettingsPage:Page{} private sealed class PositionSettingsPage:Page{} private sealed class AppearanceSettingsPage:Page{} private sealed class TypographySettingsPage:Page{} private sealed class ColorsSettingsPage:Page{} private sealed class DevicesSettingsPage:Page{} private sealed class CardsSettingsPage:Page{} private sealed class LimitsSettingsPage:Page{} private sealed class SetupSettingsPage:Page{} private sealed class SystemSettingsPage:Page{}
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
            if(sidebar.Latest!=null)choices.AddRange(sidebar.Latest.Devices.Select(d=>new SensorChoice(d.Id,d.Category+" · "+P.HardwareName(d.Id,d.Name))));
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
        var pick=ActionButton("Auswählen",()=>{});pick.Margin=new Thickness(6,0,6,0);pick.HorizontalAlignment=HorizontalAlignment.Stretch;var swatch=new Border{Width=13,Height=13,Margin=new Thickness(0,0,6,0),BorderBrush=System.Windows.Media.Brushes.Gray,BorderThickness=new Thickness(1)};
        var content=new StackPanel{Orientation=Orientation.Horizontal};content.Children.Add(swatch);content.Children.Add(Ui.Text("Auswählen",12));pick.Content=content;
        bool refreshing=false;
        void Refresh(){refreshing=true;input.Text=get();swatch.Background=Ui.Brush(DesignCatalog.ColorOr(get(),fallback()));input.ClearValue(BorderBrushProperty);refreshing=false;}
        input.Tag=(Action)Refresh;
        input.TextChanged+=(_,_)=>{if(refreshing)return;string value=input.Text.Trim();if(value.Length>0&&DesignCatalog.ColorOr(value,"").Length==0){input.BorderBrush=Ui.Brush("#F47777");return;}set(value.Length==0?"":DesignCatalog.ColorOr(value,""));input.ClearValue(BorderBrushProperty);swatch.Background=Ui.Brush(DesignCatalog.ColorOr(get(),fallback()));sidebar.Apply();};
        pick.Click+=(_,_)=>{using var dialog=new Forms.ColorDialog{FullOpen=true,Color=System.Drawing.ColorTranslator.FromHtml(DesignCatalog.ColorOr(get(),fallback()))};if(dialog.ShowDialog(new FormsOwner(new System.Windows.Interop.WindowInteropHelper(this).Handle))==Forms.DialogResult.OK)input.Text=$"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";};
        var reset=ActionButton("↺",()=>{input.Text="";Refresh();});reset.ToolTip="Standardfarbe";System.Windows.Automation.AutomationProperties.SetAutomationId(reset,"Reset:"+id);
        grid.Children.Add(input);Grid.SetColumn(pick,1);grid.Children.Add(pick);Grid.SetColumn(reset,2);grid.Children.Add(reset);Refresh();SettingRow(label,grid);return input;
    }
    private sealed class FormsOwner:Forms.IWin32Window{public IntPtr Handle{get;}public FormsOwner(IntPtr handle){Handle=handle;}}
    private void DesignPicker()
    {
        Select("Farbschema",DesignCatalog.All,DesignCatalog.Get(P.Theme),design=>P.Theme=design.Key);
    }
    private void FontPicker()
    {
        var choices=new[]{""}.Concat(Ui.FontNames).Select(name=>new FontChoice(name)).ToArray();
        var picker=new ComboBox{ItemsSource=choices,SelectedItem=choices.FirstOrDefault(c=>c.Name.Equals(P.TextFontFamily,StringComparison.OrdinalIgnoreCase))??choices[0],HorizontalAlignment=HorizontalAlignment.Stretch,MaxWidth=440};
        System.Windows.Automation.AutomationProperties.SetAutomationId(picker,"TextFontFamily");
        picker.SelectionChanged+=(_,_)=>{if(picker.SelectedItem is FontChoice choice){P.TextFontFamily=choice.Name;sidebar.Apply();}};
        SettingRow("Schriftart (gesamter Text)",picker);
        picker.ToolTip=SettingsText.Translate("Gilt für Sidebar und Einstellungen. Neue Schriften zuerst in Windows installieren und die Sidebar neu starten.",P.SettingsLanguage);
    }
    private sealed record FontChoice(string Name)
    {
        public override string ToString()=>Name.Length==0?SettingsText.Translate("Standard (Design)",SettingsText.CurrentLanguage):Name;
    }
    private void HardwareNameEditor(Device device)
    {
        Label("Hardware-Name");
        var input=new TextBox{Text=P.HardwareNames.TryGetValue(device.Id,out var name)?name:"",MaxLength=160,MinWidth=80,HorizontalAlignment=HorizontalAlignment.Stretch,Padding=new Thickness(8,5,8,5)};
        System.Windows.Automation.AutomationProperties.SetAutomationId(input,"HardwareNameInput");
        var original=Ui.Text(device.Name,11,"Muted");original.TextWrapping=TextWrapping.Wrap;
        System.Windows.Automation.AutomationProperties.SetAutomationId(original,"OriginalHardwareName");
        page.Children.Add(original);page.Children.Add(input);
        Note("Leer lassen, um den Originalnamen zu verwenden. Mit Übernehmen oder Enter speichern.");
        void Save(bool reset){P.SetHardwareName(device.Id,reset?null:input.Text);sidebar.Apply();Navigate("Geräte");}
        input.KeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Enter){e.Handled=true;Save(false);}};
        var buttons=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,6,0,8)};
        var save=ActionButton("Übernehmen",()=>Save(false));System.Windows.Automation.AutomationProperties.SetAutomationId(save,"SaveHardwareName");buttons.Children.Add(save);
        var reset=ActionButton("Originalname wiederherstellen",()=>Save(true));reset.Margin=new Thickness(8,0,0,0);System.Windows.Automation.AutomationProperties.SetAutomationId(reset,"ResetHardwareName");buttons.Children.Add(reset);page.Children.Add(buttons);
    }
    private void LayoutPicker()
    {
        Select("Kartenlayout",CardLayouts.All,CardLayouts.Get(P.Layout),layout=>P.Layout=layout.Key);
    }
    private void StartupCheckbox()
    {
        var check=new Wpf.Ui.Controls.ToggleSwitch { OnContent=SettingsText.Translate("Ein",P.SettingsLanguage),OffContent=SettingsText.Translate("Aus",P.SettingsLanguage), IsChecked=ScheduledAutostart.Enabled };
        startupCheck=check;
        check.Click+=(_,_)=>{
            try { ScheduledAutostart.Request(check.IsChecked==true); }
            catch(System.ComponentModel.Win32Exception error) when(error.NativeErrorCode==1223) { }
            catch(Exception error) { ShowStatus("Autostart konnte nicht geändert werden",error.Message,Wpf.Ui.Controls.InfoBarSeverity.Error); }
            check.IsChecked=ScheduledAutostart.Enabled;
        };
        SettingRow("Autostart",check);
    }
    private static void Toggle(List<string> list,string id,bool present) { list.RemoveAll(s=>s==id); if(present) list.Add(id); }
    public void Refresh(Snapshot? snapshot)
    {
        if(current=="Position" && startupCheck!=null) startupCheck.IsChecked=ScheduledAutostart.Enabled;
        if(current is "Sensoren" or "Eigene Werte") UpdateSensors();
        if(current=="Voraussetzungen")
        {
            sourceStatus.Text=snapshot==null ? "Sensoren werden erkannt …" : SensorSources.Name(P.SensorSource)+" · "+snapshot.Sensors.Count+" "+SettingsText.Translate("Messwerte",P.SettingsLanguage)+" · "+snapshot.Time.ToString("HH:mm:ss");
            SettingsText.Apply(sourceStatus,P.SettingsLanguage);
        }
        if(current == "System")
        {
            bool admin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            diagnostics.Text = $"Sidebar by NemeSIS {typeof(App).Assembly.GetName().Version?.ToString()} · .NET {Environment.Version}\nSensorquelle: {SensorSources.Name(P.SensorSource)}\nGestartete EXE: {Environment.ProcessPath}\nZugriff: {(admin ? "Administrator" : "Standardbenutzer")}\n"+ScheduledAutostart.Describe()+"\n"+
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
            .Select(s=>new SensorDisplay(s.Id,s.Device,s.Name,s.Kind,new Metric(s.Id,s.Name,s.Value,s.Unit){IsCpuClock=s.Kind=="Clock"&&s.HardwareKind=="Cpu"}.Display(P.Fahrenheit),P.Pins.Contains(s.Id),ChangePin,sidebar.Latest!=null&&PinPlacement.Target(sidebar.Latest,P,s.Id) is Device device?device.Category+" · "+device.Name:"Separate Karte")).ToList();
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





