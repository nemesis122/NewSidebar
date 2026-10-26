using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora;
public sealed record SidebarDesign(string Key,string Name,string Description,string Back,string Panel,string Edge,string Text,string Muted,string Accent,string Track,double Radius=12,double Padding=13,double Gap=8,double BarHeight=3,string Layout="Modern",string Font="Segoe UI"){public override string ToString()=>Name;}
public static class DesignCatalog
{
    public static IReadOnlyList<SidebarDesign> All { get; }=new SidebarDesign[] {
        new("Original","Original","Wie deine bisherige HW Sidebar","#101115","#0E1014","#232C3B","#F1F5FC","#91B2D5","#4E9EFF","#1F232D",14,12,10,6,"Original"),
        new("Midnight","Midnight","Tiefblau mit Mint-Akzenten","#0B1018","#121B27","#263244","#EDF4FF","#94A7C0","#66DEC5","#283344"),
        new("Graphite","Graphite","Neutrales Anthrazit","#111214","#1C1E22","#343840","#F1F3F7","#A6B0C0","#9CAFEE","#30343C"),
        new("OLED","OLED Black","Echtes Schwarz, klare Kontraste","#000000","#080808","#2A2A2A","#FFFFFF","#A8A8B0","#64E1CA","#242429",8),
        new("Nord","Nord","Kühle, nordische Blautöne","#242933","#2E3440","#48566B","#ECEFF4","#B0BDCF","#88C0D0","#434C5E",10),
        new("Dracula","Dracula","Violett, Pink und sanftes Grün","#191821","#252433","#4C4565","#F8F8F2","#B5ACC9","#BD93F9","#423D55",14),
        new("Gruvbox","Gruvbox","Warme Retro-Farben","#1D2021","#282828","#665C54","#FBF1C7","#C3B698","#FABD2F","#504945",7),
        new("Solarized","Solarized Dark","Petrol mit goldenen Akzenten","#002B36","#073642","#365663","#EEE8D5","#A8B5B4","#E4BA53","#254B55",8),
        new("Terminal","Terminal","Eckig, monochrom und kompakt","#030A05","#06120B","#245C39","#B9FBC8","#8CCA9B","#6AFF9C","#163C23",0,10,6,3,"Terminal","Consolas"),
        new("Neon","Neon","Leuchtende Akzente auf Indigo","#080613","#151025","#473168","#F7F1FF","#B7A6D2","#DC8EFF","#36264D",10,13,9,4,"Neon"),
        new("Compact","Compact","Kleine Abstände, mehr Platz","#10151D","#171F29","#334150","#EEF5FC","#A1B4C8","#73C7F3","#2A3948",6,8,5,3,"Compact"),
        new("Minimal","Minimal","Flache Zeilen ohne Kartenrahmen","#14171C","#14171C","#3A424E","#F0F2F5","#A5B0BE","#A4B8D4","#303641",0,9,12,2,"Minimal"),
        new("Light","Light","Helle Karten mit dunkler Schrift","#EDF2F6","#FFFFFF","#CBD6E2","#172D42","#586D83","#007968","#E3EAF0",12),
        new("Arctic","Arctic","Eisblau und sachlich","#DFEAF4","#F5FAFF","#B8CCDF","#152F49","#49647E","#1765A5","#D6E5F1",5)
    };
    public static SidebarDesign Get(string? key)=>All.FirstOrDefault(d=>d.Key==key) ?? All.First(d=>d.Key=="Midnight");
    public static string ColorOr(string? value,string fallback)=>value!=null&&System.Text.RegularExpressions.Regex.IsMatch(value.Trim(),"^#[0-9a-fA-F]{6}$")?value.Trim().ToUpperInvariant():fallback;
    public static SidebarDesign Resolve(Preferences p){var d=Get(p.Theme);return d with{Text=ColorOr(p.TextColor,d.Text),Muted=ColorOr(p.LabelColor,d.Muted),Track=ColorOr(p.GraphTrackColor,d.Track),BarHeight=d.BarHeight*GraphSize(p)};}
    public static double GraphSize(Preferences p)=>double.IsFinite(p.GraphScale)?Math.Clamp(p.GraphScale,.5,1.6):1;
    public static string CategoryColor(SidebarDesign d,string category,Preferences? prefs=null)
    {
        if(prefs?.GraphColors!=null&&prefs.GraphColors.TryGetValue(category,out var custom)&&ColorOr(custom,"") is string valid&&valid.Length>0)return valid;
        if(d.Key=="Terminal")return d.Accent;
        if(d.Key is "Light" or "Arctic")return category switch {"CPU"=>"#087D67","RAM"=>"#2568A3","GPU"=>"#7351B9","Drives"=>"#A45C0B","Network"=>"#006E84",_=>d.Accent};
        if(d.Key=="Nord")return category switch {"CPU"=>"#A3BE8C","RAM"=>"#81A1C1","GPU"=>"#B48EAD","Drives"=>"#EBCB8B","Network"=>"#88C0D0",_=>d.Accent};
        if(d.Key=="Gruvbox")return category switch {"CPU"=>"#B8BB26","RAM"=>"#83A598","GPU"=>"#D3869B","Drives"=>"#FABD2F","Network"=>"#8EC07C",_=>d.Accent};
        return category switch {"CPU"=>"#66DEC5","RAM"=>"#78B9FC","GPU"=>"#AC9BFA","Drives"=>"#F6BC70","Network"=>"#7DD9EC",_=>d.Accent};
    }
}
