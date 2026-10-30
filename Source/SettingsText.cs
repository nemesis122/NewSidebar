using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace Aurora;
public record SettingsLanguage(string Key,string Name){public override string ToString()=>Name;}
public static class SettingsText
{
 public static string CurrentLanguage="de";
 public static SettingsLanguage[] Languages={new("de","Deutsch"),new("en","English"),new("fr","Français"),new("it","Italiano"),new("es","Español"),new("pt","Português")};
 private static readonly Dictionary<string,string[]> Terms=Data.Split('\n',StringSplitOptions.RemoveEmptyEntries).Select(line=>line.Trim().Split('|')).Where(parts=>parts.Length==6).ToDictionary(parts=>parts[0],parts=>parts.Skip(1).ToArray());
 private static readonly string[] Ordered=Terms.Keys.OrderByDescending(s=>s.Length).ToArray();
 private sealed class State{public State(){}public Dictionary<string,(string Source,string Rendered)> Values=new();}
 private static readonly ConditionalWeakTable<DependencyObject,State> States=new();
 public static string Translate(string text,string language)
 {
  int index=Array.FindIndex(Languages,l=>l.Key==language)-1;if(index<0)return text;
  if(Terms.TryGetValue(text,out var exact))return exact[index];
  foreach(string source in Ordered)if(text.Contains(source,StringComparison.Ordinal))text=Regex.Replace(text,@"(?<![\p{L}\p{N}_])"+Regex.Escape(source)+@"(?![\p{L}\p{N}_])",_=>Terms[source][index]);
  return text;
 }
 public static void Apply(DependencyObject root,string language)
 {
  var visited=new HashSet<DependencyObject>();
  void Value(DependencyObject node,string slot,string current,Action<string> set)
  {var values=States.GetOrCreateValue(node).Values;if(!values.TryGetValue(slot,out var old)||current!=old.Rendered)old=(current,current);string translated=Translate(old.Source,language);values[slot]=(old.Source,translated);if(translated!=current)set(translated);}
  void Visit(DependencyObject node)
  {
   if(!visited.Add(node))return;
   if(node is FrameworkElement literal && System.Windows.Automation.AutomationProperties.GetAutomationId(literal) is "DeviceNamePreview" or "OriginalHardwareName")return;
   if(node is TextBlock text)Value(node,"Text",text.Text,v=>text.Text=v);
   if(node is ContentControl content&&content.Content is string caption)Value(node,"Content",caption,v=>content.Content=v);
   if(node is FrameworkElement element&&element.ToolTip is string tip)Value(node,"ToolTip",tip,v=>element.ToolTip=v);
   if(node is Window window)Value(node,"Title",window.Title,v=>window.Title=v);
   if(node is ListView list&&list.View is GridView grid)foreach(var column in grid.Columns)if(column.Header is string header)Value(column,"Header",header,v=>column.Header=v);
   foreach(var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().ToArray())Visit(child);
   if(node is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(node);i++)Visit(VisualTreeHelper.GetChild(node,i));
  }
  Visit(root);
 }
 public static DataTemplate ChoiceTemplate()
 {var text=new FrameworkElementFactory(typeof(TextBlock));text.SetBinding(TextBlock.TextProperty,new Binding{Converter=new ChoiceConverter()});text.SetValue(TextBlock.TextTrimmingProperty,TextTrimming.CharacterEllipsis);return new DataTemplate{VisualTree=text};}
 private sealed class ChoiceConverter:IValueConverter
 {public object Convert(object value,Type target,object parameter,CultureInfo culture)=>Translate(value?.ToString()??"",CurrentLanguage);public object ConvertBack(object value,Type target,object parameter,CultureInfo culture)=>Binding.DoNothing;}
 private const string Data="""
Die Libre-Option ergänzt nicht erkannte Laufwerke automatisch mit dem zweiten eingebauten Leser.|The Libre option automatically adds undetected drives using the second built-in reader.|L'option Libre ajoute automatiquement les disques non détectés avec le second lecteur intégré.|L'opzione Libre aggiunge automaticamente le unità non rilevate con il secondo lettore integrato.|La opción Libre añade automáticamente las unidades no detectadas con el segundo lector integrado.|A opção Libre adiciona automaticamente as unidades não detetadas com o segundo leitor integrado.
Text- und Hardware-Symbolgrößen auf 100 % zurücksetzen|Reset text and hardware icon sizes to 100%|Rétablir la taille des textes et icônes à 100 %|Ripristina le dimensioni di testo e icone al 100%|Restablecer tamaños de texto e iconos al 100 %|Repor os tamanhos de texto e ícones em 100%
Alle Texte und Hardware-Symbole anzeigen|Show all text and hardware icons|Afficher tous les textes et icônes matérielles|Mostra tutti i testi e le icone hardware|Mostrar todos los textos e iconos de hardware|Mostrar todos os textos e ícones de hardware
Alle Werte in einer Kachel (HW Sensors)|All readings in one card (HW Sensors)|Toutes les mesures dans une carte (HW Sensors)|Tutti i valori in una scheda (HW Sensors)|Todos los valores en una tarjeta (HW Sensors)|Todos os valores num cartão (HW Sensors)
Die Hardwarebereiche behalten ihr Originaldesign mit eigenen Instrumenten und Verläufen. Nur der äußere Rahmen wird zu HW Sensors zusammengefasst.|Hardware sections retain their original design, instruments and histories. Only the outer frame is shared as HW Sensors.|Les sections matérielles conservent leur design, instruments et historiques. Seul le cadre extérieur est partagé sous HW Sensors.|Le sezioni hardware mantengono design, strumenti e cronologie originali. Solo il bordo esterno è condiviso come HW Sensors.|Las secciones conservan su diseño, instrumentos e historiales originales. Solo se comparte el marco exterior como HW Sensors.|As secções mantêm o design, instrumentos e históricos originais. Apenas a moldura exterior é partilhada como HW Sensors.
Hardware-Symbole anzeigen|Show hardware icons|Afficher les icônes matérielles|Mostra icone hardware|Mostrar iconos de hardware|Mostrar ícones de hardware
Hardware-Symbole|Hardware icons|Icônes matérielles|Icone hardware|Iconos de hardware|Ícones de hardware
Beim ersten Wechsel werden passende Geräte und Werte mit deiner bisherigen Auswahl übernommen. Danach bleiben Änderungen je Quelle erhalten. RAM und Netzwerk verwenden Windows.|On first switching, matching devices and readings inherit your existing selection. Later changes remain source-specific. RAM and network use Windows.|Au premier changement, les appareils et mesures correspondants reprennent votre sélection. Les changements suivants restent propres à chaque source. La mémoire et le réseau utilisent Windows.|Al primo cambio, dispositivi e valori corrispondenti ereditano la selezione esistente. Le modifiche successive restano per sorgente. RAM e rete usano Windows.|En el primer cambio, los dispositivos y valores coincidentes heredan tu selección. Los cambios posteriores se conservan por fuente. RAM y red usan Windows.|Na primeira mudança, os dispositivos e leituras correspondentes herdam a seleção existente. Alterações posteriores ficam por fonte. RAM e rede utilizam o Windows.
Sensorquelle|Sensor source|Source des capteurs|Sorgente sensori|Fuente de sensores|Fonte dos sensores
Sensorerkennung|Sensor detection|Détection des capteurs|Rilevamento sensori|Detección de sensores|Detecção de sensores
Sensorquelle wird gewechselt …|Switching sensor source …|Changement de source …|Cambio sorgente sensori …|Cambiando fuente de sensores …|A mudar a fonte dos sensores …
Beide Optionen sind eingebaut. Der Wechsel erfolgt im Hintergrund. Kein separates Programm erforderlich.|Both options are built in. Switching runs in the background. No separate application is required.|Les deux options sont intégrées. Le changement se fait en arrière-plan. Aucune application séparée requise.|Entrambe le opzioni sono integrate. Il cambio avviene in background. Non serve un programma separato.|Ambas opciones están integradas. El cambio se realiza en segundo plano. No se necesita un programa separado.|Ambas as opções estão integradas. A mudança ocorre em segundo plano. Não é necessário um programa separado.
Open Hardware Monitor + PawnIO: GPU und SMART über Open Hardware Monitor, CPU und Mainboard über die LibreHardwareMonitor/PawnIO-Brücke. Nicht unterstützte Geräte verwenden Libre als Ersatz.|Open Hardware Monitor + PawnIO: GPU and SMART via Open Hardware Monitor, CPU and motherboard via the LibreHardwareMonitor/PawnIO bridge. Unsupported devices use Libre as a fallback.|Open Hardware Monitor + PawnIO : GPU et SMART via Open Hardware Monitor, CPU et carte mère via la passerelle LibreHardwareMonitor/PawnIO. Libre sert de repli pour les autres périphériques.|Open Hardware Monitor + PawnIO: GPU e SMART tramite Open Hardware Monitor, CPU e scheda madre tramite il ponte LibreHardwareMonitor/PawnIO. Libre gestisce i dispositivi non supportati.|Open Hardware Monitor + PawnIO: GPU y SMART mediante Open Hardware Monitor, CPU y placa base mediante el puente LibreHardwareMonitor/PawnIO. Libre cubre los dispositivos no compatibles.|Open Hardware Monitor + PawnIO: GPU e SMART pelo Open Hardware Monitor, CPU e placa-mãe pela ponte LibreHardwareMonitor/PawnIO. Libre serve de alternativa para dispositivos não suportados.
Ohne WinRing0 und InpOut. Beide Optionen benötigen PawnIO für erweiterte CPU- und Mainboard-Werte.|No WinRing0 or InpOut. Both options require PawnIO for advanced CPU and motherboard readings.|Sans WinRing0 ni InpOut. Les deux options nécessitent PawnIO pour les mesures avancées CPU et carte mère.|Senza WinRing0 e InpOut. Entrambe le opzioni richiedono PawnIO per i valori avanzati di CPU e scheda madre.|Sin WinRing0 ni InpOut. Ambas opciones requieren PawnIO para las lecturas avanzadas de CPU y placa base.|Sem WinRing0 nem InpOut. Ambas as opções requerem PawnIO para leituras avançadas de CPU e placa-mãe.
Sensorzuordnungen werden je Quelle getrennt behalten. RAM und Netzwerk verwenden Windows.|Sensor assignments are kept separately for each source. RAM and network use Windows.|Les attributions sont conservées par source. La mémoire et le réseau utilisent Windows.|Le assegnazioni sono salvate per sorgente. RAM e rete usano Windows.|Las asignaciones se conservan por fuente. RAM y red usan Windows.|As atribuições são mantidas por fonte. RAM e rede utilizam o Windows.

Anzeigen|Show|Afficher|Mostra|Mostrar|Mostrar
Alle Texte anzeigen|Show all text|Afficher tous les textes|Mostra tutti i testi|Mostrar todos los textos|Mostrar todos os textos
Asymmetrische Datenkacheln mit echtem Sensorverlauf|Asymmetric data tiles with real sensor history|Tuiles asymétriques avec historique réel|Riquadri asimmetrici con cronologia reale|Mosaicos asimétricos con historial real|Blocos assimétricos com histórico real
Transparente Kanten und ein lineares Glas-Instrument|Transparent edges and a linear glass instrument|Bords transparents et instrument en verre linéaire|Bordi trasparenti e strumento lineare in vetro|Bordes transparentes e indicador lineal de cristal|Bordas transparentes e indicador linear de vidro
Breite Verlaufskurve mit klaren Datenfeldern|Wide history chart with clear data fields|Historique large et champs de données clairs|Grafico ampio con campi dati chiari|Historial amplio con campos de datos claros|Histórico amplo com campos de dados claros
Schmale Widgets mit mehreren vertikalen Sensorpegeln|Slim widgets with multiple vertical sensor meters|Widgets fins avec plusieurs jauges verticales|Widget stretti con più indicatori verticali|Widgets estrechos con varios medidores verticales|Widgets estreitos com vários medidores verticais
Große Hauptanzeige mit kompakten Sensorfeldern und Verläufen|Large primary reading with compact sensor fields and histories|Grande valeur principale avec champs et historiques compacts|Valore principale grande con campi e cronologie compatte|Valor principal grande con campos e historiales compactos|Valor principal grande com campos e históricos compactos
Tiefschwarze Karten mit großer Typografie und feinem Pegel|Deep black cards with large typography and a fine meter|Cartes noires avec grande typographie et jauge fine|Schede nere con testo grande e indicatore sottile|Tarjetas negras con texto grande y medidor fino|Cartões pretos com texto grande e medidor fino
Schwarz-weiße Instrumente mit schlichten Sensorbalken|Black-and-white instruments with clean sensor bars|Instruments noir et blanc avec barres sobres|Strumenti in bianco e nero con barre semplici|Instrumentos en blanco y negro con barras simples|Instrumentos a preto e branco com barras simples
Türkise Anzeigen mit violetten Details und Instrument rechts|Cyan displays with violet details and a right-hand dial|Affichages cyan, détails violets et cadran à droite|Indicatori ciano, dettagli viola e quadrante a destra|Indicadores cian, detalles violeta y dial a la derecha|Indicadores ciano, detalhes violeta e mostrador à direita
Blau-violette Instrumente mit echtem Balkenverlauf|Blue-violet instruments with real history bars|Instruments bleu-violet et historique réel en barres|Strumenti blu-viola e barre con cronologia reale|Instrumentos azul-violeta y barras de historial real|Instrumentos azul-violeta e barras de histórico real
Kompakte Hardware-Panels mit Sensor-Pegeln|Compact hardware panels with sensor meters|Panneaux matériels compacts avec jauges|Pannelli hardware compatti con indicatori|Paneles de hardware compactos con medidores|Painéis de hardware compactos com medidores
Schwarze Sportinstrumente mit roten Akzenten|Black sports instruments with red accents|Instruments sportifs noirs avec accents rouges|Strumenti sportivi neri con accenti rossi|Instrumentos deportivos negros con acentos rojos|Instrumentos desportivos pretos com detalhes vermelhos
Moderne Datenkacheln mit Matrix-Instrument|Modern data tiles with a matrix instrument|Tuiles de données modernes et instrument matriciel|Riquadri dati moderni e strumento a matrice|Mosaicos de datos modernos e indicador matricial|Blocos de dados modernos e indicador matricial
Vertikaler Farbpegel mit klaren Messwerten|Vertical color meter with clear readings|Jauge verticale colorée et mesures claires|Indicatore verticale a colori con valori chiari|Medidor vertical a color con lecturas claras|Medidor vertical a cores com leituras claras
Große Hauptanzeige mit kompaktem Live-Verlauf|Large primary reading with compact live history|Grande mesure principale et historique compact|Valore principale grande con cronologia compatta|Valor principal grande con historial compacto|Valor principal grande com histórico compacto
Feine Sensorringe auf ruhigen Kartenflächen|Fine sensor rings on clean card surfaces|Anneaux fins sur cartes épurées|Anelli sottili su schede pulite|Anillos finos en tarjetas limpias|Anéis finos em cartões simples
Feines Rundinstrument auf getönten Glasflächen|Fine dial on tinted glass surfaces|Cadran fin sur surfaces vitrées teintées|Quadrante sottile su superfici in vetro colorato|Dial fino sobre superficies de cristal tintado|Mostrador fino sobre superfícies de vidro colorido
Echter Sensorverlauf mit klaren Datenzeilen|Real sensor history with clear reading rows|Historique réel avec lignes de mesures claires|Cronologia reale con righe di valori chiare|Historial real con filas de lecturas claras|Histórico real com linhas de leituras claras
Kompakte Typografie mit präzisem Statusbalken|Compact typography with a precise status bar|Typographie compacte et barre précise|Tipografia compatta e barra precisa|Tipografía compacta y barra precisa|Tipografia compacta e barra precisa
Kantiges Systempanel mit mehrfarbigen Segmentinstrumenten|Angular system panel with multicolor segment instruments|Panneau angulaire avec instruments segmentés multicolores|Pannello angolare con strumenti segmentati multicolore|Panel angular con indicadores segmentados multicolor|Painel angular com instrumentos segmentados multicolor
Neonrahmen mit großem Rundinstrument und Werteblöcken|Neon frame with large dial and reading blocks|Cadre néon avec grand cadran et blocs de mesures|Cornice neon con grande quadrante e blocchi di valori|Marco neón con dial grande y bloques de lecturas|Moldura néon com mostrador grande e blocos de leituras
Graph oben, Werte unten – auch bei einer Spalte.|Graph above, readings below — also with one column.|Graphique en haut, mesures en dessous — aussi sur une colonne.|Grafico sopra, valori sotto — anche con una colonna.|Gráfico arriba, lecturas debajo — también con una columna.|Gráfico acima, leituras abaixo — também com uma coluna.
Metallrahmen mit segmentiertem Sensorinstrument|Metal frame with segmented sensor instrument|Cadre métallique et instrument segmenté|Cornice metallica con strumento segmentato|Marco metálico con indicador segmentado|Moldura metálica com indicador segmentado
Neonrahmen mit Rundinstrument und echtem Verlauf|Neon frame with dial and real history|Cadre néon avec cadran et historique réel|Cornice neon con quadrante e cronologia reale|Marco neón con dial e historial real|Moldura néon com mostrador e histórico real
Maximal kompakte Messkarte mit klaren Datenzeilen|Ultra-compact card with clean reading rows|Carte ultra-compacte aux mesures claires|Scheda ultra-compatta con valori chiari|Tarjeta ultracompacta con lecturas claras|Cartão ultracompacto com leituras claras
Asymmetrische Messwert-Kacheln|Asymmetric reading tiles|Tuiles de mesures asymétriques|Riquadri asimmetrici delle misure|Mosaicos asimétricos de lecturas|Mosaicos assimétricos de leituras
Verbundene Sensorfelder um einen Chip|Connected sensor fields around a chip|Champs de capteurs reliés à une puce|Campi sensore collegati a un chip|Campos de sensores conectados a un chip|Campos de sensores ligados a um chip
Geteilte Hardware- und Datenflächen|Split hardware and data surfaces|Surfaces séparées pour matériel et données|Superfici separate per hardware e dati|Superficies separadas de hardware y datos|Superfícies separadas de hardware e dados
Große Typografie mit ruhiger Werteliste|Large typography with a clean reading list|Grande typographie et liste de mesures claire|Tipografia grande e lista di valori chiara|Tipografía grande y lista de valores clara|Tipografia grande e lista de valores clara
Technische Zeichnung mit Messfeldern|Technical drawing with reading fields|Dessin technique avec champs de mesures|Disegno tecnico con campi di misura|Dibujo técnico con campos de lectura|Desenho técnico com campos de leitura
Weiche Flächen und klare Typografie|Soft surfaces and clear typography|Surfaces douces et typographie claire|Superfici morbide e testo chiaro|Superficies suaves y tipografía clara|Superfícies suaves e tipografia clara
Transparente Karten mit feinen Konturen|Transparent cards with fine outlines|Cartes transparentes aux contours fins|Schede trasparenti con contorni sottili|Tarjetas transparentes con bordes finos|Cartões transparentes com contornos finos
Kompakte Messwert-Kacheln|Compact reading tiles|Tuiles de mesures compactes|Riquadri compatti delle misure|Mosaicos compactos de lecturas|Mosaicos compactos de leituras
Präzise Skala und ausgerichtete Werte|Precise scale and aligned readings|Échelle précise et mesures alignées|Scala precisa e valori allineati|Escala precisa y valores alineados|Escala precisa e valores alinhados
Lichtkonturen mit geometrischem Instrument|Light outlines with a geometric instrument|Contours lumineux et instrument géométrique|Bordi luminosi e strumento geometrico|Bordes luminosos e instrumento geométrico|Contornos luminosos e instrumento geométrico
Orbit-Instrument mit seitlichen Messwerten|Orbital instrument with side readings|Instrument orbital et mesures latérales|Strumento orbitale e misure laterali|Instrumento orbital y lecturas laterales|Instrumento orbital e leituras laterais
Schriftart (gesamter Text)|Font family (all text)|Police (tout le texte)|Carattere (tutto il testo)|Fuente (todo el texto)|Tipo de letra (todo o texto)
Standard (Design)|Default (design)|Par défaut (style)|Predefinito (design)|Predeterminada (diseño)|Predefinido (design)
Gilt für Sidebar und Einstellungen. Neue Schriften zuerst in Windows installieren und die Sidebar neu starten.|Applies to the sidebar and settings. Install new fonts in Windows first, then restart the sidebar.|S’applique à la barre et aux paramètres. Installez les nouvelles polices dans Windows, puis redémarrez la barre.|Si applica alla barra e alle impostazioni. Installa nuovi caratteri in Windows e riavvia la barra.|Se aplica a la barra y a los ajustes. Instala nuevas fuentes en Windows y reinicia la barra.|Aplica-se à barra e às definições. Instale novas fontes no Windows e reinicie a barra.
Hardware-Name|Hardware name|Nom du matériel|Nome hardware|Nombre del hardware|Nome do hardware
Leer lassen, um den Originalnamen zu verwenden. Mit Übernehmen oder Enter speichern.|Leave blank to use the original name. Save with Apply or Enter.|Laissez vide pour utiliser le nom d’origine. Enregistrez avec Appliquer ou Entrée.|Lascia vuoto per usare il nome originale. Salva con Applica o Invio.|Déjalo vacío para usar el nombre original. Guarda con Aplicar o Intro.|Deixe vazio para usar o nome original. Guarde com Aplicar ou Enter.
Originalname wiederherstellen|Restore original name|Rétablir le nom d’origine|Ripristina nome originale|Restaurar nombre original|Restaurar nome original
Übernehmen|Apply|Appliquer|Applica|Aplicar|Aplicar
Grafikgröße (%)|Graphic size (%)|Taille du graphique (%)|Dimensione grafica (%)|Tamaño del gráfico (%)|Tamanho do gráfico (%)
Gesamte Instrumentgröße (%)|Whole instrument size (%)|Taille complète de l’instrument (%)|Dimensione intero strumento (%)|Tamaño completo del instrumento (%)|Tamanho completo do instrumento (%)
Skaliert die ganze Grafik mit Zahl und Einheit. Bei wenig Platz wird die Größe passend begrenzt.|Scales the whole graphic, including its number and unit. Size is limited to fit the available space.|Redimensionne le graphique entier, avec chiffre et unité. La taille est limitée à l’espace disponible.|Ridimensiona l’intera grafica, con numero e unità. La dimensione si adatta allo spazio disponibile.|Escala todo el gráfico, incluido el número y la unidad. El tamaño se limita al espacio disponible.|Redimensiona o gráfico inteiro, incluindo número e unidade. O tamanho é limitado ao espaço disponível.
Textgrößen|Text sizes|Tailles de texte|Dimensioni del testo|Tamaños del texto|Tamanhos do texto
Textgrößen zurücksetzen|Reset text sizes|Réinitialiser les tailles de texte|Reimposta dimensioni testo|Restablecer tamaños del texto|Repor tamanhos do texto
Textgrößen einzeln (%)|Individual text sizes (%)|Tailles de texte individuelles (%)|Dimensioni testo individuali (%)|Tamaños de texto individuales (%)|Tamanhos de texto individuais (%)
Widget-Titel|Widget titles|Titres des widgets|Titoli dei widget|Títulos de widgets|Títulos dos widgets
Bezeichnungen|Labels|Libellés|Etichette|Etiquetas|Etiquetas
Instrument-Zahlen|Instrument numbers|Chiffres des instruments|Numeri degli strumenti|Números del instrumento|Números dos instrumentos
Instrument-Einheiten|Instrument units|Unités des instruments|Unità degli strumenti|Unidades del instrumento|Unidades dos instrumentos
Die Einzelgrößen wirken zusätzlich zur allgemeinen Schriftgröße. 100 % entspricht dem gewählten Design.|Individual sizes multiply the general text size. 100% matches the selected design.|Les tailles individuelles multiplient la taille générale. 100 % correspond au style choisi.|Le dimensioni individuali moltiplicano quella generale. 100% corrisponde al design scelto.|Los tamaños individuales multiplican el tamaño general. 100 % corresponde al diseño seleccionado.|Os tamanhos individuais multiplicam o tamanho geral. 100% corresponde ao design selecionado.
Schriftgröße zurücksetzen (100 %)|Reset text size (100%)|Réinitialiser la taille du texte (100 %)|Reimposta dimensione testo (100%)|Restablecer tamaño del texto (100 %)|Repor tamanho do texto (100%)
Benötigt mindestens 2 Spalten.|Requires at least 2 columns.|Nécessite au moins 2 colonnes.|Richiede almeno 2 colonne.|Requiere al menos 2 columnas.|Requer pelo menos 2 colunas.
Text unter dem Graphen|Text below the graph|Texte sous le graphique|Testo sotto il grafico|Texto debajo del gráfico|Texto abaixo do gráfico
Ab 2 Spalten: Graph oben, Werte unten. Eine Spalte bleibt unverändert.|From 2 columns: graph above, readings below. One column keeps its existing arrangement.|À partir de 2 colonnes : graphique en haut et valeurs en dessous. Une colonne conserve sa disposition.|Da 2 colonne: grafico sopra e valori sotto. Una colonna mantiene la disposizione esistente.|A partir de 2 columnas: gráfico arriba y valores debajo. Una columna mantiene su disposición.|A partir de 2 colunas: gráfico acima e valores abaixo. Uma coluna mantém a disposição existente.
Einstellungen|Settings|Paramètres|Impostazioni|Configuración|Definições
Übersicht|Overview|Vue d’ensemble|Panoramica|Resumen|Visão geral
Position & Größe|Position & size|Position et taille|Posizione e dimensioni|Posición y tamaño|Posição e tamanho
Design & Schrift|Appearance & text|Apparence et texte|Aspetto e testo|Aspecto y texto|Aspeto e texto
Geräte & Werte|Devices & readings|Appareils et mesures|Dispositivi e valori|Dispositivos y valores|Dispositivos e valores
Karten & Reihenfolge|Cards & order|Cartes et ordre|Schede e ordine|Tarjetas y orden|Cartões e ordem
Warngrenzen|Warning thresholds|Seuils d’alerte|Soglie di avviso|Umbrales de aviso|Limites de aviso
Einrichtung|Setup|Installation|Configurazione|Instalación|Configuração
Systeminfo|System information|Informations système|Informazioni di sistema|Información del sistema|Informações do sistema
Sprache|Language|Langue|Lingua|Idioma|Idioma
DEIN SYSTEM. IM BLICK.|YOUR SYSTEM. AT A GLANCE.|VOTRE SYSTÈME EN BREF.|IL TUO SISTEMA IN BREVE.|TU SISTEMA DE UN VISTAZO.|O SEU SISTEMA NUM RELANCE.
Änderungen werden automatisch gespeichert.|Changes are saved automatically.|Les modifications sont enregistrées automatiquement.|Le modifiche vengono salvate automaticamente.|Los cambios se guardan automáticamente.|As alterações são guardadas automaticamente.
Passe die Sidebar an. Jede Änderung wird sofort übernommen und gespeichert.|Changes apply and save immediately.|Les modifications sont appliquées et enregistrées immédiatement.|Le modifiche sono applicate e salvate subito.|Los cambios se aplican y guardan al instante.|As alterações são aplicadas e guardadas de imediato.
Monitor, Breite, Autostart und Einpassen|Monitor, width, startup and fit|Écran, largeur, démarrage et ajustement|Monitor, larghezza, avvio e adattamento|Monitor, ancho, inicio y ajuste|Monitor, largura, arranque e ajuste
Layout, Farben und Lesbarkeit|Layout, colors and readability|Disposition, couleurs et lisibilité|Layout, colori e leggibilità|Diseño, colores y legibilidad|Disposição, cores e legibilidade
Geräte, sichtbare Werte und eigene Sensoren|Devices, readings and custom sensors|Appareils, mesures et capteurs personnels|Dispositivi, valori e sensori personali|Dispositivos, valores y sensores propios|Dispositivos, valores e sensores próprios
Sichtbare Karten und ihre Reihenfolge|Visible cards and their order|Cartes visibles et leur ordre|Schede visibili e ordine|Tarjetas visibles y su orden|Cartões visíveis e respetiva ordem
Treiber und Sensorzugriff prüfen|Check drivers and sensor access|Vérifier pilotes et accès aux capteurs|Controlla driver e accesso ai sensori|Comprobar controladores y sensores|Verificar controladores e sensores
Aktuelle Sidebar|Current sidebar|Barre actuelle|Barra attuale|Barra actual|Barra atual
Lege fest, wo die Sidebar sitzt und wie viel Platz sie benötigt.|Choose the position and space used by the sidebar.|Choisissez la position et l’espace occupé par la barre.|Scegli posizione e spazio occupato dalla barra.|Elige la posición y el espacio de la barra.|Escolha a posição e o espaço da barra.
Bildschirm|Screen|Écran|Schermo|Pantalla|Ecrã
Hauptmonitor|Primary monitor|Écran principal|Monitor principale|Monitor principal|Monitor principal
Monitore identifizieren|Identify monitors|Identifier les écrans|Identifica monitor|Identificar monitores|Identificar monitores
Bildschirmseite|Screen edge|Bord de l’écran|Lato dello schermo|Lado de la pantalla|Lado do ecrã
Rechts|Right|Droite|Destra|Derecha|Direita
Links|Left|Gauche|Sinistra|Izquierda|Esquerda
Größe & Anordnung|Size & arrangement|Taille et disposition|Dimensioni e disposizione|Tamaño y disposición|Tamanho e disposição
Breite pro Spalte (px)|Width per column (px)|Largeur par colonne (px)|Larghezza per colonna (px)|Ancho por columna (px)|Largura por coluna (px)
Karten nebeneinander|Cards side by side|Cartes côte à côte|Schede affiancate|Tarjetas en paralelo|Cartões lado a lado
Alle Karten auf eine Ansicht einpassen|Fit all cards in one view|Ajuster toutes les cartes à une vue|Adatta tutte le schede a una vista|Ajustar todas las tarjetas a una vista|Ajustar todos os cartões a uma vista
Einpassen aus: Karten behalten ihre Größe und lassen sich scrollen.|Fit off: cards keep their size and can be scrolled.|Sans ajustement : les cartes gardent leur taille et défilent.|Senza adattamento: le schede mantengono dimensioni e scorrono.|Sin ajuste: las tarjetas mantienen su tamaño y se desplazan.|Sem ajuste: os cartões mantêm o tamanho e permitem deslocamento.
Fenster & Autostart|Window & startup|Fenêtre et démarrage|Finestra e avvio|Ventana e inicio|Janela e arranque
Platz am Bildschirmrand reservieren|Reserve screen space|Réserver de l’espace à l’écran|Riserva spazio sullo schermo|Reservar espacio en pantalla|Reservar espaço no ecrã
Immer im Vordergrund|Always on top|Toujours au premier plan|Sempre in primo piano|Siempre visible|Sempre em primeiro plano
Mit Windows starten|Start with Windows|Démarrer avec Windows|Avvia con Windows|Iniciar con Windows|Iniciar com o Windows
Ein|On|Activé|Attivo|Activado|Ativado
Aus|Off|Désactivé|Disattivo|Desactivado|Desativado
Darstellung|Appearance|Apparence|Aspetto|Aspecto|Aspeto
Layout verändert den Aufbau. Das Farbschema bestimmt die Farben deiner Sidebar.|Layout changes the structure. The color scheme controls sidebar colors.|La disposition change la structure. Le thème définit les couleurs.|Il layout cambia la struttura. Il tema definisce i colori.|El diseño cambia la estructura. El tema define los colores.|A disposição muda a estrutura. O tema define as cores.
Kartenlayout|Card layout|Disposition des cartes|Layout delle schede|Diseño de tarjetas|Disposição dos cartões
Farbschema|Color scheme|Thème de couleurs|Tema colori|Tema de colores|Tema de cores
Schrift & Anzeige|Text & display|Texte et affichage|Testo e visualizzazione|Texto y pantalla|Texto e apresentação
Schriftgröße (%)|Text size (%)|Taille du texte (%)|Dimensione testo (%)|Tamaño del texto (%)|Tamanho do texto (%)
Deckkraft (%)|Opacity (%)|Opacité (%)|Opacità (%)|Opacidad (%)|Opacidade (%)
Interne IP|Internal IP|IP interne|IP interno|IP interna|IP interno
Externe IP|External IP|IP externe|IP esterno|IP externa|IP externo
IP-Adressen abrufen / aktualisieren|Fetch / refresh IP addresses|Obtenir / actualiser les adresses IP|Recupera / aggiorna indirizzi IP|Obtener / actualizar direcciones IP|Obter / atualizar endereços IP
IP-Adressen werden abgerufen …|Fetching IP addresses …|Récupération des adresses IP …|Recupero indirizzi IP …|Obteniendo direcciones IP …|A obter endereços IP …
Externe IP nicht erreichbar. Der bisherige Wert bleibt gespeichert.|External IP is unavailable. The previous value is kept.|IP externe inaccessible. La valeur précédente est conservée.|IP esterno non disponibile. Il valore precedente viene conservato.|IP externa no disponible. Se conserva el valor anterior.|IP externo indisponível. O valor anterior é mantido.
IP-Abfrage fehlgeschlagen. Gespeicherte Werte bleiben erhalten.|IP lookup failed. Saved values are kept.|Échec de la recherche IP. Les valeurs enregistrées sont conservées.|Ricerca IP non riuscita. I valori salvati vengono conservati.|La consulta IP ha fallado. Se conservan los valores guardados.|A consulta IP falhou. Os valores guardados são mantidos.
IP-Adressen werden nur per Button abgerufen und gespeichert. Keine automatische Aktualisierung.|IP addresses are fetched and saved only with this button. No automatic refresh.|Les adresses IP sont récupérées et enregistrées uniquement avec ce bouton. Aucune actualisation automatique.|Gli indirizzi IP vengono recuperati e salvati solo con questo pulsante. Nessun aggiornamento automatico.|Las direcciones IP se obtienen y guardan solo con este botón. Sin actualización automática.|Os endereços IP são obtidos e guardados apenas com este botão. Sem atualização automática.
Externe IP: api64.ipify.org über die Standardroute, nicht zwingend über den ausgewählten Adapter.|External IP: api64.ipify.org via the default route, not necessarily the selected adapter.|IP externe : api64.ipify.org via la route par défaut, pas nécessairement l’adaptateur sélectionné.|IP esterno: api64.ipify.org tramite il percorso predefinito, non necessariamente l’adattatore selezionato.|IP externa: api64.ipify.org mediante la ruta predeterminada, no necesariamente el adaptador seleccionado.|IP externo: api64.ipify.org pela rota predefinida, não necessariamente pelo adaptador selecionado.
Text & Diagramme|Text & charts|Texte et graphiques|Testo e grafici|Texto y gráficos|Texto e gráficos
Textfarben sowie Farben und Stärke der Balken und Instrumente anpassen.|Customize text colors and the colors and thickness of bars and gauges.|Personnaliser les couleurs du texte et la couleur et l’épaisseur des barres et cadrans.|Personalizza i colori del testo e il colore e lo spessore di barre e strumenti.|Personaliza los colores del texto y el color y grosor de barras e instrumentos.|Personalize as cores do texto e a cor e espessura das barras e instrumentos.
Textfarben|Text colors|Couleurs du texte|Colori del testo|Colores del texto|Cores do texto
Überschriften|Headings|Titres|Titoli|Títulos|Títulos
Sensorbezeichnungen|Sensor labels|Libellés des capteurs|Etichette sensori|Etiquetas de sensores|Rótulos de sensores
Hardware-Namen|Hardware names|Noms du matériel|Nomi hardware|Nombres del hardware|Nomes do hardware
Balken & Instrumente|Bars & gauges|Barres et cadrans|Barre e strumenti|Barras e instrumentos|Barras e instrumentos
Akzentfarbe|Accent color|Couleur d’accentuation|Colore accento|Color de acento|Cor de destaque
Hintergrundspur|Track color|Couleur de la piste|Colore traccia|Color de pista|Cor da faixa
Zweiter Ring|Second ring|Deuxième anneau|Secondo anello|Segundo anillo|Segundo anel
Linien- / Balkenstärke (%)|Line / bar thickness (%)|Épaisseur lignes / barres (%)|Spessore linee / barre (%)|Grosor de líneas / barras (%)|Espessura de linhas / barras (%)
Leer = Farbe des Farbschemas. Eigene Farben bleiben beim Designwechsel erhalten.|Empty = theme color. Custom colors are kept when switching designs.|Vide = couleur du thème. Les couleurs personnalisées sont conservées lors du changement de thème.|Vuoto = colore del tema. I colori personalizzati restano cambiando design.|Vacío = color del tema. Los colores personalizados se conservan al cambiar el diseño.|Vazio = cor do tema. As cores personalizadas mantêm-se ao mudar o design.
Farbanpassungen zurücksetzen|Reset color customizations|Réinitialiser les couleurs personnalisées|Ripristina colori personalizzati|Restablecer colores personalizados|Repor cores personalizadas
Auswählen|Choose|Choisir|Scegli|Elegir|Escolher
Standardfarbe|Default color|Couleur par défaut|Colore predefinito|Color predeterminado|Cor predefinida
Widget-Hintergrund (%)|Widget background (%)|Fond des widgets (%)|Sfondo widget (%)|Fondo de widgets (%)|Fundo dos widgets (%)
Hufeisen|Horseshoe|Fer à cheval|Ferro di cavallo|Herradura|Ferradura
Punktmatrix|Dot matrix|Matrice de points|Matrice di punti|Matriz de puntos|Matriz de pontos
Panorama|Panorama|Panorama|Panorama|Panorama|Panorama
Messstreifen|Sensor tape|Bande de mesure|Striscia sensore|Cinta de sensor|Faixa do sensor
Messpult|Console|Console|Console|Consola|Consola
Digital|Digital|Numérique|Digitale|Digital|Digital
Seitenbalken|Side rail|Barre latérale|Barra laterale|Barra lateral|Barra lateral
Segmentring|Segment ring|Anneau segmenté|Anello segmentato|Anillo segmentado|Anel segmentado
Tachometer|Speedometer|Compteur|Tachimetro|Velocímetro|Velocímetro
Sensor-Säule|Sensor column|Colonne de capteur|Colonna sensore|Columna de sensor|Coluna de sensor
Hintergrund-Deckkraft (%)|Background opacity (%)|Opacité du fond (%)|Opacità dello sfondo (%)|Opacidad del fondo (%)|Opacidade do fundo (%)
Doppelring|Twin rings|Double anneau|Doppio anello|Doble anillo|Anel duplo
Halbkreis|Semicircle|Demi-cercle|Semicerchio|Semicírculo|Semicírculo
große Werte|large readings|grandes valeurs|valori grandi|valores grandes|valores grandes
Temperatureinheit|Temperature unit|Unité de température|Unità di temperatura|Unidad de temperatura|Unidade de temperatura
Karten|Cards|Cartes|Schede|Tarjetas|Cartões
Kompakt|Compact|Compact|Compatto|Compacto|Compacto
Instrumente|Gauges|Jauges|Indicatori|Indicadores|Indicadores
Textansicht|Text view|Vue texte|Vista testo|Vista de texto|Vista de texto
Wertekacheln|Value tiles|Tuiles de valeurs|Riquadri valori|Mosaicos de valores|Mosaicos de valores
Balken|Bars|Barres|Barre|Barras|Barras
Links ein Gerät wählen. Rechts Anzeige, Werte und eigene Sensoren bearbeiten.|Select a device on the left. Edit its readings and sensors on the right.|Choisissez un appareil à gauche, ses mesures et capteurs à droite.|Scegli un dispositivo a sinistra, valori e sensori a destra.|Elige un dispositivo a la izquierda y sus valores y sensores a la derecha.|Escolha um dispositivo à esquerda e os valores e sensores à direita.
Gerät anzeigen|Show device|Afficher l’appareil|Mostra dispositivo|Mostrar dispositivo|Mostrar dispositivo
Sichtbare Werte|Visible readings|Mesures visibles|Valori visibili|Valores visibles|Valores visíveis
Eigene Sensoren|Custom sensors|Capteurs personnels|Sensori personali|Sensores propios|Sensores próprios
Keine eigenen Sensoren hinzugefügt.|No custom sensors added.|Aucun capteur personnel ajouté.|Nessun sensore personale aggiunto.|No hay sensores propios añadidos.|Sem sensores próprios adicionados.
Sensor hinzufügen|Add sensor|Ajouter un capteur|Aggiungi sensore|Añadir sensor|Adicionar sensor
Entfernen|Remove|Supprimer|Rimuovi|Eliminar|Remover
CPU-Temperaturquelle|CPU temperature source|Source de température CPU|Fonte temperatura CPU|Origen de temperatura CPU|Origem da temperatura da CPU
Temperatursensor|Temperature sensor|Capteur de température|Sensore temperatura|Sensor de temperatura|Sensor de temperatura
Automatisch: CPU / Socket bevorzugen|Automatic: prefer CPU / socket|Automatique : préférer CPU / socket|Automatico: preferisci CPU / socket|Automático: preferir CPU / socket|Automático: preferir CPU / socket
Häkchen schalten Karten ein oder aus. Mit den Pfeilen änderst du ihre Reihenfolge.|Check cards to show them. Use arrows to change their order.|Cochez les cartes à afficher. Les flèches changent leur ordre.|Seleziona le schede da mostrare. Le frecce cambiano l’ordine.|Marca las tarjetas a mostrar. Las flechas cambian su orden.|Selecione os cartões. As setas alteram a ordem.
Sensoren hinzufügen|Add sensors|Ajouter des capteurs|Aggiungi sensori|Añadir sensores|Adicionar sensores
Sensor auswählen|Select sensor|Choisir un capteur|Seleziona sensore|Seleccionar sensor|Selecionar sensor
Suchen nach Gerät, Sensor oder Typ|Search device, sensor or type|Rechercher appareil, capteur ou type|Cerca dispositivo, sensore o tipo|Buscar dispositivo, sensor o tipo|Pesquisar dispositivo, sensor ou tipo
Ziel-Widget für den ausgewählten Sensor|Target widget for selected sensor|Widget cible du capteur|Widget di destinazione del sensore|Widget de destino del sensor|Widget de destino do sensor
Zurück zu Geräte & Werte|Back to devices & readings|Retour aux appareils et mesures|Torna a dispositivi e valori|Volver a dispositivos y valores|Voltar a dispositivos e valores
Zu eigenen Werten hinzufügen|Add custom reading|Ajouter une mesure personnelle|Aggiungi valore personale|Añadir valor propio|Adicionar valor próprio
Aus eigenen Werten entfernen|Remove custom reading|Supprimer la mesure personnelle|Rimuovi valore personale|Eliminar valor propio|Remover valor próprio
Hinzugefügt|Added|Ajouté|Aggiunto|Añadido|Adicionado
Gerät|Device|Appareil|Dispositivo|Dispositivo|Dispositivo
Wert|Reading|Valeur|Valore|Valor|Valor
Eigene Werte|Custom readings|Mesures personnelles|Valori personali|Valores propios|Valores próprios
Automatisch · zum zugehörigen Widget|Automatic · matching widget|Automatique · widget correspondant|Automatico · widget corrispondente|Automático · widget correspondiente|Automático · widget correspondente
Separate Karte · Eigene Werte|Separate card · custom readings|Carte séparée · mesures personnelles|Scheda separata · valori personali|Tarjeta separada · valores propios|Cartão separado · valores próprios
Separate Karte|Separate card|Carte séparée|Scheda separata|Tarjeta separada|Cartão separado
Angezeigt in|Shown in|Affiché dans|Mostrato in|Se muestra en|Mostrado em
hinzugefügte Werte|added readings|mesures ajoutées|valori aggiunti|valores añadidos|valores adicionados
Sensor auswählen, um das Ziel-Widget festzulegen.|Select a sensor to choose its widget.|Choisissez un capteur pour définir son widget.|Seleziona un sensore per scegliere il widget.|Selecciona un sensor para elegir su widget.|Selecione um sensor para escolher o widget.
Widget derzeit nicht verfügbar|Widget currently unavailable|Widget indisponible|Widget non disponibile|Widget no disponible|Widget indisponível
Sensoren werden erkannt …|Detecting sensors …|Détection des capteurs…|Rilevamento sensori…|Detectando sensores…|A detetar sensores…
Temperatur|Temperature|Température|Temperatura|Temperatura|Temperatura
Auslastung|Load|Charge|Carico|Uso|Utilização
Leistung|Power|Puissance|Potenza|Potencia|Potência
Spannung|Voltage|Tension|Tensione|Voltaje|Tensão
Lüfter|Fan|Ventilateur|Ventola|Ventilador|Ventoinha
Takt Ø|Average clock|Fréquence moyenne|Frequenza media|Frecuencia media|Frequência média
Takt|Clock|Fréquence|Frequenza|Frecuencia|Frequência
Belegt|Used|Utilisé|Utilizzato|Usado|Usado
Verfügbar|Available|Disponible|Disponibile|Disponible|Disponível
Gesamt|Total|Total|Totale|Total|Total
Kapazität|Capacity|Capacité|Capacità|Capacidad|Capacidade
Lesen|Read|Lecture|Lettura|Lectura|Leitura
Schreiben|Write|Écriture|Scrittura|Escritura|Escrita
VRAM belegt|VRAM used|VRAM utilisé|VRAM usata|VRAM usada|VRAM usada
VRAM Last|VRAM load|Charge VRAM|Carico VRAM|Uso de VRAM|Utilização da VRAM
Temperaturgrenze (°C)|Temperature threshold (°C)|Seuil de température (°C)|Soglia temperatura (°C)|Umbral de temperatura (°C)|Limite de temperatura (°C)
Auslastungsgrenze (%)|Load threshold (%)|Seuil de charge (%)|Soglia di carico (%)|Umbral de uso (%)|Limite de utilização (%)
Karten werden bei Überschreitung rot umrandet.|Cards have a red border above the threshold.|Les cartes ont un bord rouge au-dessus du seuil.|Le schede hanno un bordo rosso oltre la soglia.|Las tarjetas tienen borde rojo al superar el umbral.|Os cartões têm contorno vermelho acima do limite.
Die Warnung ist nur optisch.|The warning is visual only.|L’alerte est uniquement visuelle.|L’avviso è solo visivo.|El aviso es solo visual.|O aviso é apenas visual.
Sensoren einrichten|Sensor setup|Configuration des capteurs|Configurazione sensori|Configuración de sensores|Configuração de sensores
Sensorzugriff einmalig einrichten|Configure sensor access once|Configurer l’accès aux capteurs|Configura accesso ai sensori|Configurar acceso a sensores|Configurar acesso aos sensores
PawnIO installieren / aktualisieren|Install / update PawnIO|Installer / mettre à jour PawnIO|Installa / aggiorna PawnIO|Instalar / actualizar PawnIO|Instalar / atualizar PawnIO
1 · Laufzeit|1 · Runtime|1 · Environnement|1 · Ambiente di esecuzione|1 · Entorno de ejecución|1 · Ambiente de execução
2 · Sensortreiber für CPU und Mainboard|2 · CPU and motherboard sensor driver|2 · Pilote capteurs CPU et carte mère|2 · Driver sensori CPU e scheda madre|2 · Controlador de sensores CPU y placa base|2 · Controlador de sensores CPU e placa-mãe
3 · Automatischer Sensorzugriff|3 · Automatic sensor access|3 · Accès automatique aux capteurs|3 · Accesso automatico ai sensori|3 · Acceso automático a sensores|3 · Acesso automático aos sensores
4 · Grafiktreiber (falls GPU-Werte fehlen)|4 · Graphics drivers (if readings are missing)|4 · Pilotes graphiques (si mesures manquantes)|4 · Driver grafici (se mancano valori)|4 · Controladores gráficos (si faltan valores)|4 · Controladores gráficos (se faltarem valores)
.NET-Laufzeit ist enthalten – keine zusätzliche Installation nötig.|.NET runtime included – no extra installation needed.|Environnement .NET inclus : aucune installation supplémentaire.|Ambiente .NET incluso: nessuna installazione aggiuntiva.|Entorno .NET incluido: no requiere otra instalación.|Ambiente .NET incluído: sem instalação adicional.
Die Einrichtung benötigt einmalig Administratorrechte.|Setup requires administrator approval once.|L’installation nécessite une autorisation administrateur unique.|La configurazione richiede una sola autorizzazione amministratore.|La configuración requiere permiso de administrador una vez.|A configuração requer autorização de administrador uma vez.
Grafiktreiber werden beim Hersteller heruntergeladen.|Download graphics drivers from the manufacturer.|Téléchargez les pilotes chez le fabricant.|Scarica i driver grafici dal produttore.|Descarga los controladores del fabricante.|Obtenha os controladores gráficos no fabricante.
Für CPU- und Mainboard-Sensoren wird PawnIO benötigt.|CPU and motherboard sensors require PawnIO.|Les capteurs CPU et carte mère nécessitent PawnIO.|I sensori CPU e scheda madre richiedono PawnIO.|Los sensores CPU y placa base requieren PawnIO.|Os sensores CPU e placa-mãe requerem PawnIO.
Treiber|Drivers|Pilotes|Driver|Controladores|Controladores
Aktualisierung (Millisekunden)|Refresh interval (milliseconds)|Actualisation (millisecondes)|Aggiornamento (millisecondi)|Actualización (milisegundos)|Atualização (milissegundos)
Konfiguration öffnen|Open configuration|Ouvrir la configuration|Apri configurazione|Abrir configuración|Abrir configuração
Unterstützung hängt von Hardware, Firmware und Treibern ab.|Support depends on hardware, firmware and drivers.|La compatibilité dépend du matériel, du firmware et des pilotes.|Il supporto dipende da hardware, firmware e driver.|La compatibilidad depende del hardware, firmware y controladores.|O suporte depende do hardware, firmware e controladores.
Autostart wird über die Windows-Aufgabenplanung ausgeführt.|Startup uses Windows Task Scheduler.|Le démarrage utilise le Planificateur de tâches Windows.|L’avvio usa l’Utilità di pianificazione di Windows.|El inicio usa el Programador de tareas de Windows.|O arranque utiliza o Agendador de Tarefas do Windows.
Einstellungen werden neben der EXE gespeichert.|Settings are saved next to the EXE.|Les paramètres sont enregistrés à côté du fichier EXE.|Le impostazioni sono salvate accanto al file EXE.|La configuración se guarda junto al EXE.|As definições são guardadas junto ao EXE.
Administrator|Administrator|Administrateur|Amministratore|Administrador|Administrador
Standardbenutzer|Standard user|Utilisateur standard|Utente standard|Usuario estándar|Utilizador padrão
Zugriff|Access|Accès|Accesso|Acceso|Acesso
Geräte|Devices|Appareils|Dispositivi|Dispositivos|Dispositivos
Messwerte|Readings|Mesures|Valori|Valores|Valores
Spalte(n)|column(s)|colonne(s)|colonna/e|columna(s)|coluna(s)
Nicht verfügbar|Unavailable|Indisponible|Non disponibile|No disponible|Indisponível
Autostart|Startup|Démarrage|Avvio|Inicio|Arranque
ist verfügbar|is available|est disponible|è disponibile|está disponible|está disponível
Download|Download|Téléchargement|Download|Descarga|Receção
Physischer Arbeitsspeicher|Physical memory|Mémoire physique|Memoria fisica|Memoria física|Memória física
Upload|Upload|Envoi|Upload|Subida|Envio
Adapter-Link|Adapter link|Liaison de l’adaptateur|Collegamento adattatore|Enlace del adaptador|Ligação do adaptador
Sensor suchen und Häkchen setzen. Eigene Werte erscheinen automatisch im zugehörigen Hardware-Widget.|Find a sensor and check it to add it to its widget.|Cherchez un capteur et cochez-le pour l’ajouter au widget.|Cerca un sensore e selezionalo per aggiungerlo al widget.|Busca un sensor y márcalo para añadirlo al widget.|Procure um sensor e selecione-o para o adicionar ao widget.
Ziel-Widget auswählen. Häkchen hinzufügen oder entfernen.|Choose a target widget. Check to add or uncheck to remove.|Choisissez un widget. Cochez pour ajouter, décochez pour supprimer.|Scegli un widget. Seleziona per aggiungere, deseleziona per rimuovere.|Elige un widget. Marca para añadir o desmarca para eliminar.|Escolha um widget. Selecione para adicionar ou desmarque para remover.
Status prüfen und fehlende Komponenten gezielt einrichten. Es wird nichts automatisch installiert.|Check status and configure missing components. Nothing installs automatically.|Vérifiez l’état et configurez les composants manquants. Aucune installation automatique.|Controlla lo stato e configura i componenti mancanti. Nessuna installazione automatica.|Comprueba el estado y configura componentes faltantes. Sin instalación automática.|Verifique o estado e configure os componentes em falta. Sem instalação automática.
Häkchen = in Eigene Werte hinzugefügt. Zum Entfernen Häkchen ausschalten.|Checked = custom reading added. Uncheck to remove.|Coché = mesure ajoutée. Décochez pour supprimer.|Selezionato = valore aggiunto. Deseleziona per rimuovere.|Marcado = valor añadido. Desmarca para eliminar.|Selecionado = valor adicionado. Desmarque para remover.
Sensoren werden initialisiert …|Initializing sensors …|Initialisation des capteurs…|Inizializzazione sensori…|Inicializando sensores…|A iniciar sensores…
Gestartete EXE|Running EXE|EXE en cours|EXE in esecuzione|EXE en ejecución|EXE em execução
Autostart-Datei|Startup EXE|EXE au démarrage|EXE di avvio|EXE de inicio|EXE de arranque
Autostart verweist auf eine andere EXE.|Startup points to another EXE.|Le démarrage pointe vers un autre EXE.|L’avvio punta a un altro EXE.|El inicio apunta a otro EXE.|O arranque aponta para outro EXE.
Datei|File|Fichier|File|Archivo|Ficheiro
aktiv|enabled|activé|attivo|activado|ativado
deaktiviert|disabled|désactivé|disattivo|desactivado|desativado
Aufgabenplanung|Task Scheduler|Planificateur de tâches|Utilità di pianificazione|Programador de tareas|Agendador de Tarefas
erhöhte Sensorrechte|elevated sensor access|accès élevé aux capteurs|accesso elevato ai sensori|acceso elevado a sensores|acesso elevado aos sensores
Autostart mit Sensorrechten ist noch nicht eingerichtet.|Startup with sensor access is not configured yet.|Le démarrage avec accès aux capteurs n’est pas encore configuré.|L’avvio con accesso ai sensori non è ancora configurato.|El inicio con acceso a sensores aún no está configurado.|O arranque com acesso aos sensores ainda não está configurado.
Treiberzugriff verfügbar.|Driver access available.|Accès au pilote disponible.|Accesso al driver disponibile.|Acceso al controlador disponible.|Acesso ao controlador disponível.
Administratorrechte aktiv.|Administrator access active.|Droits administrateur actifs.|Diritti amministratore attivi.|Permisos de administrador activos.|Permissões de administrador ativas.
Zugriff fehlgeschlagen|Access failed|Échec d’accès|Accesso non riuscito|Error de acceso|Falha de acesso
Sidebar mit Administratorrechten neu starten.|Restart the sidebar with administrator access.|Redémarrez la barre avec les droits administrateur.|Riavvia la barra con diritti amministratore.|Reinicia la barra con permisos de administrador.|Reinicie a barra com permissões de administrador.
Treiber-/Modulkompatibilität prüfen.|Check driver/module compatibility.|Vérifiez la compatibilité pilote/module.|Verifica compatibilità driver/modulo.|Comprueba compatibilidad del controlador/módulo.|Verifique a compatibilidade do controlador/módulo.
PawnIO ist nicht installiert. CPU-Temperaturen und weitere Hardware-Sensoren benötigen diesen Sensortreiber.|PawnIO is not installed. CPU temperatures and other hardware sensors require it.|PawnIO n’est pas installé. Les températures CPU et d’autres capteurs en ont besoin.|PawnIO non è installato. Temperature CPU e altri sensori lo richiedono.|PawnIO no está instalado. La temperatura CPU y otros sensores lo requieren.|O PawnIO não está instalado. A temperatura CPU e outros sensores precisam dele.
Klare Fluent-Karten mit großer Hauptzahl|Clean Fluent cards with a large primary reading|Cartes Fluent avec une grande valeur principale|Schede Fluent con valore principale grande|Tarjetas Fluent con valor principal grande|Cartões Fluent com valor principal grande
Kompakte Fluent-Karten|Compact Fluent cards|Cartes Fluent compactes|Schede Fluent compatte|Tarjetas Fluent compactas|Cartões Fluent compactos
Große Hauptzahl und ruhige Messwertliste|Large primary reading with a clear reading list|Grande valeur principale et liste claire|Valore principale grande ed elenco chiaro|Valor principal grande y lista clara|Valor principal grande e lista clara
Helle Widget-Flächen mit kompakten Zusatzwerten|Light widget surfaces with compact details|Widgets clairs avec détails compacts|Widget chiari con dettagli compatti|Widgets claros con detalles compactos|Widgets claros com detalhes compactos
Dashboard mit Glasflächen und Datenkacheln|Dashboard with glass surfaces and data tiles|Tableau de bord avec surfaces en verre et tuiles de données|Dashboard con superfici in vetro e riquadri dati|Panel con superficies de cristal y mosaicos de datos|Painel com superfícies de vidro e blocos de dados
Schwarze Referenz-Karten mit echtem 60-Sekunden-Verlauf|Black reference cards with real 60-second history|Cartes noires avec historique réel de 60 secondes|Schede nere con cronologia reale di 60 secondi|Tarjetas negras con historial real de 60 segundos|Cartões pretos com histórico real de 60 segundos
Breiter Verlauf zwischen Hauptwert und Messwerten|Wide history between primary value and readings|Historique large entre valeur principale et mesures|Cronologia ampia tra valore principale e misure|Historial amplio entre valor principal y mediciones|Histórico amplo entre valor principal e leituras
Messwerte links und große Verlaufskurve rechts|Readings on the left and a large history chart on the right|Mesures à gauche et grand historique à droite|Misure a sinistra e grande grafico a destra|Mediciones a la izquierda y gráfico grande a la derecha|Leituras à esquerda e gráfico amplo à direita
Kompakte Messkarte mit Verlauf am unteren Rand|Compact reading card with a bottom history strip|Carte compacte avec historique en bas|Scheda compatta con cronologia in basso|Tarjeta compacta con historial inferior|Cartão compacto com histórico na base
Zwei getrennte Verläufe mit eigenen Einheiten|Two separate histories with independent units|Deux historiques séparés avec leurs unités|Due cronologie separate con unità indipendenti|Dos historiales separados con unidades independientes|Dois históricos separados com unidades independentes
Dezenter Verlauf hinter den Messwerten|Subtle history behind the readings|Historique discret derrière les mesures|Cronologia discreta dietro le misure|Historial sutil detrás de las mediciones|Histórico discreto atrás das leituras
Verlauf und klar geordnete Messwertfelder|History chart with organized reading tiles|Historique avec tuiles de mesures organisées|Cronologia con riquadri di misure ordinati|Historial con mosaicos de mediciones ordenados|Histórico com blocos de leituras organizados
""";
}


