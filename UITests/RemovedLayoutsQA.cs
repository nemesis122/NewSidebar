using Aurora;
static class RemovedLayoutsQA
{
 public static void Verify(Action<bool,string> check)
 {
  string[] removed={"ApertureGrid","Bento","BentoLive","BridgeRows","ChronoDots","ColumnFilm","CornerTiles","DataSpine","EqualizerDeck","FaderBank","LayerStack","MarginNotes","MicroGrid","PixelLedger","RadarMesh","RangeDeck","RingMatrix","RulerTracks"};
  var sample=new Device("test/cpu","CPU","Test CPU",new(){new("temp","Temperatur",42,"°C")});
  foreach(var key in removed){check(!CardLayouts.All.Any(l=>l.Key==key),"Removed layout is absent from selector: "+key);check(CardLayouts.Get(key).Key=="Original"&&CardLayouts.Get(key+".Large").Key=="Original","Removed saved layouts resolve safely: "+key);check(CardLayouts.Create(sample,new(){Layout=key}) is HardwareCard,"Removed layout renders the Original fallback: "+key);}
  check(CardLayouts.All.Count==97,"Exactly18 layouts removed;91 remain plus6 v3.01 glass designs");
 }
}
