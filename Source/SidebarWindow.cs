using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace Aurora;
public sealed class SidebarWindow : Window
{
    public Preferences Prefs { get; } = Preferences.Load();
    public Snapshot? Latest { get; private set; }
    private readonly SensorService service = new();
    private readonly NativeDock dock;
    private readonly Forms.NotifyIcon tray;
    private readonly Grid root = new();
    private readonly CardViewport scroll = new();
    private Grid cards=>scroll.Cards;
    private readonly TextBlock status = Ui.Text("SENSOREN WERDEN ERKANNT", 9, "Muted");
    private readonly Dictionary<string, IHardwareCard> views = new();
    private string shape = "";
    private SettingsWindow? settings;
    private bool placing, exiting;
    public SidebarWindow()
    {
        Title = "Sidebar by NemeSIS"; WindowStyle = WindowStyle.None; AllowsTransparency=true;ResizeMode = ResizeMode.NoResize;
        SetResourceReference(BackgroundProperty,"Back"); SetResourceReference(ForegroundProperty,"Text");
        ShowInTaskbar = true; Width = Prefs.ColumnWidth; Height = 800;
        UseLayoutRounding = true; SnapsToDevicePixels = true;
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        dock = new(this);
        root.Margin = new Thickness(2, 0, 2, 0);
        root.RowDefinitions.Add(new() { Height = GridLength.Auto });
        root.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var header = new Grid { Margin = new Thickness(4, 5, 2, 5) };
        header.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) }); header.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var gear = Ui.Button("⚙", OpenSettings); gear.ToolTip = "Settings"; gear.SetResourceReference(BackgroundProperty,"Chrome"); System.Windows.Automation.AutomationProperties.SetAutomationId(gear,"OpenSettings"); gear.Padding = new Thickness(6,2,6,2);
        var hide = Ui.Button("−", HideToTray); hide.ToolTip = "Hide to tray"; hide.SetResourceReference(BackgroundProperty,"Chrome"); hide.Padding = new Thickness(6,2,6,2); hide.Margin = new Thickness(5,0,0,0);
        actions.Children.Add(gear); actions.Children.Add(hide); Grid.SetColumn(actions, 1); header.Children.Add(actions); root.Children.Add(header);
        Grid.SetRow(scroll,1); root.Children.Add(scroll);
        Content = root;
        var menu = new ContextMenu();
        var m1 = new MenuItem { Header = "Settings" }; m1.Click += (_,_) => OpenSettings(); menu.Items.Add(m1);
        var m2 = new MenuItem { Header = "Hide" }; m2.Click += (_,_) => HideToTray(); menu.Items.Add(m2);
        var m3 = new MenuItem { Header = "Exit" }; m3.Click += (_,_) => Exit(); menu.Items.Add(m3); ContextMenu = menu;
        tray = new Forms.NotifyIcon { Text = "Sidebar by NemeSIS", Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application, Visible = true };
        tray.DoubleClick += (_,_) => Dispatcher.BeginInvoke(new Action(ShowSidebar));
        var trayMenu = new Forms.ContextMenuStrip();
        trayMenu.Items.Add("Show sidebar", null, (_,_) => Dispatcher.BeginInvoke(new Action(ShowSidebar)));
        trayMenu.Items.Add("Settings", null, (_,_) => Dispatcher.BeginInvoke(new Action(OpenSettings)));
        trayMenu.Items.Add("Exit", null, (_,_) => Dispatcher.BeginInvoke(new Action(Exit))); tray.ContextMenuStrip = trayMenu;
        SourceInitialized += (_,_) => { dock.Hook(Place); Apply(); };
        Loaded += (_,_) => { Apply(); service.Updated += OnSnapshot; service.Start(); };
        SizeChanged += (_,_) => ResizeCards();
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplayChanged;
        KeyDown += (_,e) => { if(e.Key == Key.Escape) HideToTray(); };
        Closed += (_,_) => { Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= DisplayChanged; service.Stop(); dock.Release(); tray.Dispose(); settings?.Close(); try { service.Worker?.Wait(2000); } catch { } };
    }
    private void DisplayChanged(object? sender, EventArgs e) => Dispatcher.BeginInvoke(new Action(Place));
    public void Apply()
    {
        ApplyTheme(); Topmost = Prefs.Topmost; Opacity = 1; service.Interval = Prefs.Interval;
        service.CpuTemperatureSources=new Dictionary<string,string>(Prefs.CpuTemperatureSources);
        Place(); shape = ""; if (Latest != null) Render(Latest);
        try { Prefs.Save(); } catch(Exception e) { status.Text = "Speichern fehlgeschlagen"; status.ToolTip = e.Message; }
    }
    private void ApplyTheme()
    {
        var d=DesignCatalog.Resolve(Prefs);
        var background=Ui.Brush(d.Back);background.Opacity=0;Application.Current.Resources["Back"]=background;
        var panel=Ui.Brush(d.Panel);panel.Opacity=Prefs.Opacity;Application.Current.Resources["Panel"]=panel;
        Application.Current.Resources["Chrome"]=Ui.Brush(d.Panel);
        Application.Current.Resources["Edge"]=Ui.Brush(d.Edge);
        Application.Current.Resources["Text"]=Ui.Brush(d.Text);
        Application.Current.Resources["Muted"]=Ui.Brush(d.Muted);
        Application.Current.Resources["HardwareName"]=Ui.Brush(DesignCatalog.ColorOr(Prefs.HardwareNameColor,d.Text));
        Application.Current.Resources["Accent"]=Ui.Brush(d.Accent);
    }
    private void Place()
    {
        if (placing || !IsLoaded) return;
        placing = true; try { dock.Place(Prefs); ResizeCards(); } finally { placing = false; }
    }
    private void ResizeCards()
    {
        double availableHeight=Math.Max(100,root.ActualHeight>0?root.ActualHeight-root.RowDefinitions[0].ActualHeight:ActualHeight-28);
        scroll.Configure(Prefs.Fit,Math.Max(160,ActualWidth-4),availableHeight,Prefs.Columns>1);
    }
    private void OnSnapshot(Snapshot snapshot)
    {
        if (Dispatcher.HasShutdownStarted || exiting) return;
        Dispatcher.BeginInvoke(new Action(() => { Latest = snapshot; Render(snapshot); settings?.Refresh(snapshot); }));
    }
    private void Render(Snapshot snapshot)
    {
        if (!Prefs.DeviceDefaultsApplied && snapshot.Devices.Count > 0)
        {
            foreach(var d in snapshot.Devices.Where(d=>!d.DefaultVisible)) if (!Prefs.HiddenDevices.Contains(d.Id)) Prefs.HiddenDevices.Add(d.Id);
            Prefs.DeviceDefaultsApplied = true;
            try { Prefs.Save(); } catch { }
        }
        var data = PinPlacement.Compose(snapshot,Prefs).Where(d => !Prefs.HiddenCategories.Contains(d.Category) && !Prefs.HiddenDevices.Contains(d.Id))
            .Select(d=>d with {Metrics=d.Metrics.Where(m=>Prefs.IsMetricVisible(d.Id,m.Id)).ToList()}).Where(d=>d.Metrics.Count>0).ToList();
        data = data.OrderBy(d => { int i = Prefs.Order.IndexOf(d.Category); return i < 0 ? 99 : i; }).ToList();
        var key = string.Join("|", data.Select(d => d.Id + ":" + string.Join(",",d.Metrics.Select(m=>m.Id+"="+m.Label))));
        if (shape != key)
        {
            shape = key; cards.Children.Clear(); cards.ColumnDefinitions.Clear(); views.Clear();
            Panel layout=Prefs.Columns>1?new HardwareCardGrid{Columns=Prefs.Columns}:new StackPanel();cards.Children.Add(layout);
            foreach(var d in data){var card=CardLayouts.Create(d,Prefs);views[d.Id]=card;layout.Children.Add(card.View);}
        }
        foreach (var d in data) if (views.TryGetValue(d.Id, out var view)) view.Update(d, Prefs);
        status.Text = snapshot.Time.ToString("HH:mm:ss") + " · " + snapshot.Devices.Count + " GERÄTE";
        status.ToolTip = snapshot.Warnings.Count > 0 ? string.Join("\n", snapshot.Warnings) : "Lokale Live-Abfragen · Keine Cloud-Verbindung";
        if (data.Count == 0 && cards.Children.Count == 0) cards.Children.Add(Ui.Text("No cards enabled.\nSelect devices in Settings ⚙.", 13, "Muted"));
        ResizeCards();
    }
    public void OpenSettings()
    {
        if (settings == null) { settings = new SettingsWindow(this); settings.Closed += (_,_) => settings = null; }
        settings.Show(); settings.Activate();
    }
    private void HideToTray() { dock.Release(); Hide(); }
    private void ShowSidebar() { Show(); Apply(); Activate(); }
    public void Restore() => ShowSidebar();
    public void RevealPinnedValues(string? sensorId=null){Dispatcher.BeginInvoke(new Action(()=>{string target=sensorId!=null&&Latest!=null?PinPlacement.Target(Latest,Prefs,sensorId)?.Id??"custom":"custom";if(views.TryGetValue(target,out var view))view.View.BringIntoView();}));}
    private void Exit() { exiting = true; Close(); Application.Current.Shutdown(); }
}

