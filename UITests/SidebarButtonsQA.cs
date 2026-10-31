using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora;

static class SidebarButtonsQA
{
    public static void Verify(Action<bool,string> check,string folder)
    {
        foreach(var kind in new[]{SidebarActionIcon.Settings,SidebarActionIcon.Minimize})
        {
            int clicks=0;var button=SidebarActionButton.Create(kind,()=>clicks++);
            check(button.Content is System.Windows.Shapes.Path{Data:not null},"Sidebar action uses a real vector path: "+kind);
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));check(clicks==1,"Vector button retains its action: "+kind);
            check(!string.IsNullOrWhiteSpace(System.Windows.Automation.AutomationProperties.GetName(button)),"New button has a screenreader action name: "+kind);
            check(button.ToolTip!=null&&button.Width==32&&button.Height==32,"New icon has a consistent button target and tooltip: "+kind);
        }
        var design=DesignCatalog.Get("Midnight");
        var surface=new Border{Width=300,Padding=new Thickness(12),Background=Ui.Brush(design.Panel),BorderBrush=Ui.Brush(design.Edge),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(12),Resources=new ResourceDictionary{["Chrome"]=Ui.Brush(design.Panel),["Text"]=Ui.Brush(design.Text),["Edge"]=Ui.Brush(design.Edge),["Accent"]=Ui.Brush(design.Accent)}};
        var content=new StackPanel();surface.Child=content;var header=new Grid();header.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});header.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        var title=new TextBlock{Text="HW Sensors",FontSize=13,Foreground=Ui.Brush(design.Text),VerticalAlignment=VerticalAlignment.Center};header.Children.Add(title);
        var buttons=new StackPanel{Orientation=Orientation.Horizontal};var gear=SidebarActionButton.Create(SidebarActionIcon.Settings,()=>{});var minimize=SidebarActionButton.Create(SidebarActionIcon.Minimize,()=>{});minimize.Margin=new Thickness(6,0,0,0);buttons.Children.Add(gear);buttons.Children.Add(minimize);Grid.SetColumn(buttons,1);header.Children.Add(buttons);content.Children.Add(header);
        var reading=new Grid{Margin=new Thickness(0,16,0,2)};reading.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});reading.ColumnDefinitions.Add(new(){Width=GridLength.Auto});reading.Children.Add(new TextBlock{Text="CPU · Temperature",FontSize=12,Foreground=Ui.Brush(design.Muted)});var number=new TextBlock{Text="52 °C",FontSize=12,Foreground=Ui.Brush(design.Text)};Grid.SetColumn(number,1);reading.Children.Add(number);content.Children.Add(reading);
        surface.Measure(new Size(300,double.PositiveInfinity));surface.Arrange(new Rect(0,0,300,surface.DesiredSize.Height));surface.UpdateLayout();
        check(gear.ActualWidth==32&&minimize.ActualWidth==32&&gear.ActualHeight==32,"Actual sidebar controls render at the intended size");
        check(((System.Windows.Shapes.Path)gear.Content).ActualWidth==17&&((System.Windows.Shapes.Path)minimize.Content).ActualWidth==17&&((System.Windows.Shapes.Path)minimize.Content).ActualHeight>=16,"Both vector icons render inside the common 17-pixel icon grid");
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(surface.ActualWidth*2),(int)Math.Ceiling(surface.ActualHeight*2),192,192,PixelFormats.Pbgra32);bitmap.Render(surface);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using var output=File.Create(Path.Combine(folder,"Sidebar-New-Action-Icons.png"));png.Save(output);
    }
}
