# HW Sensors – Originaldesigns in einer gemeinsamen Kachel

Erweiterung in Sidebar by NemeSIS v2.21.2.4.

## Verwendung

1. Die bisherige Sidebar über das Tray-Menü beenden und das neue Paket in einen eigenen beschreibbaren Ordner entpacken.
2. Zum Übernehmen der bisherigen Auswahl eine Kopie der vorhandenen `aurora-settings.json` neben die EXE legen.
3. Unter **Einstellungen → Karten & Reihenfolge** den Schalter **Alle Werte in einer Kachel (HW Sensors)** einschalten.
4. Das Originaldesign wie bisher unter **Design & Schrift** wählen. Geräte, Werte, Sensoren, Monitor und Spalten weiterhin über die vorhandenen Einstellungen auswählen.

**HW Sensors ist ein gemeinsamer äußerer Rahmen.** CPU, GPU, RAM und Netzwerk behalten ihre eigenen originalen Bereiche, Überschriften, Hardware-Namen, Instrumente, Detailwerte und Verläufe. Es gibt keine gemeinsame Hauptzahl, keine neue Messwertliste und keine neue Designsammlung. Die IDs und Beschriftungen der Sensoren werden nicht verändert.

Alle 69 vorhandenen Designs werden durch ihre unveränderten Produktions-Karten dargestellt. Die einzelnen äußeren Kachelrahmen und Zwischenräume entfallen. Bei Fluent-Layouts bleibt der Originalinhalt erhalten, während die verschachtelte äußere Kartenhülle entfällt. HUD-Rahmendekorationen werden einmal an den gemeinsamen Außenrand versetzt. Kleine Kacheln und Konturen innerhalb der Original-Instrumente bleiben Teil des jeweiligen Designs.

Die gemeinsame Kachel übernimmt den Oberflächenstil des gewählten Designs. Die bestehende Ein- oder Mehrspaltenanordnung gilt auch innerhalb der gemeinsamen Kachel. Die vorhandene Geräte-/Werteauswahl, Sensor-Pins, Schrift- und Sichtbarkeitsregler, Hintergründe und Warngrenzen gelten weiterhin. Historien bleiben den ursprünglichen Geräten und Sensoren zugeordnet; beim Umschalten der Gruppierung werden sie nicht neu zugeordnet.

Der Schalter ist standardmäßig ausgeschaltet. Ausschalten stellt die einzelnen ursprünglichen Kachelrahmen wieder her. Der frühere Prototyp mit zusammengeführten Messwerten und der separate Editor für eigene Karten sind nicht Bestandteil dieser Fassung.

Das portable Windows-x64-Paket enthält .NET 8.0.33. Eine separate .NET-Installation ist nicht erforderlich. Die EXE ist wie die Ausgangsversion unsigniert.

## Textgrößen zurücksetzen

**Textgrößen zurücksetzen** stellt nur die sechs Textgrößen, die allgemeine Schriftgröße und die HW-Symbolgröße auf 100 % zurück. Die gewählte Sichtbarkeit bleibt erhalten. Das gilt auch für den kleinen Schrift-Reset unter Design & Schrift.

**Alle Texte anzeigen** schaltet alle sechs Textarten und ausdrücklich auch die HW-Symbole ein. Die gewählten Größen bleiben erhalten. Sämtliche Änderungen werden sofort gespeichert. Layout, Farben, Monitor, Instrumentgröße und HW-Sensors-Gruppierung bleiben erhalten.

## Neue Sidebar-Buttons

Die beiden oberen Sidebar-Buttons verwenden neue, skalierbare Linien-Vektoren: ein Zahnrad für Einstellungen und einen Pfeil ins Tray für das Ausblenden. Beide haben eine 32 × 32 Pixel große, abgerundete Schaltfläche mit Hover-, Tastaturfokus- und Druckzustand. Sie bleiben unabhängig von Hardware-Symbolgröße und Schriftwahl. Bei sehr schmaler Sidebar bleibt der Einstellungsbutton sichtbar; Ausblenden ist weiterhin auch im Kontextmenü möglich.

Diese Fassung meldet Version **2.21.2.4** in den Systeminformationen. Das Einstellungsmenü bleibt in der bisherigen Gestaltung. Zum Testen die alte Sidebar beenden und die neue EXE aus diesem Paket starten. Die bisherigen Designvorschläge sind nicht Teil der Anwendung.

## Intel-CPU-Takt korrigiert

Der bisherige Filter akzeptierte nur `Core #…` und `Cores (Average)`. Die eingebundene LibreHardwareMonitor-Version 0.9.6 benennt Intel-Kerntakte dagegen unter anderem `CPU Core #1`, `CPU Core` und auf Hybrid-CPUs `P-Core #1` / `E-Core #1`. Solche vorhandenen Werte wurden bisher übersprungen. Die OHM-Option verwendet bei CPUs dieselbe Libre/PawnIO-Brücke und war ebenfalls betroffen.