public static class Ui
{
    public static string SidebarLabel(string label)=>label switch{"Download"=>"DL","Upload"=>"UL","Adapter-Link" or "Adapter link" or "Link speed" or "Verbindung"=>"Link","Temperatur" or "Temperature"=>"Temp","Auslastung"=>"Load","Takt Ø" or "Average clock" or "Average Clock"=>"Clock","Takt"=>"Clock","Leistung"=>"Power","Spannung"=>"Voltage","Lüfter"=>"Fan","Belegt"=>"Used","Verfügbar"=>"Available","Frei"=>"Free","Gesamt"=>"Total","Kapazität"=>"Capacity","Lesen"=>"Read","Schreiben"=>"Write","VRAM belegt"=>"VRAM used","VRAM Last"=>"VRAM load","Verbleibende Lebensdauer"=>"Remaining life","Mainboard"=>"Motherboard","Custom"=>"Custom sensors","Controller"=>"Controllers",_=>label.Replace("Temperatur (","Temp (").Replace("Temperature (","Temp (").Replace("Nicht verfügbar","Unavailable")};
    public static string SidebarFullLabel(string label)=>label switch{"Temperatur" or "Temperature" or "Temp"=>"Temperature","Takt Ø" or "Average clock" or "Average Clock"=>"Average clock","Download" or "DL"=>"Download","Upload" or "UL"=>"Upload","Adapter-Link" or "Adapter link" or "Link speed" or "Link" or "Verbindung"=>"Adapter link speed",_=>SidebarLabel(label)};
    public static string HardwareName(string name)=>name.Replace("Physischer Arbeitsspeicher","Physical memory").Replace("Eigene Werte ohne Widget-Zuordnung","Unassigned custom readings").Replace("Deine angehefteten Sensoren","Pinned sensors");
    public static TextBlock SidebarText(string text,double size=13,string resource="Text"){var block=Text(SidebarLabel(text),size,resource);block.ToolTip=SidebarFullLabel(text);return block;}
    public static TextBlock HardwareTitle(string name,double size){var text=Text(HardwareName(name),size+1,"HardwareName");text.FontWeight=FontWeights.SemiBold;text.ToolTip=HardwareName(name);System.Windows.Automation.AutomationProperties.SetAutomationId(text,"HardwareName");return text;}
    public static SolidColorBrush ValueBrush(Preferences prefs,string fallback)=>Brush(DesignCatalog.ColorOr(prefs.ValueColor,fallback));
    public static SolidColorBrush Brush(string color) => (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
    public static TextBlock Text(string text, double size = 13, string resource = "Text") { var t = new TextBlock { Text = text, FontSize = size }; t.SetResourceReference(TextBlock.ForegroundProperty, resource); return t; }
    public static Button Button(string text, Action click) { var b = new Button { Content = text }; b.Click += (_,_) => click(); return b; }
    public static string Color(string category) => category switch { "CPU" => "#66DEC5", "GPU" => "#AC9BFA", "RAM" => "#78B9FC", "Drives" => "#F6BC70", "Network" => "#7DD9EC", _ => "#ADBBD0" };
    public static string Icon(string category) => category switch { "CPU" => "▦", "GPU" => "▣", "RAM" => "▥", "Drives" => "▰", "Network" => "⇅", "Custom" => "✧", _ => "◇" };
    public static FrameworkElement HardwareIcon(string category,double size)
    {
        string path=category switch {
            "CPU"=>"M5,5 L15,5 15,15 5,15 Z M8,8 L12,8 12,12 8,12 Z M7,1 L7,5 M10,1 L10,5 M13,1 L13,5 M7,15 L7,19 M10,15 L10,19 M13,15 L13,19 M1,7 L5,7 M1,10 L5,10 M1,13 L5,13 M15,7 L19,7 M15,10 L19,10 M15,13 L19,13",
            "RAM"=>"M2,5 L18,5 18,14 2,14 Z M5,7 L5,12 M8,7 L8,12 M11,7 L11,12 M14,7 L14,12 M4,14 L4,17 M7,14 L7,17 M10,14 L10,17 M13,14 L13,17 M16,14 L16,17",
            "GPU"=>"M2,2 L18,2 18,14 2,14 Z M10,14 L10,18 M6,18 L14,18",
            "Drives"=>"M3,6 L17,6 19,8 19,13 17,15 3,15 1,13 1,8 Z M4,11 L6,11 M15,11 L16,11",
            "Network"=>"M5,15 C0,15 0,8 5,8 C5,1 15,1 15,8 C21,8 21,15 16,15 M10,7 L10,18 M7,15 L10,18 13,15",
            _=>"M10,1 L19,10 10,19 1,10 Z M10,6 L10,14 M6,10 L14,10" };
        var icon=new System.Windows.Shapes.Path { Data=Geometry.Parse(path),Width=size,Height=size,StrokeThickness=1.15,Stretch=Stretch.Uniform,Margin=new Thickness(0,0,8,0),VerticalAlignment=VerticalAlignment.Center };
        icon.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty,"Text"); return icon;
    }
}

