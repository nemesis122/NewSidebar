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
""";
}
