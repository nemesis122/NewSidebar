using Aurora;

static class TypographyChecks
{
    public static void Run(Action<bool,string> check)
    {
        var p=new Preferences{HardwareIconScale=1.75,FontScale=1.4,TitleTextScale=1.8,HardwareTextScale=.5,LabelTextScale=1.6,ValueTextScale=1.2,InstrumentTextScale=2,UnitTextScale=.7,ShowHardwareIcons=false,ShowWidgetTitles=false,ShowHardwareNames=false,ShowLabels=false,ShowReadings=false,ShowInstrumentNumbers=false,ShowInstrumentUnits=false,Theme="OLED",Layout="CyberPulse",Monitor="existing-monitor",InstrumentScale=1.6,CombineSensorCards=true};
        p.ResetTypography();
        check(new[]{p.HardwareIconScale,p.FontScale,p.TitleTextScale,p.HardwareTextScale,p.LabelTextScale,p.ValueTextScale,p.InstrumentTextScale,p.UnitTextScale}.All(v=>v==1),"Typography reset restores hardware icons and every text size to 100 percent");
        check(!p.ShowHardwareIcons&&!p.ShowWidgetTitles&&!p.ShowHardwareNames&&!p.ShowLabels&&!p.ShowReadings&&!p.ShowInstrumentNumbers&&!p.ShowInstrumentUnits,"Size reset preserves all explicit visibility choices");
        check(p.Theme=="OLED"&&p.Layout=="CyberPulse"&&p.Monitor=="existing-monitor"&&p.InstrumentScale==1.6&&p.CombineSensorCards,"Typography reset preserves the hardware design, monitor and common frame");
        p.HardwareIconScale=1.75;p.ShowAllTextAndIcons();
        check(p.ShowHardwareIcons&&p.ShowWidgetTitles&&p.ShowHardwareNames&&p.ShowLabels&&p.ShowReadings&&p.ShowInstrumentNumbers&&p.ShowInstrumentUnits,"Show all text also restores the hidden hardware icons");
        check(p.HardwareIconScale==1.75,"Show all text preserves the chosen icon size");
        p.Save();var saved=Preferences.Load();
        check(saved.ShowHardwareIcons&&saved.ShowHardwareNames&&saved.HardwareIconScale==1.75&&saved.FontScale==1&&saved.UnitTextScale==1,"Show-all visibility and retained size persist after restarting");
    }
}