public sealed class HardwareCard : Border,IHardwareCard
{
    public FrameworkElement View=>this;
    private readonly Dictionary<string, (TextBlock Value, Border? Bar)> values = new();
    private readonly TextBlock hero;
    private readonly string color;
    private readonly SidebarDesign design;
    private readonly bool isOriginal;
    public HardwareCard(Device device, Preferences prefs)
    {
        design=DesignCatalog.Resolve(prefs);
        double readingScale=prefs.FontScale*CardLayouts.ReadingMultiplier(prefs.Layout);
        bool original=CardLayouts.BaseLayout(prefs.Layout)=="Original", compact=design.Layout is "Compact" or "Terminal";
        isOriginal=original;
        color = DesignCatalog.CategoryColor(design,device.Category,prefs);
        System.Windows.Documents.TextElement.SetFontFamily(this,new FontFamily(design.Font));
        CornerRadius = new CornerRadius(design.Radius);
        BorderThickness = design.Layout=="Minimal" ? new Thickness(0,0,0,1) : design.Layout=="Neon" ? new Thickness(3,1,1,1) : new Thickness(1);
        Padding = new Thickness(design.Padding,compact?7:original?10:11,design.Padding,compact?7:10); Margin = new Thickness(3,0,3,design.Gap);
        SetResourceReference(BackgroundProperty,"Panel"); SetResourceReference(BorderBrushProperty,"Edge");
        var stack = new StackPanel(); Child = stack;
        var head = new Grid();head.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});head.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        bool stackedHero=device.Category=="Network" && !original;
        if(stackedHero){head.RowDefinitions.Add(new(){Height=GridLength.Auto});head.RowDefinitions.Add(new(){Height=GridLength.Auto});}
        hero = Ui.Text("—", (compact?16:21)*prefs.FontScale); hero.FontWeight = FontWeights.SemiBold; hero.Foreground = Ui.ValueBrush(prefs,color); hero.HorizontalAlignment = HorizontalAlignment.Right;
        System.Windows.Automation.AutomationProperties.SetAutomationId(hero,"PrimaryMetric");
        if(original)hero.Visibility=Visibility.Collapsed;
        if(stackedHero){Grid.SetRow(hero,1);Grid.SetColumnSpan(hero,2);hero.Margin=new Thickness(0,5,0,0);}else Grid.SetColumn(hero,1);
        hero.TextTrimming=TextTrimming.CharacterEllipsis;head.Children.Add(hero);
        var title = Ui.Text(original?Ui.SidebarLabel(device.Category):Ui.SidebarLabel(device.Category).ToUpperInvariant(), (original?15:11)*prefs.FontScale); title.FontWeight = original?FontWeights.Normal:FontWeights.SemiBold; title.VerticalAlignment = VerticalAlignment.Center;
        var heading=new Grid {VerticalAlignment=VerticalAlignment.Center};heading.ColumnDefinitions.Add(new(){Width=GridLength.Auto});heading.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});heading.Children.Add(Ui.HardwareIcon(device.Category,(original?18:13)*prefs.FontScale));Grid.SetColumn(title,1);title.TextTrimming=TextTrimming.CharacterEllipsis;title.ToolTip=device.Category;heading.Children.Add(title);if(stackedHero)Grid.SetColumnSpan(heading,2);head.Children.Add(heading);stack.Children.Add(head);
        var subtitle = Ui.HardwareTitle(device.Name, (original?11:10)*prefs.FontScale); subtitle.TextTrimming = TextTrimming.CharacterEllipsis; subtitle.Margin = new Thickness(0,original?7:3,0,compact?4:7); subtitle.ToolTip = Ui.HardwareName(device.Name); stack.Children.Add(subtitle);
        IEnumerable<Metric> display=device.Metrics;
        if(original && device.Category!="Custom")
        {
            if(device.Category is "CPU" or "GPU")display=display.OrderBy(m=>m.Label=="Temperatur"?0:m.Label=="Auslastung"?1:2);
        }
        foreach(var m in display)
        {
            var row = new DockPanel { Margin = new Thickness(0,compact?1:2,0,compact?1:2) };
            var v = Ui.Text("—", (original?12:11)*readingScale); v.Foreground=Ui.ValueBrush(prefs,design.Text); v.TextTrimming=TextTrimming.CharacterEllipsis;v.FontFamily = new FontFamily(original?"Segoe UI":design.Layout=="Terminal"?"Consolas":"Cascadia Mono, Consolas"); DockPanel.SetDock(v,Dock.Right); row.Children.Add(v);row.SizeChanged+=(_,_)=>v.MaxWidth=Math.Max(50,row.ActualWidth*.65);
            string caption=original ? m.Label switch {"Temperatur"=>"Temp","Auslastung"=>"Load","Takt Ø"=>"Clock","Takt"=>"Core","Leistung"=>"Power","Spannung"=>"Volt","Lüfter"=>"Fan","Belegt"=>"Used","Verfügbar"=>"Available","Frei"=>"Free","Gesamt"=>"Total","Kapazität"=>"Total","Lesen"=>"Read","Schreiben"=>"Write","Download"=>"DL","Upload"=>"UL","Verbindung"=>"Link speed",_=>m.Label} : m.Label;
            var label = Ui.Text(Ui.SidebarLabel(caption), (original?12:11)*readingScale,"Muted"); label.TextTrimming = TextTrimming.CharacterEllipsis; label.Margin = new Thickness(0,0,9,0); label.ToolTip = Ui.SidebarFullLabel(m.Label); row.Children.Add(label); stack.Children.Add(row);
            Border? bar = null;
            if (m.Unit is "%" or "°C" && device.Category != "Custom")
            {
                var track = new Border { Height = design.BarHeight, Background = Ui.Brush(design.Track), CornerRadius = new CornerRadius(design.BarHeight/2), Margin = new Thickness(0,2,0,4) };
                bar = new Border { Height = design.BarHeight, Width = 0, Background = Ui.Brush(color), CornerRadius = new CornerRadius(design.BarHeight/2), HorizontalAlignment = HorizontalAlignment.Left };
                track.Child = bar; stack.Children.Add(track);
                var captured = bar; track.SizeChanged += (_,_) => { if (captured.Tag is double fraction) captured.Width = Math.Max(0,track.ActualWidth*fraction); };
            }
            values[m.Id] = (v,bar);
        }
    }
    public void Update(Device d, Preferences prefs)
    {
        var main = CardLayouts.MainMetric(d);
        hero.Text = main?.Display(prefs.Fahrenheit) ?? "—";
        bool alert = d.Metrics.Any(m => m.Value.HasValue && ((m.Unit == "°C" && m.Value >= prefs.TempLimit) || (m.Label == "Auslastung" && m.Value >= prefs.LoadLimit)));
        BorderBrush = Ui.Brush(alert?"#F47777":design.Layout=="Neon"?color:design.Edge);
        foreach(var m in d.Metrics)
        {
            if(!values.TryGetValue(m.Id,out var ui)) continue;
            ui.Value.Text = m.Display(prefs.Fahrenheit);
            ui.Value.ToolTip = m.Value.HasValue ? m.Display(prefs.Fahrenheit)+"\n"+(m.Source??m.Id) : "No reading is available for this sensor. Some sensors require administrator access.";
            if(ui.Bar != null)
            {
                var fraction = m.Value.HasValue ? Math.Clamp(m.Value.Value/Math.Max(1,m.Maximum),0,1) : 0;
                ui.Bar.Tag = fraction;
                ui.Bar.Width = Math.Max(0,((Border)ui.Bar.Parent).ActualWidth*fraction);
                string fill=color;
                if(isOriginal && !(prefs.GraphColors.TryGetValue(d.Category,out var customGraphColor)&&DesignCatalog.ColorOr(customGraphColor,"").Length>0))fill=m.Unit=="°C" ? d.Category=="Drives"?"#E5A527":"#C7EA35" : d.Category=="RAM"?"#A6E72F":"#43DF2C";
                ui.Bar.Background = Ui.Brush(m.Unit == "°C" && m.Value >= prefs.TempLimit ? "#F47777" : fill);
            }
        }
    }
}
