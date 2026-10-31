using System;
using System.Collections.Generic;
using System.Linq;
namespace Aurora;

public sealed record SensorSourceChoice(string Key,string Name){public override string ToString()=>Name;}
public static class SensorSources
{
    public const string Libre="LibreHardwareMonitor",OpenPawn="OpenHardwareMonitorPawnIO";
    public static readonly SensorSourceChoice[] All={new(Libre,"LibreHardwareMonitor + PawnIO"),new(OpenPawn,"Open Hardware Monitor + PawnIO")};
    public static string Normalize(string? source)=>source is OpenPawn or "OpenHardwareMonitor"?OpenPawn:Libre;
    public static string Name(string? source)=>All.First(s=>s.Key==Normalize(source)).Name;
    public static string OpenId(string identifier)=>"ohm/direct"+(identifier.StartsWith('/')?identifier:"/"+identifier);
    public static string BridgeId(string identifier)=>"ohm/pawn"+(identifier.StartsWith('/')?identifier:"/"+identifier);
    public static double? Finite(double? value)=>value.HasValue&&double.IsFinite(value.Value)?value:null;
    public static string HardwareKind(string kind)=>kind switch{"CPU"=>"Cpu","RAM"=>"Memory","HDD"=>"Storage","Mainboard"=>"Motherboard",_=>kind};
}