Der neue Filter erkennt diese Kerntakte, verwendet einen gültigen vorhandenen Durchschnitt bevorzugt und berechnet sonst das arithmetische Mittel der verfügbaren echten Kerntakte. Bus-/Referenz-, Speicher-, GPU- und effektive Takte werden nicht als Kerntakt ausgegeben. Fehlende, ungültige und initiale Nullwerte bleiben nicht verfügbar. Die bisherige Sensor-ID `/clocks-average` und damit vorhandene Sichtbarkeitseinstellungen bleiben erhalten. Der Tooltip nennt die verwendeten Sensoren oder erklärt, warum kein gültiger Kerntakt vorliegt.

Die Benennung ist im Quellstand der tatsächlich eingebundenen Bibliothek dokumentiert: [GenericCpu.CoreString](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/3d331e3370efb858411f19511373eff65a218701/LibreHardwareMonitorLib/Hardware/Cpu/GenericCpu.cs#L125-L131) und [Intel-Kerntaktsensoren](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/3d331e3370efb858411f19511373eff65a218701/LibreHardwareMonitorLib/Hardware/Cpu/IntelCpu.cs#L431-L437).

Die Korrektur wurde mit Sensor-Fixtures für beide Quellen geprüft. CPU-Takte werden jetzt mit zwei Nachkommastellen angezeigt, beispielsweise 4.11 GHz oder 0.80 GHz. Das gilt auch für einzeln angeheftete CPU-Kerntakte. GPU-Takte und andere Messwerte behalten ihre bisherige Formatierung. Alle 69 Originaldesigns wurden mit wechselnden CPU-Takten geprüft. Die bisherige Ganzzahlformatierung konnte unterschiedliche Taktdurchschnitte als denselben GHz-Wert darstellen. Gemessen wird der gemeldete Kerntakt, kein über Schlafzeiten gemittelter effektiver Takt.

## Prüfungen

Windows und .NET 8 SDK:

```powershell
dotnet build Source/SidebarByNemeSIS.csproj -c Release
dotnet run --project Tests/Checks.csproj -c Release
dotnet run --project UITests/LayoutQA.csproj -c Release -- --hw-sensors-only
dotnet run --project UITests/LayoutQA.csproj -c Release
dotnet run --project SensorTests/SensorChecks.csproj -c Release
```

Die gezielten UI-Prüfungen vergleichen für alle 69 Designs bei zwei Farbthemen und ein/zwei Spalten die gruppierten Bereiche mit den originalen Einzelkarten: Kartentyp, Texte, Werte, Schriftgrößen, Sichtbarkeit und Historienzuordnung müssen übereinstimmen. Zusätzlich werden gemeinsame Umrandung, Warnungen, Live-Aktualisierung und die tatsächliche Einstellungsoption geprüft.

Die Vorschau verwendet ausdrücklich simulierte Hardwaredaten. Diese Demo-Daten sind nicht Bestandteil der Anwendung. Die automatisierten Prüfungen ersetzen keinen Langzeit- oder Hardware-Kompatibilitätstest auf weiteren Rechnern.




## Weitere Hardware- und Funktionskorrekturen (2.21.2.4)

Die Libre-Option verwendet zusätzlich den eingebauten OHM-Laufwerkleser für sonst nicht erkannte physische Laufwerke. Das behebt den in der Live-Diagnose beobachteten fehlenden Samsung-PM981a-Datenträger. Bereits erkannte Libre-Laufwerke behalten ihre IDs und Messwerte; gleiche Modelle werden einzeln abgeglichen. Der Fallback ist über den Provider erkennbar. Er installiert keinen Treiber und startet kein externes Monitoringprogramm.

Bei der OHM-Option ergänzt die Libre-Brücke nun auch fehlende Werte und zusätzliche Sensoren bereits erkannter Geräte. Vorhandene OHM-Werte bleiben erhalten. Damit geht beispielsweise die in der Diagnose vorhandene Temperatur der Intel-SSD nicht mehr verloren.

Ein fehlender bevorzugter GPU-Takt verdeckt keinen verfügbaren Render-Takt mehr. Intel-D3D-3D-Auslastung wird ausdrücklich bevorzugt, sodass ein zuerst gelieferter Video-Decodierwert nicht als 3D-Last erscheint. Die VRAM-Skala nutzt die gemeldete Gesamtkapazität. Zeit-, Timing-, Geräusch- und Leitfähigkeitssensoren haben ihre Einheiten.

Mehrfaches Starten des Sensordienstes erzeugt keinen zweiten Worker. Bei vollständig ausgeblendeten Karten erscheint der vorhandene Leerzustand-Hinweis. Tests prüfen Start, Verbergen, Wiederherstellen, Einstellungen erneut öffnen und geordnetes Schließen mit Freigabe des Sensorzugriffs.

Der mitgelieferte APP-PRUEFBERICHT.txt unterscheidet Live-Messungen, Wiedergabe der aufgenommenen Sensordaten, automatisierte Funktionsprüfungen und nicht ausgeführte Systemänderungen. Hardware ohne bereitgestellte Sensoren bleibt nicht verfügbar. Ein universeller Nachweis auf allen Hardwareplattformen ist damit nicht verbunden.

