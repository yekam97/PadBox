// PadBox: convierte cualquier controlador MIDI de pads en un panel de música y de atajos (perfiles para M-VAVE, Akai y Novation).
// Pad 1 alterna entre modo MÚSICA y modo OFICINA; pad 2 pasa al siguiente preset del modo.
// Compilar con build.bat (usa el compilador de C# que trae Windows).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace PadBox
{
    // ---------- idioma ----------
    // La app está escrita en español; en inglés se traduce cada texto con esta tabla.
    // El idioma sale de «Idioma» en la configuración, o del idioma de Windows (español si es español, si no inglés).
    static class L
    {
        public static bool En;

        public static void Init(string[] args, string cfgPath)
        {
            string pick = null;
            int i = Array.IndexOf(args, "--lang");   // pruebas: --lang en | es
            if (i >= 0 && i + 1 < args.Length) pick = args[i + 1];
            if (pick == null)
                try
                {
                    if (File.Exists(cfgPath))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(File.ReadAllText(cfgPath), "\"Lang\":\"(es|en)\"");
                        if (m.Success) pick = m.Groups[1].Value;
                    }
                }
                catch (Exception) { }
            if (pick == "es" || pick == "en") En = pick == "en";
            else
                try { En = !string.Equals(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "es", StringComparison.OrdinalIgnoreCase); }
                catch (Exception) { En = false; }
        }

        public static string T(string es)
        {
            string en;
            return En && es != null && Map.TryGetValue(es, out en) ? en : es;
        }

        static readonly Dictionary<string, string> Map = new Dictionary<string, string> {
            {"Otro controlador (16 pads)", "Other controller (16 pads)"},
            {"Otro controlador (8 pads)", "Other controller (8 pads)"},
            {"Batería", "Drums"},
            {"Canción", "Song"},
            {"Teclado", "Keyboard"},
            {"Nota", "Note"},
            {"Notas", "Notes"},
            {"Controles", "Controls"},
            {"Escritorio", "Desktop"},
            {"Navegador", "Browser"},
            {"Grabar / OBS", "Record / OBS"},
            {"Transmitir", "Stream"},
            {"Pluma", "Pen"},
            {"Pincel", "Brush"},
            {"Documento", "Document"},
            {"Paleta", "Palette"},
            {"Foto", "Photo"},
            {"Cámara", "Camera"},
            {"Micrófono", "Microphone"},
            {"Audífonos", "Headphones"},
            {"Reproducir", "Play"},
            {"Juegos", "Games"},
            {"Rayo", "Lightning"},
            {"Código", "Code"},
            {"Correo", "Mail"},
            {"Carpeta", "Folder"},
            {"Calculadora", "Calculator"},
            {"Ajustes", "Settings"},
            {"Estrella", "Star"},
            {"Batería rock", "Rock drums"},
            {"Cumbia (percusión)", "Cumbia (percussion)"},
            {"Guitarra blues", "Blues guitar"},
            {"Piano de acordes", "Chord piano"},
            {"Mis sonidos", "My sounds"},
            {"Batería GM", "GM drums"},
            {"Piano cumbia villera", "Cumbia villera piano"},
            {"Acordeón", "Accordion"},
            {"Teclado cumbia rebajada", "Slowed cumbia keys"},
            {"Saxofón", "Saxophone"},
            {"Rack de batería", "Drum rack"},
            {"Cromática C3", "Chromatic C3"},
            {"Escala Do mayor", "C major scale"},
            {"Pentatónica La menor", "A minor pentatonic"},
            {"Acordes en Do", "Chords in C"},
            {"Botones CC (on/off)", "CC buttons (on/off)"},
            {"Mayor", "Major"},
            {"Menor", "Minor"},
            {"Disminuido", "Diminished"},
            {"Ritmo rock", "Rock beat"},
            {"Bombo", "Kick"},
            {"Redoblante", "Snare"},
            {"Aro", "Rimshot"},
            {"Hat cerrado", "Closed hat"},
            {"Hat pedal", "Pedal hat"},
            {"Hat abierto", "Open hat"},
            {"Tom piso", "Floor tom"},
            {"Tom bajo", "Low tom"},
            {"Tom medio", "Mid tom"},
            {"Tom alto", "High tom"},
            {"Ritmo house", "House beat"},
            {"Ritmo techno", "Techno beat"},
            {"Botón ", "Button "},
            {"Ritmo reggaetón", "Reggaeton beat"},
            {"Percusión", "Percussion"},
            {"Bajo", "Bass"},
            {"Bajo wub", "Wub bass"},
            {"Acordes", "Chords"},
            {"Melodía", "Melody"},
            {"Arpegio", "Arpeggio"},
            {"Voz", "Vocal"},
            {"Subida", "Riser"},
            {"Impacto", "Impact"},
            {"Redoble", "Snare roll"},
            {"Ritmo blues", "Blues beat"},
            {"Ritmo balada", "Ballad beat"},
            {"Ritmo dubstep", "Dubstep beat"},
            {"Golpe", "Hit"},
            {"Bajo ", "Bass "},
            {"Ritmo cumbia", "Cumbia beat"},
            {"Güiro largo", "Long güiro"},
            {"Güiro corto", "Short güiro"},
            {"Conga abierta", "Open conga"},
            {"Conga muda", "Muted conga"},
            {"Conga baja", "Low conga"},
            {"Timbal alto", "High timbale"},
            {"Timbal bajo", "Low timbale"},
            {"Cencerro", "Cowbell"},
            {"Bongó alto", "High bongo"},
            {"Bongó bajo", "Low bongo"},
            {"Ritmo cuarteto", "Cuarteto beat"},
            {"Ritmo vallenato", "Vallenato beat"},
            {"Ritmo rebajado", "Slowed beat"},
            {"Ritmo bossa nova", "Bossa nova beat"},
            {"Copiar", "Copy"},
            {"Pegar", "Paste"},
            {"Deshacer", "Undo"},
            {"Rehacer", "Redo"},
            {"Cambiar ventana", "Switch window"},
            {"Ver escritorio", "Show desktop"},
            {"Captura de pantalla", "Screenshot"},
            {"Pista anterior", "Previous track"},
            {"Play / Pausa", "Play / Pause"},
            {"Pista siguiente", "Next track"},
            {"Silenciar", "Mute"},
            {"Bajar volumen", "Volume down"},
            {"Subir volumen", "Volume up"},
            {"Grabar on/off", "Record on/off"},
            {"Transmitir on/off", "Stream on/off"},
            {"Pausar grabación", "Pause recording"},
            {"Guardar repetición", "Save replay"},
            {"Escena 1", "Scene 1"},
            {"Escena 2", "Scene 2"},
            {"Escena 3", "Scene 3"},
            {"Escena 4", "Scene 4"},
            {"Silenciar micrófono", "Mute mic"},
            {"Silenciar escritorio", "Mute desktop audio"},
            {"Captura de OBS", "OBS screenshot"},
            {"Cámara virtual", "Virtual camera"},
            {"Mostrar/ocultar cámara", "Show/hide camera"},
            {"Nueva pestaña", "New tab"},
            {"Cerrar pestaña", "Close tab"},
            {"Reabrir pestaña", "Reopen tab"},
            {"Pestaña anterior", "Previous tab"},
            {"Pestaña siguiente", "Next tab"},
            {"Atrás", "Back"},
            {"Adelante", "Forward"},
            {"Recargar", "Reload"},
            {"Barra de direcciones", "Address bar"},
            {"Buscar en la página", "Find in page"},
            {"Selección", "Selection"},
            {"Selección directa", "Direct selection"},
            {"Texto", "Type"},
            {"Rectángulo", "Rectangle"},
            {"Elipse", "Ellipse"},
            {"Mano (mantener)", "Hand (hold)"},
            {"Agrupar", "Group"},
            {"Desagrupar", "Ungroup"},
            {"Ajustar a ventana", "Fit to window"},
            {"Mover", "Move"},
            {"Marco", "Marquee"},
            {"Lazo", "Lasso"},
            {"Borrador", "Eraser"},
            {"Tampón", "Clone stamp"},
            {"Duplicar capa", "Duplicate layer"},
            {"Transformar", "Transform"},
            {"Intercambiar colores", "Swap colors"},
            {"Atajo no válido", "Invalid shortcut"},
            {"Espacio", "Space"},
            {"Silencio", "Mute"},
            {"Play/Pausa", "Play/Pause"},
            {"Supr", "Del"},
            {"Retroceso", "Backspace"},
            {"Sonidos|*.mp3;*.wav;*.wma;*.m4a;*.aac;*.aif;*.aiff;*.mp2;*.snd;*.au|Todos los archivos|*.*", "Sounds|*.mp3;*.wav;*.wma;*.m4a;*.aac;*.aif;*.aiff;*.mp2;*.snd;*.au|All files|*.*"},
            {"Cumbia villera", "Cumbia villera"},
            {"Cuarteto", "Cuarteto"},
            {"Cumbia rebajada", "Slowed cumbia"},
            {"Balada", "Ballad"},
            {"Reggaetón (dembow)", "Reggaeton (dembow)"},
            {"Luces: ", "Lights: "},
            {"Luces: el SMC-PAD no respondió.", "Lights: the SMC-PAD did not respond."},
            {"Luces: el SMC-PAD no confirmó el cambio de color.", "Lights: the SMC-PAD did not confirm the color change."},
            {"(vacío)", "(empty)"},
            {"Atajo de teclado", "Keyboard shortcut"},
            {"Escribir texto", "Type text"},
            {"Abrir programa o web", "Open program or website"},
            {"Sonido (mp3, wav…)", "Sound (mp3, wav…)"},
            {"Nota MIDI", "MIDI note"},
            {"Acorde MIDI", "MIDI chord"},
            {"Melodía paso a paso", "Step-by-step melody"},
            {"Ritmo en bucle (on/off)", "Looped beat (on/off)"},
            {"CC momentáneo", "Momentary CC"},
            {"Nada", "Nothing"},
            {"Una vez (suena completo)", "Once (plays to the end)"},
            {"Mientras mantienes el pad", "While you hold the pad"},
            {"Bucle on/off", "Loop on/off"},
            {"MÚSICA", "MUSIC"},
            {"OFICINA", "OFFICE"},
            {"Cambiar el ícono del preset", "Change the preset icon"},
            {"Nuevo", "New"},
            {"Duplicar", "Duplicate"},
            {"Renombrar", "Rename"},
            {"Borrar", "Delete"},
            {"Nombre", "Name"},
            {"Acción", "Action"},
            {"Atajo", "Shortcut"},
            {"Grabar", "Record"},
            {"Pulsa «Grabar» y luego la combinación en tu teclado, o escríbela.\n\nEjemplos: Ctrl+Shift+N · Win+D · Alt+Tab · F5\nTeclas especiales: Space, Enter, Esc, Delete, Left, Right, Add (Num +), VolumeUp, VolumeMute, MediaPlayPause\n\nLa tecla queda presionada mientras sostienes el pad.", "Press “Record” and then the key combination, or type it.\n\nExamples: Ctrl+Shift+N · Win+D · Alt+Tab · F5\nSpecial keys: Space, Enter, Esc, Delete, Left, Right, Add (Num +), VolumeUp, VolumeMute, MediaPlayPause\n\nThe key stays pressed while you hold the pad."},
            {"Texto, programa o página web", "Text, program or website"},
            {"Examinar…", "Browse…"},
            {"«Gemido» al empezar (pitch bend)", "“Bend-in” at the start (pitch bend)"},
            {"Canal", "Channel"},
            {"Velocidad (0 = según golpe)", "Velocity (0 = from hit)"},
            {"Archivo de sonido", "Sound file"},
            {"Elegir sonido…", "Choose sound…"},
            {"Modo", "Mode"},
            {"Volumen (0 = golpe)", "Volume (0 = from hit)"},
            {"También puedes arrastrar archivos de audio desde el Explorador y soltarlos sobre un pad.", "You can also drag audio files from File Explorer and drop them on a pad."},
            {"Color en el pad", "Pad color"},
            {"Color del preset", "Preset color"},
            {"Usar este color", "Use this color"},
            {"▶  Probar", "▶  Test"},
            {"Los cambios se guardan solos.\nPad 1: modo · Pad 2: preset anterior · Pad 3: preset siguiente.", "Changes are saved automatically.\nPad 1: mode · Pad 2: previous preset · Pad 3: next preset."},
            {"Actualizar", "Refresh"},
            {"Detectar pads", "Detect pads"},
            {"Aviso en pantalla al cambiar", "On-screen notice when switching"},
            {"Iniciar con Windows", "Start with Windows"},
            {"Restaurar presets", "Restore presets"},
            {"Colores en el pad", "Pad colors"},
            {"Automático", "Automatic"},
            {"Controlador:", "Controller:"},
            {"Entrada:", "Input:"},
            {"Salida música:", "Music output:"},
            {"Abrir", "Open"},
            {"Modo música", "Music mode"},
            {"Modo oficina", "Office mode"},
            {"Salir", "Exit"},
            {"PadBox sigue activo", "PadBox is still running"},
            {"Los pads siguen funcionando. Ábrelo desde el icono junto al reloj.", "Your pads keep working. Open it from the icon next to the clock."},
            {"No se pudo guardar la configuración: ", "Could not save the settings: "},
            {"Windows no dejó activar el inicio automático. Actívalo en Configuración → Aplicaciones → Inicio.", "Windows did not allow automatic startup. Turn it on in Settings → Apps → Startup."},
            {"No se pudo cambiar el inicio automático: ", "Could not change automatic startup: "},
            {"(ninguna)", "(none)"},
            {"Conecta tu controlador MIDI por USB…", "Connect your MIDI controller via USB…"},
            {"Controlador desconectado. Esperando a que vuelva…", "Controller disconnected. Waiting for it to come back…"},
            {"Conectado a ", "Connected to "},
            {"). Toca un pad.", "). Press a pad."},
            {"No se pudo abrir «", "Could not open “"},
            {"». ¿Lo está usando otro programa (DAW o navegador)? Ciérralo y pulsa Actualizar.", "”. Is another program (a DAW or browser) using it? Close it and press Refresh."},
            {"No se pudo abrir la salida «", "Could not open the output “"},
            {"».", "”."},
            {"● Pad conectado", "● Pad connected"},
            {"● Sin pad", "● No pad"},
            {" · nota ", " · note "},
            {") · fuerza ", ") · velocity "},
            {"El pad mandó la nota ", "The pad sent note "},
            {"), que no está asignada. Usa «Detectar pads».", "), which is not assigned. Use “Detect pads”."},
            {"El atajo «", "The shortcut “"},
            {"» no es válido.", "” is not valid."},
            {"»: ", "”: "},
            {"La melodía de «", "The melody of “"},
            {"» tiene notas no válidas.", "” has invalid notes."},
            {"Nota ", "Note "},
            {" de ", " of "},
            {"Elige una «Salida música» abajo para oír el ritmo.", "Choose a “Music output” below to hear the beat."},
            {"Ritmo desconocido: ", "Unknown beat: "},
            {": no tiene sonido. Arrastra un archivo de audio sobre él o elígelo en el editor.", ": no sound yet. Drag an audio file onto it or choose one in the editor."},
            {"No encuentro el sonido «", "Can't find the sound “"},
            {"Los pads 1, 2 y 3 son fijos (modo y presets). Suelta el sonido en los pads 4 a 16.", "Pads 1, 2 and 3 are fixed (mode and presets). Drop the sound on pads 4 to 16."},
            {"No se pudo copiar «", "Could not copy “"},
            {"Sonido asignado al pad ", "Sound assigned to pad "},
            {" sonidos asignados desde el pad ", " sounds assigned starting at pad "},
            {"Elige un sonido para el pad ", "Choose a sound for pad "},
            {"Elige una «Salida música» abajo para oír los pads.", "Choose a “Music output” below to hear the pads."},
            {"Aceptar", "OK"},
            {"Cancelar", "Cancel"},
            {"Duplicar preset", "Duplicate preset"},
            {"Nuevo preset", "New preset"},
            {" (copia)", " (copy)"},
            {"Mi preset", "My preset"},
            {"Renombrar preset", "Rename preset"},
            {"Cada modo necesita al menos un preset.", "Each mode needs at least one preset."},
            {"¿Borrar el preset «", "Delete the preset “"},
            {"»?", "”?"},
            {"¿Restaurar los presets originales de los dos modos? Se pierden los cambios que hiciste en los presets (la detección de pads se conserva).", "Restore the original presets of both modes? Your changes to the presets will be lost (pad detection is kept)."},
            {"Detección cancelada.", "Detection canceled."},
            {"Primero conecta tu controlador (mira la barra de abajo).", "Connect your controller first (see the bar at the bottom)."},
            {"DETECTAR: toca el pad físico que quieres como Pad 1 (arriba a la izquierda en pantalla).", "DETECT: press the physical pad you want as Pad 1 (top left on screen)."},
            {"¡Listo! Los ", "Done! All "},
            {" pads quedaron detectados.", " pads have been detected."},
            {"DETECTAR: ahora toca el pad físico para el Pad ", "DETECT: now press the physical pad for Pad "},
            {"Cambiar modo", "Switch mode"},
            {"Preset anterior", "Previous preset"},
            {"Preset siguiente", "Next preset"},
            {"→ Música", "→ Music"},
            {"→ Oficina", "→ Office"},
            {"Escribe: ", "Types: "},
            {"Abre: ", "Opens: "},
            {" · canal ", " · channel "},
            {"Acorde ", "Chord "},
            {"Notas no válidas", "Invalid notes"},
            {"Melodía · ", "Melody · "},
            {" notas · toca para avanzar", " notes · press to advance"},
            {" on/off · canal ", " on/off · channel "},
            {"Programa ", "Program "},
            {"Sin sonido: arrastra un archivo aquí", "No sound: drag a file here"},
            {"Mantener · ", "Hold · "},
            {"Bucle · ", "Loop · "},
            {"Este pad alterna entre el modo MÚSICA y el modo OFICINA.\n\nCada modo guarda sus propios presets y recuerda cuál usaste por última vez.", "This pad switches between MUSIC mode and OFFICE mode.\n\nEach mode keeps its own presets and remembers the one you used last."},
            {"Este pad pasa al ", "This pad goes to the "},
            {"preset anterior", "previous preset"},
            {"preset siguiente", "next preset"},
            {" del modo actual:\n\n", " of the current mode:\n\n"},
            {"Notas en orden", "Notes in order"},
            {"Puede ser un programa (.exe), un acceso directo, una carpeta o una página web (https://…).", "It can be a program (.exe), a shortcut, a folder or a website (https://…)."},
            {"Cada toque suena la siguiente nota; si paras ", "Each press plays the next note; if you stop for "},
            {"2,5", "2.5"},
            {" s vuelve a empezar.\nNotas: Do Re Mi Fa Sol La Si (o C D E…), con # o b y octava. Ej: Sol#3 La3 Do4\nUsa | para separar frases. Canal ", " s it starts over.\nNotes: C D E F G A B (or Do Re Mi…), with # or b and an octave. E.g. G#3 A3 C4\nUse | to separate phrases. Channel "},
            {"El texto se escribe en la ventana activa, como si lo teclearas.", "The text is typed into the active window, as if you typed it."},
            {"Nota raíz", "Root note"},
            {"Programa", "Program"},
            {"Tempo (BPM)", "Tempo (BPM)"},
            {"Número de CC", "CC number"},
            {"Ritmo", "Beat"},
            {"Acorde", "Chord"},
            {"Valor (0 = 127)", "Value (0 = 127)"},
            {"Programas y accesos directos|*.exe;*.lnk;*.bat;*.cmd|Todos los archivos|*.*", "Programs and shortcuts|*.exe;*.lnk;*.bat;*.cmd|All files|*.*"},
            {"Elige el programa a abrir", "Choose the program to open"},
            {"Los atajos y el texto se envían a la ventana activa: abre el programa y toca el pad.", "Shortcuts and text go to the active window: open the program and press the pad."},
            {"Presiona…", "Press keys…"},
            {"Ocurrió un error: ", "An error occurred: "},
            {"PadBox ya está abierto. Búscalo junto al reloj de Windows.", "PadBox is already open. Look for it next to the Windows clock."},
            {"Idioma:", "Language:"},
            {"Español", "Español"},
            {"El idioma cambiará la próxima vez que abras PadBox. Con «Restaurar presets» los presets quedan en ese idioma.", "The language will change the next time you open PadBox. Use “Restore presets” to get the presets in that language."}
        };
    }

    // ---------- modelo ----------
    public class PadAction
    {
        public string Name { get; set; }
        public string Type { get; set; }   // keys, text, run, note, chord, seq, loop, cc, toggle, pc, none
        public string Value { get; set; }  // atajo, texto, ruta/web, notas de la melodía o nombre del ritmo
        public int Num { get; set; }
        public int Ch { get; set; }
        public int Vel { get; set; }
        public string Chord { get; set; }
        public bool Scoop { get; set; }    // melodía: "gemido" con pitch bend en la primera nota
        public string Color { get; set; }  // color del pad en el aparato ("#RRGGBB"); vacío = color del preset
        public PadAction() { Name = ""; Type = "none"; Value = ""; Ch = 1; Chord = "maj"; }
        public PadAction Clone() { return (PadAction)MemberwiseClone(); }
    }

    public class Preset
    {
        public string Name { get; set; }
        public List<PadAction> Pads { get; set; }   // pads 3..16
        public int Program { get; set; }            // instrumento GM 1-128 para el canal 1 (0 = no cambiar)
        public int Program2 { get; set; }           // instrumento GM 1-128 para el canal 2 (0 = no cambiar)
        public int DrumKit { get; set; }            // juego de batería GS para el canal 10 (programa + 1; 0 = estándar)
        public int SyncBpm { get; set; }            // bucles de sonido sincronizados a este tempo (0 = empiezan al tocar)
        public int SyncBars { get; set; }           // largo del ciclo de sincronía en compases
        public string Icon { get; set; }            // clave de Icons.All
    }

    public class Config
    {
        public List<int> InNotes { get; set; }      // nota que envía cada pad físico
        public List<Preset> Music { get; set; }
        public List<Preset> Office { get; set; }
        public bool OfficeMode { get; set; }
        public int MusicIdx { get; set; }
        public int OfficeIdx { get; set; }
        public string InPort { get; set; }
        public string OutPort { get; set; }
        public bool HideOsd { get; set; }
        public int Version { get; set; }            // para agregar presets nuevos a configuraciones viejas
        public bool NoPadLights { get; set; }       // true = no pintar los pads del aparato
        public Dictionary<string, string> PadOriginals { get; set; }   // colores originales de cada banco del pad (hex), para restaurar
        public string Device { get; set; }          // perfil de controlador elegido ("auto" = según el puerto)
        public string Lang { get; set; }            // idioma: null = el de Windows, "es" o "en" (se aplica al abrir la app)
        public Dictionary<string, List<int>> PadNotes { get; set; }   // notas detectadas por perfil de controlador
    }

    // ---------- controladores compatibles ----------
    // Cada perfil: cómo reconocer el aparato (texto en el nombre del puerto MIDI), cuántos pads usa PadBox,
    // las notas que mandan sus pads de fábrica (en el orden de la pantalla: de arriba a la izquierda) y qué luces admite.
    // Si las notas no coinciden (otro banco u otra configuración), "Detectar pads" las aprende tocándolos.
    class DeviceProfile
    {
        public string Id, Name;
        public string[] Match = new string[0];
        public int Pads = 16;
        public int[] Notes;
        public string Lights = "";     // "mvave" = SMC-PAD Pocket (protocolo de M-VAVE), "launchpad" = Novation (modo programador)
        public byte LaunchpadId;       // identificador SysEx del modelo de Launchpad
        public bool Tested;            // probado con el aparato real
    }

    static class Devices
    {
        // Distribución tipo MPC: el pad de abajo a la izquierda manda la nota 36
        static int[] Mpc(int pads) { int rows = pads / 4; return Enumerable.Range(0, pads).Select(i => 36 + (rows - 1 - i / 4) * 4 + i % 4).ToArray(); }
        // Launchpad en modo programador: nota = fila × 10 + columna (fila 1 abajo); PadBox usa el bloque de 4 × 4 de abajo a la izquierda
        static int[] LaunchpadBlock() { return Enumerable.Range(0, 16).Select(i => (4 - i / 4) * 10 + i % 4 + 1).ToArray(); }

        public static readonly List<DeviceProfile> All = new List<DeviceProfile> {
            new DeviceProfile { Id = "mvave-smcpad-pocket", Name = "M-VAVE SMC-PAD Pocket", Match = new[] { "SMC-PAD Pocket" }, Notes = Mpc(16), Lights = "mvave", Tested = true },
            new DeviceProfile { Id = "mvave-smcpad", Name = "M-VAVE SMC-PAD", Match = new[] { "SMC-PAD", "SMC PAD" }, Notes = Mpc(16) },
            new DeviceProfile { Id = "mvave-smk", Name = "M-VAVE SMK-25 / SMK-37", Match = new[] { "SMK-25", "SMK-37", "SMK25", "SMK37", "SMK" }, Pads = 8, Notes = Mpc(8) },
            new DeviceProfile { Id = "akai-mpd218", Name = "Akai MPD218", Match = new[] { "MPD218" }, Notes = Mpc(16) },
            new DeviceProfile { Id = "akai-mpd226", Name = "Akai MPD226 / MPD232", Match = new[] { "MPD226", "MPD232" }, Notes = Mpc(16) },
            new DeviceProfile { Id = "akai-lpd8", Name = "Akai LPD8", Match = new[] { "LPD8" }, Pads = 8, Notes = Mpc(8) },
            new DeviceProfile { Id = "akai-mpkmini", Name = "Akai MPK Mini", Match = new[] { "MPK mini", "MPKmini", "MPK Mini" }, Pads = 8, Notes = Mpc(8) },
            new DeviceProfile { Id = "novation-lpx", Name = "Novation Launchpad X", Match = new[] { "LPX", "Launchpad X" }, Notes = LaunchpadBlock(), Lights = "launchpad", LaunchpadId = 0x0C },
            new DeviceProfile { Id = "novation-lpminimk3", Name = "Novation Launchpad Mini MK3", Match = new[] { "LPMiniMK3", "Launchpad Mini" }, Notes = LaunchpadBlock(), Lights = "launchpad", LaunchpadId = 0x0D },
            new DeviceProfile { Id = "novation-lppromk3", Name = "Novation Launchpad Pro MK3", Match = new[] { "LPProMK3", "Launchpad Pro" }, Notes = LaunchpadBlock(), Lights = "launchpad", LaunchpadId = 0x0E },
            new DeviceProfile { Id = "generic16", Name = L.T("Otro controlador (16 pads)"), Notes = Mpc(16) },
            new DeviceProfile { Id = "generic8", Name = L.T("Otro controlador (8 pads)"), Pads = 8, Notes = Mpc(8) },
        };
        public static DeviceProfile Get(string id) { return All.FirstOrDefault(p => p.Id == id); }
        public static DeviceProfile Generic { get { return Get("generic16"); } }

        // Perfil del puerto (el primero cuyo texto aparezca en el nombre), o null si no es un modelo conocido
        public static DeviceProfile ForPort(string port)
        {
            if (string.IsNullOrEmpty(port)) return null;
            return All.FirstOrDefault(p => p.Match.Any(m => port.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0));
        }
        // Puertos que no son controladores (sintetizadores y puertos virtuales)
        public static bool IsSoftwarePort(string port)
        {
            return port.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0 || port.IndexOf("loopMIDI", StringComparison.OrdinalIgnoreCase) >= 0
                || port.IndexOf("Wavetable", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    // Pad 1: modo · pad 2: preset anterior · pad 3: preset siguiente · pads 4-16: acciones del preset
    static class PadLayout
    {
        public const int FixedPads = 3, Actions = 16 - FixedPads;
    }

    static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(20, 22, 28);
        public static readonly Color Panel = Color.FromArgb(30, 33, 41);
        public static readonly Color Field = Color.FromArgb(42, 46, 56);
        public static readonly Color Pad = Color.FromArgb(27, 30, 37);
        public static readonly Color Line = Color.FromArgb(60, 65, 78);
        public static readonly Color Text = Color.FromArgb(232, 234, 240);
        public static readonly Color Muted = Color.FromArgb(139, 147, 167);
        public static readonly Color Music = Color.FromArgb(124, 92, 255);
        public static readonly Color Office = Color.FromArgb(0, 184, 148);
        public static readonly Color Fixed = Color.FromArgb(255, 204, 0);
        public static readonly Color Warn = Color.FromArgb(245, 166, 35);
        public static readonly Color Ok = Color.FromArgb(46, 204, 113);
        public static readonly Color Err = Color.FromArgb(255, 92, 92);
        public static readonly Font Base = new Font("Segoe UI", 9.75f);
        public static readonly Font Bold = new Font("Segoe UI Semibold", 10.5f);
        public static readonly Font Small = new Font("Segoe UI", 8.25f);
        public static readonly Font Title = new Font("Segoe UI Semibold", 14f);

        public static Color Blend(Color a, Color b, double t)
        {
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // Íconos de los presets: un cuadro de color con un símbolo de la fuente de íconos de Windows.
    static class Icons
    {
        public class Def
        {
            public string Key, Name, Glyph;
            public Color Color;
            public Def(string key, string name, int glyph, int rgb) { Key = key; Name = name; Glyph = glyph == 0 ? "" : ((char)glyph).ToString(); Color = Color.FromArgb(255, Color.FromArgb(rgb)); }
        }

        public static readonly List<Def> All = new List<Def> {
            new Def("drum", L.T("Batería"), 0, 0xFF5A4F), new Def("cumbia", "Cumbia", 0xE8D6, 0xFF9500), new Def("song", L.T("Canción"), 0xE90B, 0xFF4FA3),
            new Def("rack", "Drum Rack", 0xE81E, 0xA36BFF), new Def("keys", L.T("Teclado"), 0xE765, 0x5B8CFF), new Def("note", L.T("Nota"), 0xEC4F, 0x34C759),
            new Def("notes", L.T("Notas"), 0xE189, 0x9B5CFF), new Def("sliders", L.T("Controles"), 0xE9E9, 0x1FB8CD), new Def("desktop", L.T("Escritorio"), 0xE7F4, 0x0A84FF),
            new Def("browser", L.T("Navegador"), 0xE774, 0x00B894), new Def("obs", L.T("Grabar / OBS"), 0xE7C8, 0xE5484D), new Def("stream", L.T("Transmitir"), 0xE93E, 0x8E44FF),
            new Def("pen", L.T("Pluma"), 0xE76D, 0xFF9A00), new Def("brush", L.T("Pincel"), 0xE771, 0x31A8FF), new Def("doc", L.T("Documento"), 0xE8A5, 0x4F6BED),
            new Def("palette", L.T("Paleta"), 0xE790, 0xFF6B9A), new Def("photo", L.T("Foto"), 0xE91B, 0x2FB5A0), new Def("camera", L.T("Cámara"), 0xE722, 0x6C7A89),
            new Def("video", "Video", 0xE714, 0xFF7043), new Def("mic", L.T("Micrófono"), 0xE720, 0xE84393), new Def("headphones", L.T("Audífonos"), 0xE7F6, 0x00A8FF),
            new Def("play", L.T("Reproducir"), 0xE768, 0x2ECC71), new Def("game", L.T("Juegos"), 0xE7FC, 0x7C5CFF), new Def("bolt", L.T("Rayo"), 0xE945, 0xF5B000),
            new Def("code", L.T("Código"), 0xE943, 0x26A69A), new Def("mail", L.T("Correo"), 0xE715, 0x1E88E5), new Def("folder", L.T("Carpeta"), 0xE8B7, 0xE6A100),
            new Def("calc", L.T("Calculadora"), 0xE8EF, 0x78909C), new Def("gear", L.T("Ajustes"), 0xE713, 0x8D99AE), new Def("star", L.T("Estrella"), 0xE734, 0xFFB300),
        };

        static readonly Dictionary<string, string> ByPreset = new Dictionary<string, string> {
            {L.T("Batería rock"), "drum"}, {L.T("Cumbia (percusión)"), "cumbia"}, {L.T("Guitarra blues"), "mic"}, {L.T("Piano de acordes"), "photo"}, {L.T("Mis sonidos"), "folder"}, {"Drum Pad EDM", "stream"},
            {L.T("Batería GM"), "drum"}, {"Cumbia", "cumbia"}, {L.T("Piano cumbia villera"), "keys"}, {L.T("Acordeón"), "notes"}, {L.T("Teclado cumbia rebajada"), "headphones"}, {L.T("Saxofón"), "play"}, {"Dubstep", "bolt"}, {L.T("Rack de batería"), "rack"},
            {L.T("Cromática C3"), "keys"}, {L.T("Escala Do mayor"), "note"}, {L.T("Pentatónica La menor"), "note"}, {L.T("Acordes en Do"), "notes"},
            {L.T("Botones CC (on/off)"), "sliders"}, {L.T("Escritorio"), "desktop"}, {L.T("Navegador"), "browser"}, {"OBS", "obs"},
            {"Illustrator", "pen"}, {"Photoshop", "brush"}, {"Word/Excel (español)", "doc"}, {"Word/Excel", "doc"} };

        public static Def Get(string key) { return All.FirstOrDefault(d => d.Key == key) ?? All.First(d => d.Key == "star"); }
        public static string Default(string presetName) { string k; return presetName != null && ByPreset.TryGetValue(presetName, out k) ? k : "star"; }

        static FontFamily family;
        static FontFamily Family
        {
            get
            {
                if (family == null)
                {
                    try { family = new FontFamily("Segoe Fluent Icons"); }
                    catch (ArgumentException) { family = new FontFamily("Segoe MDL2 Assets"); }
                }
                return family;
            }
        }

        public static void Draw(Graphics g, RectangleF r, string key)
        {
            var d = Get(key);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var gr = RectangleF.Inflate(r, 0, 1);
            using (var path = Theme.Round(r, r.Width * 0.24f))
            using (var b = new LinearGradientBrush(gr, Theme.Blend(d.Color, Color.White, .2), Theme.Blend(d.Color, Color.Black, .15), 90f))
                g.FillPath(b, path);
            if (d.Key == "drum") { DrawDrum(g, r); return; }
            using (var gp = new GraphicsPath())
            {
                gp.AddString(d.Glyph, Family, 0, r.Height * 0.5f, PointF.Empty, StringFormat.GenericTypographic);
                var bb = gp.GetBounds();
                using (var m = new Matrix())
                {
                    m.Translate(r.X + (r.Width - bb.Width) / 2 - bb.X, r.Y + (r.Height - bb.Height) / 2 - bb.Y);
                    gp.Transform(m);
                }
                g.FillPath(Brushes.White, gp);
            }
        }

        // No hay tambor en la fuente de Windows: se dibuja uno con baquetas.
        static void DrawDrum(Graphics g, RectangleF r)
        {
            float w = r.Width;
            using (var p = new Pen(Color.White, w * 0.065f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var top = new RectangleF(r.X + w * .22f, r.Y + w * .40f, w * .56f, w * .17f);
                float mid = top.Top + top.Height / 2, h = w * .22f;
                g.DrawEllipse(p, top);
                g.DrawLine(p, top.Left, mid, top.Left, mid + h);
                g.DrawLine(p, top.Right, mid, top.Right, mid + h);
                g.DrawArc(p, top.X, top.Y + h, top.Width, top.Height, 0, 180);
                g.DrawLine(p, r.X + w * .30f, r.Y + w * .17f, r.X + w * .48f, r.Y + w * .44f);
                g.DrawLine(p, r.X + w * .70f, r.Y + w * .17f, r.X + w * .54f, r.Y + w * .44f);
            }
        }

        public static Bitmap Bitmap(string key, int px)
        {
            var bmp = new Bitmap(px, px);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                Draw(g, new RectangleF(0.5f, 0.5f, px - 1.5f, px - 1.5f), key);
            }
            return bmp;
        }
    }

    static class Mus
    {
        public static readonly string[] Names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public static string NoteName(int n) { return Names[((n % 12) + 12) % 12] + (n / 12 - 2); } // C3 = 60
        static readonly string[] Solfa = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };
        public static string SolfaName(int n) { return L.En ? NoteName(n) : Solfa[((n % 12) + 12) % 12] + (n / 12 - 2); }   // misma octava que ParseSeq; en inglés, C D E…

        static readonly Dictionary<string, int[]> Chords = new Dictionary<string, int[]> {
            {"maj", new[] {0, 4, 7}}, {"min", new[] {0, 3, 7}}, {"dim", new[] {0, 3, 6}},
            {"maj7", new[] {0, 4, 7, 11}}, {"m7", new[] {0, 3, 7, 10}}, {"dom7", new[] {0, 4, 7, 10}},
            {"sus2", new[] {0, 2, 7}}, {"sus4", new[] {0, 5, 7}} };
        static readonly Dictionary<string, string> Display = new Dictionary<string, string> {
            {"maj", L.T("Mayor")}, {"min", L.T("Menor")}, {"dim", L.T("Disminuido")}, {"maj7", "Maj7"}, {"m7", "m7"}, {"dom7", "7"}, {"sus2", "Sus2"}, {"sus4", "Sus4"} };
        static readonly Dictionary<string, string> Suffix = new Dictionary<string, string> {
            {"maj", ""}, {"min", "m"}, {"dim", "°"}, {"maj7", "maj7"}, {"m7", "m7"}, {"dom7", "7"}, {"sus2", "sus2"}, {"sus4", "sus4"} };
        public static readonly string[] ChordIds = { "maj", "min", "dim", "maj7", "m7", "dom7", "sus2", "sus4" };

        public static string ChordName(string q) { string s; return q != null && Display.TryGetValue(q, out s) ? s : L.T("Mayor"); }
        public static string ChordLabel(int root, string q) { string s; Suffix.TryGetValue(q ?? "maj", out s); return Names[((root % 12) + 12) % 12] + (s ?? ""); }
        public static int[] Intervals(string q) { int[] iv; return q != null && Chords.TryGetValue(q, out iv) ? iv : Chords["maj"]; }

        static readonly Dictionary<string, int> Steps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            {"do", 0}, {"re", 2}, {"mi", 4}, {"fa", 5}, {"sol", 7}, {"la", 9}, {"si", 11},
            {"c", 0}, {"d", 2}, {"e", 4}, {"f", 5}, {"g", 7}, {"a", 9}, {"b", 11} };
        static readonly System.Text.RegularExpressions.Regex NoteRx =
            new System.Text.RegularExpressions.Regex(@"^(do|re|mi|fa|sol|la|si|[a-g])([#b]?)(\d?)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // "Mi4 Fa4 Sol4" o "E4 F4 G4" (sin octava = 4; C3 = 60). Devuelve null si alguna nota no es válida.
        // "|" separa frases: en phraseStarts quedan las posiciones donde empieza cada una.
        public static List<int> ParseSeq(string s, HashSet<int> phraseStarts = null)
        {
            var list = new List<int>();
            if (phraseStarts != null) phraseStarts.Add(0);
            foreach (var tok in (s ?? "").Split(new[] { ' ', ',', ';', '-', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (tok == "|") { if (phraseStarts != null) phraseStarts.Add(list.Count); continue; }
                var m = NoteRx.Match(tok);
                if (!m.Success) return null;
                int n = Steps[m.Groups[1].Value] + (m.Groups[2].Value == "#" ? 1 : m.Groups[2].Value.Length > 0 ? -1 : 0);
                n += ((m.Groups[3].Value.Length > 0 ? int.Parse(m.Groups[3].Value) : 4) + 2) * 12;
                if (n < 0 || n > 127) return null;
                list.Add(n);
            }
            return list;
        }
    }

    static class Defaults
    {
        static PadAction K(string name, string keys) { return new PadAction { Name = name, Type = "keys", Value = keys }; }
        static PadAction Run(string name, string target) { return new PadAction { Name = name, Type = "run", Value = target }; }
        static PadAction Note(string name, int num, int ch) { return new PadAction { Name = name, Type = "note", Num = num, Ch = ch }; }
        static PadAction Chord(int root, string q) { return new PadAction { Name = Mus.ChordLabel(root, q), Type = "chord", Num = root, Chord = q }; }
        static Preset P(string name, IEnumerable<PadAction> pads) { return new Preset { Name = name, Pads = pads.ToList() }; }
        static IEnumerable<int> Scale(int root, int[] iv, int count)
        {
            for (int i = 0; i < count; i++) yield return root + 12 * (i / iv.Length) + iv[i % iv.Length];
        }

        // Pad 4 de cada preset de música: un ritmo en bucle que combine con el preset
        static PadAction Loop(string name, string pattern, int bpm) { return new PadAction { Name = name, Type = "loop", Value = pattern, Num = bpm, Ch = 10, Color = Red }; }
        static Preset WithLoop(PadAction loop, Preset p)
        {
            p.Pads.Insert(0, loop);
            if (p.Pads.Count > PadLayout.Actions) p.Pads.RemoveRange(PadLayout.Actions, p.Pads.Count - PadLayout.Actions);
            return p;
        }

        // Cada preset trae PadLayout.Actions acciones (pads 4 a 16); los pads 1-3 son fijos.
        // Cada preset de música es único: su propio ritmo (pad 4), instrumento, tonalidad y juego de batería (GS, canal 10).
        // Juegos de batería GS (número de programa + 1): 1 estándar, 9 room, 17 power, 25 electrónico, 26 TR-808, 33 jazz, 41 escobillas
        public static List<Preset> Music()
        {
            int n = PadLayout.Actions - 1;   // el pad 4 es el ritmo
            return new List<Preset> {
                Kit(17, WithLoop(Loop(L.T("Ritmo rock"), "rock", 100), P(L.T("Batería rock"), new[] {
                    Note(L.T("Bombo"), 36, 10), Note(L.T("Redoblante"), 38, 10), Note(L.T("Aro"), 37, 10), Note(L.T("Hat cerrado"), 42, 10),
                    Note(L.T("Hat pedal"), 44, 10), Note(L.T("Hat abierto"), 46, 10), Note("Crash", 49, 10), Note("Ride", 51, 10),
                    Note(L.T("Tom piso"), 41, 10), Note(L.T("Tom bajo"), 45, 10), Note(L.T("Tom medio"), 47, 10), Note(L.T("Tom alto"), 50, 10) }))),
                Kit(1, Cumbia()),
                Kit(1, PianoVillera()),
                Kit(1, Acordeon()),
                Kit(9, Rebajada()),
                Kit(41, Saxofon()),
                Kit(25, Dubstep()),
                Kit(33, GuitarraBlues()),
                Kit(1, PianoAcordes()),
                Kit(1, MisSonidos()),
                DrumPadEdm(),
                Kit(26, WithLoop(Loop(L.T("Ritmo house"), "house", 124), P(L.T("Rack de batería"), Enumerable.Range(0, n).Select(i => Note("Slot " + (i + 1), 36 + i, 1))))),
                Kit(26, WithLoop(Loop(L.T("Ritmo techno"), "techno", 130), P(L.T("Botones CC (on/off)"), Enumerable.Range(0, n).Select(i => new PadAction { Name = L.T("Botón ") + (i + 1), Type = "toggle", Num = 20 + i })))),
            };
        }
        static Preset Kit(int kit, Preset p) { p.DrumKit = kit; return p; }

        // Sampler: pads de sonido (mp3, wav…) que el usuario llena arrastrando sus archivos. Empieza vacío: la app no copia sonidos de terceros.
        public static Preset MisSonidos()
        {
            var pads = new List<PadAction> { Loop(L.T("Ritmo reggaetón"), "dembow", 95) };
            while (pads.Count < PadLayout.Actions) pads.Add(new PadAction { Name = "", Type = "sound", Value = "" });
            return P(L.T("Mis sonidos"), pads);
        }

        // Bucles y efectos EDM ORIGINALES (creados por GenSonidos.exe en Sonidos\EDM, 126 BPM, Sol menor).
        // Los bucles entran sincronizados cada 2 compases; los colores agrupan por tipo.
        public static Preset DrumPadEdm()
        {
            Func<string, string, int, string, PadAction> S = (name, file, mode, color) =>
                new PadAction { Name = name, Type = "sound", Value = "Sonidos\\EDM\\" + file, Num = mode, Color = color };
            var p = P("Drum Pad EDM", new[] {
                S(L.T("Batería"), "01 Bateria.wav", 2, Red), S("Hats", "02 Hats.wav", 2, Red), S(L.T("Percusión"), "03 Percusion.wav", 2, Red),
                S(L.T("Bajo"), "04 Bajo.wav", 2, Orange), S(L.T("Bajo wub"), "05 Bajo wub.wav", 2, Orange),
                S(L.T("Acordes"), "06 Acordes.wav", 2, Blue), S("Pad", "07 Pad.wav", 2, Blue),
                S(L.T("Melodía"), "08 Melodia.wav", 2, Green), S(L.T("Arpegio"), "09 Arpegio.wav", 2, Green),
                S(L.T("Voz"), "10 Voz.wav", 2, Purple),
                S(L.T("Subida"), "11 Subida.wav", 0, Pink), S(L.T("Impacto"), "12 Impacto.wav", 0, Pink), S(L.T("Redoble"), "13 Redoble.wav", 0, Pink) });
            p.SyncBpm = 126; p.SyncBars = 2;
            return p;
        }

        // Guitarra eléctrica con distorsión: escala de blues de Do y ritmo shuffle
        public static Preset GuitarraBlues()
        {
            var pads = new List<PadAction> { Loop(L.T("Ritmo blues"), "blues", 90) };
            pads.AddRange(new[] { 60, 63, 65, 66, 67, 70, 72, 75, 77, 78, 79, 82 }.Select(n => Note(Mus.SolfaName(n), n, 2)));   // Do Mib Fa Fa# Sol Sib …
            var p = P(L.T("Guitarra blues"), pads);
            p.Program2 = 30; // guitarra con distorsión
            return Tonic(p, 0);
        }

        // Piano de cola con acordes en Do mayor y ritmo de balada
        public static Preset PianoAcordes()
        {
            var pads = new List<PadAction> { Loop(L.T("Ritmo balada"), "balada", 72) };
            pads.AddRange(new[] {
                Chord(60, "maj"), Chord(62, "min"), Chord(64, "min"), Chord(65, "maj"), Chord(67, "maj"), Chord(69, "min"),
                Chord(71, "dim"), Chord(60, "maj7"), Chord(62, "m7"), Chord(64, "m7"), Chord(65, "maj7"), Chord(67, "dom7") });
            var p = P(L.T("Piano de acordes"), pads);
            p.Program = 1;   // piano de cola
            return Tint(p);
        }

        // Dubstep (140 BPM): drop, golpes, bajos (canal 1, synth bass) y lead de sierra (canal 2), en Fa menor.
        // Notas sueltas de la escala: no se usa ninguna canción ni se nombra a ningún artista.
        public static Preset Dubstep()
        {
            var pads = new List<PadAction> {
                Loop(L.T("Ritmo dubstep"), "dubstep", 140),
                Note("Drop!", 57, 10), Note(L.T("Golpe"), 40, 10), Note(L.T("Bombo"), 35, 10) };   // crash 2, redoblante eléctrico, bombo grave
            pads.AddRange(new[] { 41, 44, 48, 51 }.Select(n => Note(L.T("Bajo ") + Mus.SolfaName(n), n, 1)));    // Fa1 Lab1 Do2 Mib2
            pads.AddRange(new[] { 65, 68, 72, 75, 77 }.Select(n => Note("Lead " + Mus.SolfaName(n), n, 2)));   // Fa3 … Fa4
            var p = P("Dubstep", pads);
            p.Program = 39;   // synth bass 1
            p.Program2 = 82;  // lead sawtooth
            p.Pads[1].Color = Pink;
            p.Pads[2].Color = p.Pads[3].Color = White;
            foreach (var a in p.Pads.Skip(4).Take(4)) a.Color = Purple;
            foreach (var a in p.Pads.Skip(8)) a.Color = Cyan;
            return p;
        }

        // Ritmo en rojo, acordes en naranja, melodía completa en un pad en blanco
        public static Preset Tint(Preset p)
        {
            foreach (var a in p.Pads)
            {
                if (a.Type == "loop") a.Color = Red;
                else if (a.Type == "chord") a.Color = Orange;
                else if (a.Type == "seq" && a.Num > 0) a.Color = White;
            }
            return p;
        }

        // Cumbia clásica: solo percusión latina (sin notas ni acordes, así no se repite con los presets melódicos)
        static Preset Cumbia()
        {
            return P(L.T("Cumbia (percusión)"), new[] {
                Loop(L.T("Ritmo cumbia"), "cumbia", 92),
                Note(L.T("Güiro largo"), 74, 10), Note(L.T("Güiro corto"), 73, 10), Note(L.T("Conga abierta"), 63, 10), Note(L.T("Conga muda"), 62, 10),
                Note(L.T("Conga baja"), 64, 10), Note(L.T("Timbal alto"), 65, 10), Note(L.T("Timbal bajo"), 66, 10), Note(L.T("Cencerro"), 56, 10),
                Note("Maracas", 70, 10), Note("Claves", 75, 10), Note(L.T("Bongó alto"), 60, 10), Note(L.T("Bongó bajo"), 61, 10) });
        }

        public static Preset Escritorio()
        {
            return P(L.T("Escritorio"), Flat(
                C(Cyan, K(L.T("Copiar"), "Ctrl+C"), K(L.T("Pegar"), "Ctrl+V")),
                C(Orange, K(L.T("Deshacer"), "Ctrl+Z"), K(L.T("Rehacer"), "Ctrl+Y")),
                C(Purple, K(L.T("Cambiar ventana"), "Alt+Tab"), K(L.T("Ver escritorio"), "Win+D")),
                C(Pink, K(L.T("Captura de pantalla"), "Win+Shift+S")),
                C(Green, K(L.T("Pista anterior"), "MediaPreviousTrack"), K(L.T("Play / Pausa"), "MediaPlayPause"), K(L.T("Pista siguiente"), "MediaNextTrack")),
                C(Blue, K(L.T("Silenciar"), "VolumeMute"), K(L.T("Bajar volumen"), "VolumeDown"), K(L.T("Subir volumen"), "VolumeUp"))));
        }

        // OBS no trae atajos por defecto: se usan F13-F24 (no existen en el teclado, así no chocan con nada)
        // y se asignan una vez en OBS > Configuración > Atajos tocando el pad.
        public static Preset Obs()
        {
            return P("OBS", Flat(
                C(Red, K(L.T("Grabar on/off"), "F13"), K(L.T("Transmitir on/off"), "F14"), K(L.T("Pausar grabación"), "F15"), K(L.T("Guardar repetición"), "F16")),
                C(Blue, K(L.T("Escena 1"), "F17"), K(L.T("Escena 2"), "F18"), K(L.T("Escena 3"), "F19"), K(L.T("Escena 4"), "F20")),
                C(Orange, K(L.T("Silenciar micrófono"), "F21"), K(L.T("Silenciar escritorio"), "F22")),
                C(Pink, K(L.T("Captura de OBS"), "F23")),
                C(Purple, K(L.T("Cámara virtual"), "F24"), K(L.T("Mostrar/ocultar cámara"), "Ctrl+F13"))));
        }

        // Piano eléctrico villero: Re menor armónica (con Do#) y ritmo de cuarteto
        public static Preset PianoVillera()
        {
            var notes = new[] { 62, 64, 65, 67, 69, 70, 73, 74, 76, 77, 79, 81 }; // Re3 Mi3 Fa3 Sol3 La3 Sib3 Do#4 Re4 … La4
            var pads = new List<PadAction> { Loop(L.T("Ritmo cuarteto"), "cuarteto", 112) };
            pads.AddRange(notes.Select(n => Note(Mus.SolfaName(n), n, 2)));
            var p = P(L.T("Piano cumbia villera"), pads);
            p.Program2 = 5; // piano eléctrico
            return Tonic(p, 2);
        }

        // La tónica de cada escala en blanco, para ubicarse en el pad (pc = clase de nota: 0 Do, 2 Re, 4 Mi, 5 Fa, 7 Sol, 9 La)
        static Preset Tonic(Preset p, int pc)
        {
            foreach (var a in p.Pads)
                if (a.Type == "note" && a.Num % 12 == pc && string.IsNullOrEmpty(a.Color)) a.Color = White;
            return p;
        }

        // Acordeón vallenato: Sol mayor, melodía (canal 2) y acordes G C D Em (canal 1)
        public static Preset Acordeon()
        {
            var pads = new List<PadAction> { Loop(L.T("Ritmo vallenato"), "vallenato", 115) };
            pads.AddRange(new[] { 67, 69, 71, 72, 74, 76, 78, 79 }.Select(n => Note(Mus.SolfaName(n), n, 2)));   // Sol3 … Sol4
            pads.AddRange(new[] { Chord(55, "maj"), Chord(48, "maj"), Chord(50, "maj"), Chord(52, "min") });   // G C D Em
            var p = P(L.T("Acordeón"), pads);
            p.Program = 22; p.Program2 = 22;
            return Tonic(Tint(p), 7);
        }

        // Cumbia rebajada: órgano grave en Mi menor armónica (con Re#) y ritmo lento y pesado
        public static Preset Rebajada()
        {
            var notes = new[] { 52, 54, 55, 57, 59, 60, 63, 64, 66, 67, 69, 71 };   // Mi2 Fa#2 Sol2 La2 Si2 Do3 Re#3 Mi3 …
            var pads = new List<PadAction> { Loop(L.T("Ritmo rebajado"), "rebajada", 70) };
            pads.AddRange(notes.Select(n => Note(Mus.SolfaName(n), n, 2)));
            var p = P(L.T("Teclado cumbia rebajada"), pads);
            p.Program2 = 17; // órgano
            return Tonic(Tint(p), 4);
        }

        // Saxofón alto: Fa mayor y ritmo de bossa nova
        public static Preset Saxofon()
        {
            var pads = new List<PadAction> { Loop(L.T("Ritmo bossa nova"), "bossa", 120) };
            pads.AddRange(new[] { 65, 67, 69, 70, 72, 74, 76, 77, 79, 81, 82, 84 }.Select(n => Note(Mus.SolfaName(n), n, 2)));   // Fa3 … Do5
            var p = P(L.T("Saxofón"), pads);
            p.Program2 = 66; // saxo alto
            return Tonic(Tint(p), 5);
        }

        // Colores de los grupos de pads (los pads 2 y 3 usan el amarillo, así que no se usa para acciones)
        public const string Red = "#FF3B30", Orange = "#FF9500", Green = "#34C759", Cyan = "#32D7E6", Blue = "#0A84FF", Purple = "#9B5CFF", Pink = "#FF4FA3", White = "#F2F2F2";
        static PadAction[] C(string color, params PadAction[] pads) { foreach (var a in pads) a.Color = color; return pads; }
        static PadAction[] Flat(params PadAction[][] groups) { return groups.SelectMany(g => g).ToArray(); }

        public static List<Preset> Office()
        {
            return new List<Preset> {
                Escritorio(),
                P(L.T("Navegador"), Flat(
                    C(Cyan, K(L.T("Nueva pestaña"), "Ctrl+T"), K(L.T("Cerrar pestaña"), "Ctrl+W"), K(L.T("Reabrir pestaña"), "Ctrl+Shift+T")),
                    C(Purple, K(L.T("Pestaña anterior"), "Ctrl+Shift+Tab"), K(L.T("Pestaña siguiente"), "Ctrl+Tab")),
                    C(Green, K(L.T("Atrás"), "Alt+Left"), K(L.T("Adelante"), "Alt+Right"), K(L.T("Recargar"), "F5")),
                    C(Orange, K(L.T("Barra de direcciones"), "Ctrl+L"), K(L.T("Buscar en la página"), "Ctrl+F")),
                    C(Blue, K("Zoom +", "Ctrl+Add"), K("Zoom −", "Ctrl+Subtract")),
                    C(Red, Run("Google", "https://www.google.com")))),
                Obs(),
                P("Illustrator", Flat(
                    C(Cyan, K(L.T("Selección"), "V"), K(L.T("Selección directa"), "A"), K(L.T("Pluma"), "P"), K(L.T("Texto"), "T"), K(L.T("Rectángulo"), "M"), K(L.T("Elipse"), "L"), K(L.T("Pincel"), "B")),
                    C(Purple, K(L.T("Mano (mantener)"), "Space")),
                    C(Orange, K(L.T("Deshacer"), "Ctrl+Z"), K(L.T("Rehacer"), "Ctrl+Shift+Z")),
                    C(Green, K(L.T("Agrupar"), "Ctrl+G"), K(L.T("Desagrupar"), "Ctrl+Shift+G")),
                    C(Blue, K(L.T("Ajustar a ventana"), "Ctrl+0")))),
                P("Photoshop", Flat(
                    C(Cyan, K(L.T("Mover"), "V"), K(L.T("Marco"), "M"), K(L.T("Lazo"), "L"), K(L.T("Pincel"), "B"), K(L.T("Borrador"), "E"), K(L.T("Tampón"), "S"), K(L.T("Texto"), "T")),
                    C(Purple, K(L.T("Mano (mantener)"), "Space")),
                    C(Orange, K(L.T("Deshacer"), "Ctrl+Z"), K(L.T("Rehacer"), "Ctrl+Shift+Z")),
                    C(Green, K(L.T("Duplicar capa"), "Ctrl+J"), K(L.T("Transformar"), "Ctrl+T")),
                    C(Pink, K(L.T("Intercambiar colores"), "X")))),
                // Word y Excel usan atajos distintos según el idioma de Office (en español: Ctrl+G guardar, Ctrl+N negrita…)
                L.En ? P("Word/Excel", Flat(
                    C(Red, K("Save", "Ctrl+S")),
                    C(Cyan, K("Copy", "Ctrl+C"), K("Paste", "Ctrl+V")),
                    C(Orange, K("Undo", "Ctrl+Z"), K("Redo", "Ctrl+Y")),
                    C(Purple, K("Bold", "Ctrl+B"), K("Italic", "Ctrl+I"), K("Underline", "Ctrl+U")),
                    C(Green, K("Find", "Ctrl+F"), K("Replace", "Ctrl+H")),
                    C(Blue, K("Print", "Ctrl+P"), K("New", "Ctrl+N"), K("Select all", "Ctrl+A")))) :
                P("Word/Excel (español)", Flat(
                    C(Red, K("Guardar", "Ctrl+G")),
                    C(Cyan, K(L.T("Copiar"), "Ctrl+C"), K(L.T("Pegar"), "Ctrl+V")),
                    C(Orange, K(L.T("Deshacer"), "Ctrl+Z"), K(L.T("Rehacer"), "Ctrl+Y")),
                    C(Purple, K("Negrita", "Ctrl+N"), K("Cursiva", "Ctrl+K"), K("Subrayado", "Ctrl+S")),
                    C(Green, K("Buscar", "Ctrl+B"), K("Reemplazar", "Ctrl+L")),
                    C(Blue, K("Imprimir", "Ctrl+P"), K(L.T("Nuevo"), "Ctrl+U"), K("Seleccionar todo", "Ctrl+E")))),
            };
        }
    }

    // ---------- atajos de teclado ----------
    static class Shortcut
    {
        static readonly Dictionary<string, int> Mods = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
            {"Ctrl", 0x11}, {"Control", 0x11}, {"Shift", 0x10}, {"Mayús", 0x10}, {"Alt", 0x12}, {"Win", 0x5B} };
        static readonly Dictionary<string, Keys> Alias = new Dictionary<string, Keys>(StringComparer.OrdinalIgnoreCase) {
            {"Esc", Keys.Escape}, {"Enter", Keys.Return}, {"Supr", Keys.Delete}, {"Del", Keys.Delete}, {"Espacio", Keys.Space},
            {"PgUp", Keys.PageUp}, {"PgDn", Keys.PageDown}, {"Izquierda", Keys.Left}, {"Derecha", Keys.Right}, {"Arriba", Keys.Up}, {"Abajo", Keys.Down} };
        static readonly Dictionary<string, string> Nice = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            {"Add", "Num +"}, {"Subtract", "Num −"}, {"Left", "←"}, {"Right", "→"}, {"Up", "↑"}, {"Down", "↓"}, {"Space", L.T("Espacio")},
            {"Return", "Enter"}, {"Escape", "Esc"}, {"VolumeUp", "Vol +"}, {"VolumeDown", "Vol −"}, {"VolumeMute", L.T("Silencio")},
            {"MediaPlayPause", L.T("Play/Pausa")}, {"MediaNextTrack", L.T("Pista siguiente")}, {"MediaPreviousTrack", L.T("Pista anterior")},
            {"Next", "PgDn"}, {"PageDown", "PgDn"}, {"Prior", "PgUp"}, {"PageUp", "PgUp"}, {"Delete", L.T("Supr")}, {"Back", L.T("Retroceso")} };

        // Devuelve los códigos de tecla virtuales en orden de pulsación, o null si el texto no es válido.
        public static List<int> Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var vks = new List<int>();
            foreach (var part in s.Split('+'))
            {
                var t = part.Trim();
                int m; Keys k;
                if (t.Length == 0) return null;
                if (Mods.TryGetValue(t, out m)) { vks.Add(m); continue; }
                if (Alias.TryGetValue(t, out k)) { vks.Add((int)k); continue; }
                if (t.Length == 1 && char.IsDigit(t[0])) { vks.Add((int)Keys.D0 + (t[0] - '0')); continue; }
                if (!int.TryParse(t, out m) && Enum.TryParse(t, true, out k) && ((int)k & ~0xFFFF) == 0) { vks.Add((int)k); continue; }
                return null;
            }
            return vks;
        }

        public static string Pretty(string s)
        {
            if (Parse(s) == null) return L.T("Atajo no válido");
            return string.Join(" + ", s.Split('+').Select(p => {
                var t = p.Trim(); string n;
                if (t.Length == 2 && t[0] == 'D' && char.IsDigit(t[1])) return t.Substring(1);
                return Nice.TryGetValue(t, out n) ? n : t;
            }));
        }

        public static string FromKeys(Keys data, bool win)
        {
            var parts = new List<string>();
            if ((data & Keys.Control) != 0) parts.Add("Ctrl");
            if ((data & Keys.Shift) != 0) parts.Add("Shift");
            if ((data & Keys.Alt) != 0) parts.Add("Alt");
            if (win) parts.Add("Win");
            var k = data & Keys.KeyCode;
            parts.Add(k >= Keys.D0 && k <= Keys.D9 ? ((int)(k - Keys.D0)).ToString() : k.ToString());
            return string.Join("+", parts);
        }
    }

    static class Kbd
    {
        [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr extra; }
        [StructLayout(LayoutKind.Sequential)] struct KEYBDINPUT { public ushort wVk, wScan; public uint dwFlags, time; public IntPtr extra; }
        [StructLayout(LayoutKind.Explicit)] struct UNION { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint type; public UNION u; }
        [DllImport("user32.dll")] static extern uint SendInput(uint n, INPUT[] inputs, int size);
        [DllImport("user32.dll")] static extern uint MapVirtualKey(uint code, uint mapType);
        const uint KEYUP = 2, EXTENDED = 1, UNICODE = 4;
        static readonly HashSet<int> Ext = new HashSet<int> { 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x2C, 0x2D, 0x2E, 0x5B, 0x5C, 0x5D, 0x6F, 0x90, 0xA3, 0xA5 };

        static INPUT Key(int vk, bool up)
        {
            var i = new INPUT { type = 1 };
            i.u.ki.wVk = (ushort)vk;
            i.u.ki.wScan = (ushort)MapVirtualKey((uint)vk, 0);
            i.u.ki.dwFlags = (up ? KEYUP : 0) | (Ext.Contains(vk) ? EXTENDED : 0);
            return i;
        }
        static INPUT Uni(char c, bool up)
        {
            var i = new INPUT { type = 1 };
            i.u.ki.wScan = c;
            i.u.ki.dwFlags = UNICODE | (up ? KEYUP : 0);
            return i;
        }
        static void Send(IList<INPUT> arr) { if (arr.Count > 0) SendInput((uint)arr.Count, arr.ToArray(), Marshal.SizeOf(typeof(INPUT))); }

        public static void Down(List<int> vks) { Send(vks.Select(v => Key(v, false)).ToList()); }
        public static void Up(List<int> vks) { Send(Enumerable.Reverse(vks).Select(v => Key(v, true)).ToList()); }
        public static void Type(string s)
        {
            var list = new List<INPUT>();
            foreach (char c in s)
            {
                if (c == '\r') continue;
                if (c == '\n') { list.Add(Key(0x0D, false)); list.Add(Key(0x0D, true)); continue; }
                list.Add(Uni(c, false)); list.Add(Uni(c, true));
            }
            Send(list);
        }
    }

    // ---------- MIDI (winmm) ----------
    static class Winmm
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MIDIINCAPS { public ushort wMid, wPid; public uint ver; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string name; public uint support; }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MIDIOUTCAPS { public ushort wMid, wPid; public uint ver; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string name; public ushort tech, voices, notes, chmask; public uint support; }
        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIHDR { public IntPtr data; public uint len, rec; public IntPtr user; public uint flags; public IntPtr next, res; public uint off; [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public IntPtr[] r2; }
        public delegate void MidiInProc(IntPtr h, uint msg, IntPtr inst, IntPtr p1, IntPtr p2);
        [DllImport("winmm.dll")] public static extern uint midiInPrepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] public static extern uint midiInUnprepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] public static extern uint midiInAddBuffer(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] public static extern uint midiOutPrepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] public static extern uint midiOutUnprepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] public static extern uint midiOutLongMsg(IntPtr h, IntPtr hdr, uint size);
        public static readonly int HdrSize = Marshal.SizeOf(typeof(MIDIHDR));

        [DllImport("winmm.dll")] public static extern uint midiInGetNumDevs();
        [DllImport("winmm.dll")] public static extern uint midiOutGetNumDevs();
        [DllImport("winmm.dll", CharSet = CharSet.Auto)] public static extern uint midiInGetDevCaps(UIntPtr id, out MIDIINCAPS caps, uint size);
        [DllImport("winmm.dll", CharSet = CharSet.Auto)] public static extern uint midiOutGetDevCaps(UIntPtr id, out MIDIOUTCAPS caps, uint size);
        [DllImport("winmm.dll")] public static extern uint midiInOpen(out IntPtr h, uint id, MidiInProc cb, IntPtr inst, uint flags);
        [DllImport("winmm.dll")] public static extern uint midiInStart(IntPtr h);
        [DllImport("winmm.dll")] public static extern uint midiInStop(IntPtr h);
        [DllImport("winmm.dll")] public static extern uint midiInReset(IntPtr h);
        [DllImport("winmm.dll")] public static extern uint midiInClose(IntPtr h);
        [DllImport("winmm.dll")] public static extern uint midiOutOpen(out IntPtr h, uint id, IntPtr cb, IntPtr inst, uint flags);
        [DllImport("winmm.dll")] public static extern uint midiOutShortMsg(IntPtr h, uint msg);
        [DllImport("winmm.dll")] public static extern uint midiOutReset(IntPtr h);
        [DllImport("winmm.dll")] public static extern uint midiOutClose(IntPtr h);

        public static List<string> Inputs()
        {
            var l = new List<string>();
            uint n = midiInGetNumDevs();
            for (uint i = 0; i < n; i++) { MIDIINCAPS c; l.Add(midiInGetDevCaps((UIntPtr)i, out c, (uint)Marshal.SizeOf(typeof(MIDIINCAPS))) == 0 ? c.name : "?"); }
            return l;
        }
        public static List<string> Outputs()
        {
            var l = new List<string>();
            uint n = midiOutGetNumDevs();
            for (uint i = 0; i < n; i++) { MIDIOUTCAPS c; l.Add(midiOutGetDevCaps((UIntPtr)i, out c, (uint)Marshal.SizeOf(typeof(MIDIOUTCAPS))) == 0 ? c.name : "?"); }
            return l;
        }
    }

    class MidiIn : IDisposable
    {
        IntPtr h;
        volatile bool closing;
        readonly Winmm.MidiInProc proc;   // referencia viva para que el GC no la recoja
        readonly List<IntPtr> bufs = new List<IntPtr>();
        public string Name;
        public event Action<int, int, int> Message;
        public event Action<byte[]> SysEx;   // se llama desde el hilo del driver
        public event Action Closed;
        public MidiIn() { proc = Callback; }

        public bool Open(int id, string name)
        {
            Name = name;
            if (Winmm.midiInOpen(out h, (uint)id, proc, IntPtr.Zero, 0x30000) != 0) { h = IntPtr.Zero; return false; }
            for (int k = 0; k < 4; k++)   // búferes para recibir SysEx (respuestas de configuración del pad)
            {
                var hp = Marshal.AllocHGlobal(Winmm.HdrSize);
                var hdr = new Winmm.MIDIHDR { data = Marshal.AllocHGlobal(4096), len = 4096, r2 = new IntPtr[4] };
                Marshal.StructureToPtr(hdr, hp, false);
                Winmm.midiInPrepareHeader(h, hp, (uint)Winmm.HdrSize);
                Winmm.midiInAddBuffer(h, hp, (uint)Winmm.HdrSize);
                bufs.Add(hp);
            }
            return Winmm.midiInStart(h) == 0;
        }
        void Callback(IntPtr hh, uint msg, IntPtr inst, IntPtr p1, IntPtr p2)
        {
            try
            {
                if (msg == 0x3C3) { int d = (int)p1.ToInt64(); var m = Message; if (m != null) m(d & 0xFF, (d >> 8) & 0xFF, (d >> 16) & 0xFF); }
                else if (msg == 0x3C4)
                {
                    var hdr = (Winmm.MIDIHDR)Marshal.PtrToStructure(p1, typeof(Winmm.MIDIHDR));
                    if (hdr.rec > 0)
                    {
                        var b = new byte[hdr.rec];
                        Marshal.Copy(hdr.data, b, 0, (int)hdr.rec);
                        var s = SysEx; if (s != null) s(b);
                    }
                    if (!closing) Winmm.midiInAddBuffer(hh, p1, (uint)Winmm.HdrSize);
                }
                else if (msg == 0x3C2 && !closing) { var c = Closed; if (c != null) c(); }
            }
            catch { }
        }
        public void Dispose()
        {
            if (h == IntPtr.Zero) return;
            closing = true;
            Winmm.midiInStop(h); Winmm.midiInReset(h);
            foreach (var hp in bufs)
            {
                Winmm.midiInUnprepareHeader(h, hp, (uint)Winmm.HdrSize);
                var hdr = (Winmm.MIDIHDR)Marshal.PtrToStructure(hp, typeof(Winmm.MIDIHDR));
                Marshal.FreeHGlobal(hdr.data);
                Marshal.FreeHGlobal(hp);
            }
            bufs.Clear();
            Winmm.midiInClose(h);
            h = IntPtr.Zero;
        }
    }

    class MidiOut : IDisposable
    {
        IntPtr h;
        public string Name;
        public bool Open(int id, string name)
        {
            Name = name;
            if (Winmm.midiOutOpen(out h, (uint)id, IntPtr.Zero, IntPtr.Zero, 0) == 0) return true;
            h = IntPtr.Zero; return false;
        }
        public void Send(int s, int d1, int d2)
        {
            if (h != IntPtr.Zero) Winmm.midiOutShortMsg(h, (uint)((s & 0xFF) | ((d1 & 0x7F) << 8) | ((d2 & 0x7F) << 16)));
        }
        // Envía un SysEx y espera a que el driver lo termine de enviar
        public bool SendSysEx(byte[] msg)
        {
            if (h == IntPtr.Zero) return false;
            IntPtr hp = Marshal.AllocHGlobal(Winmm.HdrSize), data = Marshal.AllocHGlobal(msg.Length);
            try
            {
                Marshal.Copy(msg, 0, data, msg.Length);
                var hdr = new Winmm.MIDIHDR { data = data, len = (uint)msg.Length, rec = (uint)msg.Length, r2 = new IntPtr[4] };
                Marshal.StructureToPtr(hdr, hp, false);
                if (Winmm.midiOutPrepareHeader(h, hp, (uint)Winmm.HdrSize) != 0) return false;
                uint r = Winmm.midiOutLongMsg(h, hp, (uint)Winmm.HdrSize);
                var sw = Stopwatch.StartNew();
                while (r == 0 && sw.ElapsedMilliseconds < 1000 && (((Winmm.MIDIHDR)Marshal.PtrToStructure(hp, typeof(Winmm.MIDIHDR))).flags & 1) == 0)
                    System.Threading.Thread.Sleep(1);
                Winmm.midiOutUnprepareHeader(h, hp, (uint)Winmm.HdrSize);
                return r == 0;
            }
            finally { Marshal.FreeHGlobal(data); Marshal.FreeHGlobal(hp); }
        }
        public void Dispose()
        {
            if (h == IntPtr.Zero) return;
            Winmm.midiOutReset(h); Winmm.midiOutClose(h);
            h = IntPtr.Zero;
        }
    }

    // Dónde vive cada cosa. La versión portable (.zip) guarda todo junto al .exe. La de la Microsoft Store corre
    // empaquetada (MSIX): su carpeta de instalación es de solo lectura, así que la configuración y los sonidos del
    // usuario van a %LOCALAPPDATA%\PadBox; los sonidos que trae la app se siguen leyendo de la carpeta de instalación.
    static class AppPaths
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern int GetCurrentPackageFullName(ref int length, StringBuilder name);
        const int APPMODEL_ERROR_NO_PACKAGE = 15700;
        static bool? packaged;

        public static bool Packaged
        {
            get
            {
                if (packaged == null)
                {
                    try { int len = 0; packaged = GetCurrentPackageFullName(ref len, null) != APPMODEL_ERROR_NO_PACKAGE; }
                    catch (EntryPointNotFoundException) { packaged = false; }
                }
                return packaged.Value;
            }
        }
        public static string App { get { return AppDomain.CurrentDomain.BaseDirectory; } }
        public static string Data
        {
            get
            {
                if (!Packaged) return App;
                var d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PadBox");
                Directory.CreateDirectory(d);
                return d;
            }
        }
    }

    // Inicio con Windows en la versión de la Store: usa la "tarea de inicio" declarada en el paquete (el registro
    // no sirve para apps empaquetadas). Todo por reflexión, para no depender de los metadatos de WinRT al compilar.
    static class PackagedStartup
    {
        const string TaskId = "PadBoxStartup";
        const int ActivationKindStartupTask = 1020;

        static Type WinRt(string name) { return Type.GetType(name + ", Windows.ApplicationModel, ContentType=WindowsRuntime", true); }
        static object Await(object op, Type result)
        {
            var ext = Type.GetType("System.WindowsRuntimeSystemExtensions, System.Runtime.WindowsRuntime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", true);
            var asTask = ext.GetMethods().First(m => m.Name == "AsTask" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name == "IAsyncOperation`1");
            var task = (System.Threading.Tasks.Task)asTask.MakeGenericMethod(result).Invoke(null, new[] { op });
            task.Wait();
            return task.GetType().GetProperty("Result").GetValue(task, null);
        }
        static object Task()
        {
            var t = WinRt("Windows.ApplicationModel.StartupTask");
            return Await(t.GetMethod("GetAsync").Invoke(null, new object[] { TaskId }), t);
        }
        static int StateOf(object task) { return Convert.ToInt32(WinRt("Windows.ApplicationModel.StartupTask").GetProperty("State").GetValue(task, null)); }

        // Estados: 0 desactivada, 1 desactivada por el usuario, 2 activada, 3 bloqueada por directiva, 4 activada por directiva
        public static bool Enabled() { int s = StateOf(Task()); return s == 2 || s == 4; }
        public static bool Enable()
        {
            var t = Task();
            var stateType = WinRt("Windows.ApplicationModel.StartupTaskState");
            int s = Convert.ToInt32(Await(WinRt("Windows.ApplicationModel.StartupTask").GetMethod("RequestEnableAsync").Invoke(t, null), stateType));
            return s == 2 || s == 4;
        }
        public static void Disable() { var t = Task(); WinRt("Windows.ApplicationModel.StartupTask").GetMethod("Disable").Invoke(t, null); }

        // ¿Windows abrió PadBox al iniciar sesión? Entonces arranca escondido junto al reloj.
        public static bool LaunchedAtStartup()
        {
            var args = WinRt("Windows.ApplicationModel.AppInstance").GetMethod("GetActivatedEventArgs").Invoke(null, null);
            if (args == null) return false;
            var kind = WinRt("Windows.ApplicationModel.Activation.IActivatedEventArgs").GetProperty("Kind").GetValue(args, null);
            return Convert.ToInt32(kind) == ActivationKindStartupTask;
        }
    }

    // Archivos de sonido de los pads: se copian a la carpeta "Sonidos" (junto a la app, o en los datos de la app en
    // la versión de la Store) y se guardan con ruta relativa, así no se pierden si el archivo original se mueve o se borra.
    static class Sounds
    {
        public static readonly string[] Extensions = { ".mp3", ".wav", ".wma", ".m4a", ".aac", ".aif", ".aiff", ".mp2", ".snd", ".au" };
        public static readonly string Filter = L.T("Sonidos|*.mp3;*.wav;*.wma;*.m4a;*.aac;*.aif;*.aiff;*.mp2;*.snd;*.au|Todos los archivos|*.*");
        static string Dir { get { return Path.Combine(AppPaths.Data, "Sonidos"); } }

        public static bool IsAudio(string path) { return Extensions.Contains((Path.GetExtension(path) ?? "").ToLowerInvariant()); }
        // Rutas relativas: primero los sonidos del usuario, después los que trae la app (p. ej. Sonidos\EDM)
        public static string Resolve(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            if (Path.IsPathRooted(value)) return value;
            var mine = Path.Combine(AppPaths.Data, value);
            return File.Exists(mine) ? mine : Path.Combine(AppPaths.App, value);
        }
        public static string Import(string src)
        {
            var full = Path.GetFullPath(src);
            Directory.CreateDirectory(Dir);
            if (full.StartsWith(Dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return "Sonidos\\" + Path.GetFileName(full);
            string name = Path.GetFileName(full), dest = Path.Combine(Dir, name);
            long size = new FileInfo(full).Length;
            for (int k = 2; File.Exists(dest) && new FileInfo(dest).Length != size; k++)
                dest = Path.Combine(Dir, Path.GetFileNameWithoutExtension(name) + " (" + k + ")" + Path.GetExtension(name));
            if (!File.Exists(dest)) File.Copy(full, dest);
            return "Sonidos\\" + Path.GetFileName(dest);
        }
        public static string NameFor(string file)
        {
            var n = Path.GetFileNameWithoutExtension(file) ?? "";
            return n.Length > 24 ? n.Substring(0, 24) : n;
        }
    }

    // Sonido WAV (PCM de 16 bits a 44,1 kHz) cargado en memoria para el mezclador
    class Clip
    {
        public float[] S;   // muestras intercaladas
        public int Ch;      // 1 = mono, 2 = estéreo
        public long Frames { get { return S.Length / Ch; } }

        // Devuelve null si el archivo no es un WAV que el mezclador pueda tocar (entonces se usa el reproductor de Windows)
        public static Clip Load(string path)
        {
            if (!path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) return null;
            try
            {
                var b = File.ReadAllBytes(path);
                if (b.Length < 44 || Encoding.ASCII.GetString(b, 0, 4) != "RIFF" || Encoding.ASCII.GetString(b, 8, 4) != "WAVE") return null;
                int pos = 12, ch = 0, rate = 0, bits = 0, fmt = 0;
                while (pos + 8 <= b.Length)
                {
                    string id = Encoding.ASCII.GetString(b, pos, 4);
                    int size = BitConverter.ToInt32(b, pos + 4);
                    if (id == "fmt ") { fmt = BitConverter.ToInt16(b, pos + 8); ch = BitConverter.ToInt16(b, pos + 10); rate = BitConverter.ToInt32(b, pos + 12); bits = BitConverter.ToInt16(b, pos + 22); }
                    else if (id == "data")
                    {
                        if (fmt != 1 || bits != 16 || rate != Mixer.Rate || (ch != 1 && ch != 2)) return null;
                        int n = Math.Min(size, b.Length - pos - 8) / 2;
                        var s = new float[n - n % ch];
                        for (int i = 0; i < s.Length; i++) s[i] = BitConverter.ToInt16(b, pos + 8 + i * 2) / 32768f;
                        return new Clip { S = s, Ch = ch };
                    }
                    pos += 8 + size + (size & 1);
                }
            }
            catch (IOException) { }
            return null;
        }
    }

    // Mezclador propio (waveOut) para los WAV: muchos sonidos a la vez, con poca demora, y bucles que entran
    // sincronizados al inicio del siguiente ciclo, como en las apps de "drum pad".
    class Mixer : IDisposable
    {
        public const int Rate = 44100;
        const int Frames = 512, Buffers = 4;
        [StructLayout(LayoutKind.Sequential)] struct WAVEFORMATEX { public ushort tag, ch; public uint rate, bytesPerSec; public ushort align, bits, size; }
        [StructLayout(LayoutKind.Sequential)] struct WAVEHDR { public IntPtr data; public uint len, rec; public IntPtr user; public uint flags, loops; public IntPtr next, res; }
        [DllImport("winmm.dll")] static extern int waveOutOpen(out IntPtr h, uint dev, ref WAVEFORMATEX fmt, IntPtr cb, IntPtr inst, uint flags);
        [DllImport("winmm.dll")] static extern int waveOutPrepareHeader(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutUnprepareHeader(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutWrite(IntPtr h, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutReset(IntPtr h);
        [DllImport("winmm.dll")] static extern int waveOutClose(IntPtr h);
        [DllImport("winmm.dll")] static extern uint timeBeginPeriod(uint ms);
        [DllImport("winmm.dll")] static extern uint timeEndPeriod(uint ms);

        class Voice { public int Pad; public Clip C; public long Start, Pos; public bool Loop; public float Vol; }
        readonly List<Voice> voices = new List<Voice>();
        readonly object lk = new object();
        readonly IntPtr[] hdrs = new IntPtr[Buffers], datas = new IntPtr[Buffers];
        readonly int hdrSize = Marshal.SizeOf(typeof(WAVEHDR));
        IntPtr h;
        long clock;          // cuadros ya mezclados
        long epoch = -1;     // inicio de la rejilla de bucles
        volatile bool run;
        System.Threading.Thread th;

        public bool Ok { get { return h != IntPtr.Zero; } }

        public Mixer()
        {
            var fmt = new WAVEFORMATEX { tag = 1, ch = 2, rate = Rate, bits = 16, align = 4, bytesPerSec = Rate * 4 };
            if (waveOutOpen(out h, 0xFFFFFFFF, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) != 0) { h = IntPtr.Zero; return; }
            for (int b = 0; b < Buffers; b++)
            {
                datas[b] = Marshal.AllocHGlobal(Frames * 4);
                hdrs[b] = Marshal.AllocHGlobal(hdrSize);
                Marshal.StructureToPtr(new WAVEHDR { data = datas[b], len = (uint)(Frames * 4) }, hdrs[b], false);
                waveOutPrepareHeader(h, hdrs[b], hdrSize);
            }
            run = true;
            th = new System.Threading.Thread(Run) { IsBackground = true, Priority = System.Threading.ThreadPriority.Highest };
            th.Start();
        }

        void Run()
        {
            timeBeginPeriod(1);
            var pcm = new short[Frames * 2];
            var primed = new bool[Buffers];
            while (run)
            {
                for (int b = 0; b < Buffers && run; b++)
                {
                    var hd = (WAVEHDR)Marshal.PtrToStructure(hdrs[b], typeof(WAVEHDR));
                    if (primed[b] && (hd.flags & 1) == 0) continue;   // todavía sonando
                    Render(pcm);
                    Marshal.Copy(pcm, 0, datas[b], pcm.Length);
                    waveOutWrite(h, hdrs[b], hdrSize);
                    primed[b] = true;
                }
                System.Threading.Thread.Sleep(1);
            }
            timeEndPeriod(1);
        }

        void Render(short[] pcm)
        {
            lock (lk)
            {
                for (int i = 0; i < Frames; i++)
                {
                    long t = clock + i;
                    float l = 0, r = 0;
                    for (int v = voices.Count - 1; v >= 0; v--)
                    {
                        var vo = voices[v];
                        if (t < vo.Start) continue;
                        long n = vo.C.Frames;
                        if (vo.Pos >= n) { if (vo.Loop) vo.Pos = 0; else { voices.RemoveAt(v); continue; } }
                        int k = (int)(vo.Pos * vo.C.Ch);
                        float sl = vo.C.S[k], sr = vo.C.Ch == 2 ? vo.C.S[k + 1] : sl;
                        l += sl * vo.Vol; r += sr * vo.Vol;
                        vo.Pos++;
                    }
                    pcm[i * 2] = (short)(Math.Tanh(l * 0.9) * 32000);   // limitador suave: no satura con muchos sonidos
                    pcm[i * 2 + 1] = (short)(Math.Tanh(r * 0.9) * 32000);
                }
                clock += Frames;
                if (!voices.Any(v => v.Loop)) epoch = -1;
            }
        }

        // grid = largo del ciclo en cuadros (0 = empezar ya). Si ya suena un bucle, el nuevo espera al inicio del próximo ciclo.
        public void Play(int pad, Clip c, float vol, bool loop, int grid)
        {
            if (!Ok || c == null || c.Frames == 0) return;
            lock (lk)
            {
                voices.RemoveAll(v => v.Pad == pad);
                long start = clock;
                if (loop && grid > 0)
                {
                    if (epoch >= 0 && voices.Any(v => v.Loop)) start = epoch + (start - epoch + grid - 1) / grid * grid;
                    else epoch = start;
                }
                voices.Add(new Voice { Pad = pad, C = c, Start = start, Loop = loop, Vol = vol });
                LastStart = start; LastEpoch = epoch;
            }
        }
        public long LastStart, LastEpoch;   // para pruebas
        public void Stop(int pad) { lock (lk) voices.RemoveAll(v => v.Pad == pad); }
        public void StopAll() { lock (lk) voices.Clear(); }

        public void Dispose()
        {
            if (!Ok) return;
            run = false;
            if (th != null) th.Join(500);
            waveOutReset(h);
            for (int b = 0; b < Buffers; b++) { waveOutUnprepareHeader(h, hdrs[b], hdrSize); Marshal.FreeHGlobal(hdrs[b]); Marshal.FreeHGlobal(datas[b]); }
            waveOutClose(h);
            h = IntPtr.Zero;
        }
    }

    // Reproduce archivos de audio (mp3, wav, wma, m4a…) con MCI de Windows. Cada pad tiene su propio "alias",
    // así varios sonidos pueden sonar a la vez. Se usa desde el hilo de la interfaz.
    class SamplePlayer : IDisposable
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)] static extern int mciSendString(string cmd, StringBuilder ret, int len, IntPtr cb);
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)] static extern bool mciGetErrorString(int err, StringBuilder text, int len);
        readonly Dictionary<int, string> loaded = new Dictionary<int, string>();   // pad → archivo abierto
        public string LastError = "";

        static string Alias(int pad) { return "smcpad" + pad; }
        int Send(string cmd)
        {
            int r = mciSendString(cmd, null, 0, IntPtr.Zero);
            if (r != 0) { var sb = new StringBuilder(256); mciGetErrorString(r, sb, sb.Capacity); LastError = sb.ToString(); }
            return r;
        }

        public bool Load(int pad, string path)
        {
            string cur;
            if (loaded.TryGetValue(pad, out cur) && string.Equals(cur, path, StringComparison.OrdinalIgnoreCase)) return true;
            Close(pad);
            if (Send("open \"" + path + "\" type mpegvideo alias " + Alias(pad)) != 0) return false;
            loaded[pad] = path;
            return true;
        }
        // volume 0-1000
        public void Play(int pad, int volume, bool loop)
        {
            if (!loaded.ContainsKey(pad)) return;
            Send("setaudio " + Alias(pad) + " volume to " + Math.Max(0, Math.Min(1000, volume)));
            Send("play " + Alias(pad) + " from 0" + (loop ? " repeat" : ""));
        }
        public void Stop(int pad) { if (loaded.ContainsKey(pad)) Send("stop " + Alias(pad)); }
        public void StopAll() { foreach (var p in loaded.Keys.ToList()) Stop(p); }
        public void Close(int pad) { if (loaded.Remove(pad)) Send("close " + Alias(pad)); }
        // Deja abiertos solo los pads indicados (los del preset actual)
        public void Keep(ICollection<int> pads) { foreach (var p in loaded.Keys.Where(k => !pads.Contains(k)).ToList()) Close(p); }
        public void Dispose() { foreach (var p in loaded.Keys.ToList()) Close(p); }
    }

    // Toca un patrón de batería en bucle en un hilo aparte (pasos de semicorchea).
    class Looper
    {
        [DllImport("winmm.dll")] static extern uint timeBeginPeriod(uint ms);
        [DllImport("winmm.dll")] static extern uint timeEndPeriod(uint ms);

        // Ritmos disponibles (clave, nombre para mostrar), en el orden del editor
        public static readonly string[] Keys = { "cumbia", "villera", "cuarteto", "vallenato", "rebajada", "rock", "balada", "blues", "bossa", "dembow", "house", "techno", "dubstep" };
        public static readonly string[] Display = { "Cumbia", L.T("Cumbia villera"), L.T("Cuarteto"), "Vallenato", L.T("Cumbia rebajada"), "Rock", L.T("Balada"), "Blues (shuffle)", "Bossa nova", L.T("Reggaetón (dembow)"), "House", "Techno", "Dubstep" };
        public static string DisplayName(string key) { int i = Array.IndexOf(Keys, (key ?? "").ToLowerInvariant()); return i >= 0 ? Display[i] : key; }

        // Un compás de 16 semicorcheas: {paso 0-15, nota GM, velocidad}
        static readonly Dictionary<string, int[][]> Patterns = new Dictionary<string, int[][]>(StringComparer.OrdinalIgnoreCase) {
            {"cumbia", new[] {
                new[] {0, 36, 110}, new[] {8, 36, 100},                                   // bombo en 1 y 3
                new[] {0, 74, 90}, new[] {2, 73, 70}, new[] {3, 73, 60}, new[] {4, 74, 85}, // güiro: largo-corto-corto
                new[] {6, 73, 70}, new[] {7, 73, 60}, new[] {8, 74, 90}, new[] {10, 73, 70},
                new[] {11, 73, 60}, new[] {12, 74, 85}, new[] {14, 73, 70}, new[] {15, 73, 60},
                new[] {4, 62, 80}, new[] {12, 62, 80}, new[] {6, 63, 100}, new[] {14, 63, 100}, new[] {15, 64, 90} } },
            {"villera", new[] {
                new[] {0, 36, 115}, new[] {8, 36, 105},                                   // bombo
                new[] {0, 74, 90}, new[] {4, 74, 85}, new[] {8, 74, 90}, new[] {12, 74, 85}, // güiro largo en cada tiempo
                new[] {2, 73, 75}, new[] {6, 73, 75}, new[] {10, 73, 75}, new[] {14, 73, 75}, // güiro corto a contratiempo
                new[] {1, 42, 45}, new[] {3, 42, 50}, new[] {5, 42, 45}, new[] {7, 42, 50},  // hat suave
                new[] {9, 42, 45}, new[] {11, 42, 50}, new[] {13, 42, 45}, new[] {15, 42, 50},
                new[] {12, 38, 80}, new[] {7, 65, 95}, new[] {15, 65, 95}, new[] {14, 66, 85} } },   // redoble y timbales
            {"rock", new[] {
                new[] {0, 36, 115}, new[] {8, 36, 105}, new[] {10, 36, 95},
                new[] {4, 38, 115}, new[] {12, 38, 115},
                new[] {0, 42, 80}, new[] {2, 42, 65}, new[] {4, 42, 80}, new[] {6, 42, 65},
                new[] {8, 42, 80}, new[] {10, 42, 65}, new[] {12, 42, 80}, new[] {14, 42, 65} } },
            {"cuarteto", new[] {                                                         // "tunga-tunga" cordobés
                new[] {0, 36, 115}, new[] {8, 36, 110},
                new[] {6, 45, 95}, new[] {14, 45, 95},                                    // tom: el "tun"
                new[] {4, 37, 100}, new[] {12, 37, 100},                                  // aro: el "ga"
                new[] {2, 54, 70}, new[] {6, 54, 60}, new[] {10, 54, 70}, new[] {14, 54, 60},   // pandereta
                new[] {1, 42, 40}, new[] {5, 42, 40}, new[] {9, 42, 40}, new[] {13, 42, 40} } },
            {"vallenato", new[] {
                new[] {0, 36, 110}, new[] {6, 36, 90}, new[] {8, 36, 105},               // bombo
                new[] {0, 64, 100}, new[] {6, 64, 90}, new[] {8, 64, 100}, new[] {14, 64, 85},   // caja grave
                new[] {4, 62, 95}, new[] {12, 62, 95}, new[] {3, 63, 80}, new[] {11, 63, 80},    // caja aguda
                new[] {0, 69, 90}, new[] {1, 69, 50}, new[] {2, 69, 65}, new[] {3, 69, 50},      // guacharaca
                new[] {4, 69, 85}, new[] {5, 69, 50}, new[] {6, 69, 65}, new[] {7, 69, 50},
                new[] {8, 69, 90}, new[] {9, 69, 50}, new[] {10, 69, 65}, new[] {11, 69, 50},
                new[] {12, 69, 85}, new[] {13, 69, 50}, new[] {14, 69, 65}, new[] {15, 69, 50} } },
            {"rebajada", new[] {                                                         // lenta y pesada
                new[] {0, 36, 120}, new[] {7, 36, 95}, new[] {8, 36, 115},
                new[] {0, 74, 80}, new[] {8, 74, 80},                                     // güiro largo solo en 1 y 3
                new[] {4, 70, 75}, new[] {12, 70, 75},                                    // maracas
                new[] {6, 64, 95}, new[] {14, 64, 95}, new[] {15, 63, 80} } },            // conga grave
            {"balada", new[] {
                new[] {0, 36, 100}, new[] {7, 36, 70}, new[] {8, 36, 95},
                new[] {4, 37, 85}, new[] {12, 37, 85},                                    // aro cruzado
                new[] {0, 42, 55}, new[] {2, 42, 40}, new[] {4, 42, 55}, new[] {6, 42, 40},
                new[] {8, 42, 55}, new[] {10, 42, 40}, new[] {12, 42, 55}, new[] {14, 42, 40} } },
            {"blues", new[] {                                                            // shuffle: largo-corto
                new[] {0, 36, 110}, new[] {8, 36, 105}, new[] {11, 36, 80},
                new[] {4, 38, 105}, new[] {12, 38, 105},
                new[] {0, 51, 80}, new[] {3, 51, 55}, new[] {4, 51, 80}, new[] {7, 51, 55},     // ride
                new[] {8, 51, 80}, new[] {11, 51, 55}, new[] {12, 51, 80}, new[] {15, 51, 55} } },
            {"bossa", new[] {
                new[] {0, 36, 95}, new[] {3, 36, 75}, new[] {8, 36, 95}, new[] {11, 36, 75},
                new[] {0, 37, 85}, new[] {3, 37, 85}, new[] {6, 37, 85}, new[] {10, 37, 85}, new[] {13, 37, 85},   // clave bossa
                new[] {0, 42, 45}, new[] {2, 42, 35}, new[] {4, 42, 45}, new[] {6, 42, 35},
                new[] {8, 42, 45}, new[] {10, 42, 35}, new[] {12, 42, 45}, new[] {14, 42, 35} } },
            {"dembow", new[] {                                                           // "bum-chi-bum-chik"
                new[] {0, 36, 120}, new[] {4, 36, 115}, new[] {8, 36, 120}, new[] {12, 36, 115},
                new[] {3, 38, 105}, new[] {6, 38, 100}, new[] {11, 38, 105}, new[] {14, 38, 100},
                new[] {0, 42, 60}, new[] {2, 42, 50}, new[] {4, 42, 60}, new[] {6, 42, 50},
                new[] {8, 42, 60}, new[] {10, 42, 50}, new[] {12, 42, 60}, new[] {14, 42, 50} } },
            {"techno", new[] {
                new[] {0, 36, 125}, new[] {4, 36, 120}, new[] {8, 36, 125}, new[] {12, 36, 120},
                new[] {2, 51, 75}, new[] {6, 51, 75}, new[] {10, 51, 75}, new[] {14, 51, 75},   // ride a contratiempo
                new[] {7, 37, 70}, new[] {15, 37, 80}, new[] {12, 39, 85},
                new[] {1, 42, 40}, new[] {3, 42, 55}, new[] {5, 42, 40}, new[] {9, 42, 40}, new[] {11, 42, 55}, new[] {13, 42, 40} } },
            {"house", new[] {
                new[] {0, 36, 118}, new[] {4, 36, 115}, new[] {8, 36, 118}, new[] {12, 36, 115},   // bombo en negras
                new[] {4, 39, 100}, new[] {12, 39, 100},
                new[] {2, 46, 80}, new[] {6, 46, 80}, new[] {10, 46, 80}, new[] {14, 46, 80},   // hat abierto a contratiempo
                new[] {1, 42, 45}, new[] {5, 42, 45}, new[] {9, 42, 45}, new[] {13, 42, 45} } },
            {"dubstep", new[] {
                new[] {0, 36, 125}, new[] {10, 36, 105}, new[] {3, 36, 80},                   // medio tiempo: bombo
                new[] {8, 38, 127}, new[] {8, 39, 100},                                      // golpe fuerte en el 3
                new[] {0, 42, 70}, new[] {2, 42, 55}, new[] {4, 42, 70}, new[] {6, 42, 55},
                new[] {10, 42, 55}, new[] {12, 42, 70}, new[] {14, 42, 55}, new[] {15, 42, 45},
                new[] {14, 38, 60} } },
        };

        volatile bool run;
        System.Threading.Thread th;

        public bool Start(MidiOut o, string pattern, int ch, int bpm)
        {
            int[][] pat;
            if (o == null || !Patterns.TryGetValue(pattern ?? "", out pat)) return false;
            run = true;
            double stepMs = 60000.0 / Math.Max(40, bpm) / 4;
            th = new System.Threading.Thread(() => {
                timeBeginPeriod(1);
                var sw = Stopwatch.StartNew();
                var prev = new List<int>();
                for (long step = 0; run; step++)
                {
                    while (run && sw.Elapsed.TotalMilliseconds < step * stepMs) System.Threading.Thread.Sleep(1);
                    if (!run) break;
                    foreach (var n in prev) o.Send(0x80 | ch, n, 0);
                    prev.Clear();
                    int s = (int)(step % 16);
                    foreach (var e in pat) if (e[0] == s) { o.Send(0x90 | ch, e[1], e[2]); prev.Add(e[1]); }
                }
                foreach (var n in prev) o.Send(0x80 | ch, n, 0);
                timeEndPeriod(1);
            }) { IsBackground = true, Priority = System.Threading.ThreadPriority.AboveNormal };
            th.Start();
            return true;
        }
        public void Stop()
        {
            run = false;
            if (th != null) { th.Join(300); th = null; }
        }
    }

    // Protocolo de configuración de M-VAVE para el SMC-PAD Pocket (documentado por el proyecto DiscoFloor,
    // github.com/krisvandecruys/DiscoFloor, docs/PROTOCOL.md). Paquete: 00 59 OP LEN_LO LEN_HI 00 BODY CHECKSUM,
    // empaquetado como flujo de bits LSB primero en grupos de 7 bits entre F0 y F7.
    static class SmcProto
    {
        public class Packet { public int Op; public byte[] Body; }
        public const int Ack = 0x00, Discover = 0x11, Write = 0x22, Read = 0x23;

        public static byte[] Build(int op, byte[] body)
        {
            var raw = new List<byte> { 0x00, 0x59, (byte)op, (byte)body.Length, (byte)(body.Length >> 8), 0x00 };
            raw.AddRange(body);
            int sum = 0;
            foreach (var b in body) sum += b;
            raw.Add((byte)((255 - sum) & 255));
            return Pack(raw.ToArray());
        }

        static byte[] Pack(byte[] raw)
        {
            int bits = raw.Length * 8, n = (bits + 6) / 7;
            var o = new byte[n + 2];
            o[0] = 0xF0; o[n + 1] = 0xF7;
            for (int i = 0; i < n; i++)
            {
                int v = 0;
                for (int b = 0; b < 7; b++)
                {
                    int bit = i * 7 + b;
                    if (bit < bits && ((raw[bit >> 3] >> (bit & 7)) & 1) != 0) v |= 1 << b;
                }
                o[i + 1] = (byte)v;
            }
            return o;
        }

        public static Packet Parse(byte[] sx)
        {
            if (sx == null || sx.Length < 4 || sx[0] != 0xF0) return null;
            int end = Array.IndexOf(sx, (byte)0xF7);
            if (end < 0) end = sx.Length;
            int count = end - 1, bytes = count * 7 / 8;
            var raw = new byte[bytes];
            for (int bit = 0; bit < bytes * 8; bit++)
                if (((sx[1 + bit / 7] >> (bit % 7)) & 1) != 0) raw[bit >> 3] |= (byte)(1 << (bit & 7));
            if (raw.Length < 7 || raw[0] != 0x00 || raw[1] != 0x59) return null;
            int len = raw[3] | (raw[4] << 8);
            if (raw.Length < 6 + len) return null;
            var body = new byte[len];
            Array.Copy(raw, 6, body, 0, len);
            return new Packet { Op = raw[2], Body = body };
        }

        // Cuerpo de lectura/escritura: registro, offset (4 bytes LE), cantidad (3 bytes LE), datos
        public static byte[] RegBody(int reg, int off, int count, byte[] data)
        {
            var b = new List<byte> { (byte)reg, (byte)off, (byte)(off >> 8), (byte)(off >> 16), (byte)(off >> 24), (byte)count, (byte)(count >> 8), (byte)(count >> 16) };
            if (data != null) b.AddRange(data);
            return b.ToArray();
        }
    }

    // Pinta los pads del SMC-PAD Pocket. Solo escribe el color RGB de los pads del banco activo en la memoria
    // de trabajo del aparato (no guarda en flash) y devuelve los colores originales al cerrar.
    // Luces de un controlador: pinta cada pad (según su nota) y al cerrar lo deja como estaba
    interface IPadLights
    {
        void Apply(Color[] colors, int[] notes);
        void OnSysEx(byte[] data);
        void Shutdown(bool restore);
    }

    // Novation Launchpad (X, Mini MK3, Pro MK3) en modo programador: color RGB por pad con un solo SysEx.
    // Protocolo público de Novation ("Programmer's reference"). No probado con un aparato real.
    class LaunchpadLights : IPadLights
    {
        readonly MidiOut o;
        readonly byte id;
        readonly HashSet<int> painted = new HashSet<int>();
        public LaunchpadLights(MidiOut o, byte id) { this.o = o; this.id = id; }
        byte[] Head(byte cmd) { return new byte[] { 0xF0, 0x00, 0x20, 0x29, 0x02, id, cmd }; }
        public void Apply(Color[] colors, int[] notes)
        {
            var msg = new List<byte>(Head(0x03));
            for (int i = 0; i < colors.Length && i < notes.Length; i++)
            {
                if (notes[i] < 11 || notes[i] > 99) continue;
                msg.AddRange(new byte[] { 0x03, (byte)notes[i], (byte)(colors[i].R >> 1), (byte)(colors[i].G >> 1), (byte)(colors[i].B >> 1) });   // RGB 0-127
                painted.Add(notes[i]);
            }
            msg.Add(0xF7);
            o.SendSysEx(msg.ToArray());
        }
        public void OnSysEx(byte[] data) { }
        public void Shutdown(bool restore)
        {
            if (restore)
            {
                var msg = new List<byte>(Head(0x03));
                foreach (var n in painted) msg.AddRange(new byte[] { 0x00, (byte)n, 0x00 });   // apagar
                msg.Add(0xF7);
                o.SendSysEx(msg.ToArray());
                o.SendSysEx(new byte[] { 0xF0, 0x00, 0x20, 0x29, 0x02, id, 0x0E, 0x00, 0xF7 });   // volver al modo normal
            }
            o.Dispose();
        }
    }

    class PadLights : IDisposable, IPadLights
    {
        const int SlotSize = 2931, RecSize = 26, BankBytes = 16 * RecSize;
        readonly MidiOut o;
        readonly System.Collections.Concurrent.BlockingCollection<SmcProto.Packet> replies = new System.Collections.Concurrent.BlockingCollection<SmcProto.Packet>();
        readonly System.Threading.AutoResetEvent signal = new System.Threading.AutoResetEvent(false);
        readonly object pendLock = new object(), ioLock = new object();
        readonly Dictionary<int, byte[]> originals;   // inicio del banco → RGB originales (16 × 3)
        readonly System.Threading.Thread worker;
        Color[] pending; int[] pendingNotes;
        volatile bool stop;
        public event Action<string> Report;
        public event Action<int, byte[]> OriginalCaptured;   // para guardarlos y poder restaurarlos aunque la app se cierre de golpe

        public PadLights(MidiOut o, Dictionary<int, byte[]> knownOriginals)
        {
            this.o = o;
            originals = new Dictionary<int, byte[]>(knownOriginals);
            worker = new System.Threading.Thread(Run) { IsBackground = true };
            worker.Start();
        }

        public void OnSysEx(byte[] data)
        {
            var p = SmcProto.Parse(data);
            if (p != null) replies.Add(p);
        }

        public void Apply(Color[] colors, int[] notes)
        {
            lock (pendLock) { pending = colors; pendingNotes = notes; }
            signal.Set();
        }

        SmcProto.Packet Request(int op, byte[] body, int expect, int timeoutMs)
        {
            SmcProto.Packet p;
            while (replies.TryTake(out p)) { }
            if (!o.SendSysEx(SmcProto.Build(op, body))) return null;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
                if (replies.TryTake(out p, 50) && p.Op == expect) return p;
            return null;
        }
        public string Identify()
        {
            var p = Request(SmcProto.Discover, new byte[0], SmcProto.Discover, 800);
            return p == null ? null : Encoding.ASCII.GetString(p.Body).TrimEnd('\0');
        }
        public byte[] ReadReg(int reg, int off, int count)
        {
            var p = Request(SmcProto.Read, SmcProto.RegBody(reg, off, count, null), SmcProto.Read, 800);
            if (p == null || p.Body.Length < 8 + count) return null;
            var d = new byte[count];
            Array.Copy(p.Body, 8, d, 0, count);
            return d;
        }
        public bool WriteReg(int reg, int off, byte[] data)
        {
            return Request(SmcProto.Write, SmcProto.RegBody(reg, off, data.Length, data), SmcProto.Ack, 800) != null;
        }

        // Inicio en el registro 5 de los 16 pads del banco activo del preset activo del aparato
        public int ActiveBankBase()
        {
            var rt = ReadReg(4, 0, 8);
            if (rt == null) return -1;
            int slot = (rt[7] & 3) * SlotSize;
            var bk = ReadReg(5, slot + 2928, 1);
            return bk == null ? -1 : slot + (bk[0] % 7) * BankBytes;
        }

        void Run()
        {
            while (true)
            {
                signal.WaitOne();
                if (stop) return;
                Color[] c; int[] n;
                lock (pendLock) { c = pending; n = pendingNotes; pending = null; }
                if (c == null) continue;
                lock (ioLock)
                {
                    try { ApplyNow(c, n); }
                    catch (Exception ex) { Say(L.T("Luces: ") + ex.Message); }
                }
            }
        }

        void ApplyNow(Color[] c, int[] notes)
        {
            int bankBase = ActiveBankBase();
            byte[] recs = bankBase < 0 ? null : ReadReg(5, bankBase, BankBytes);
            if (recs == null) { Say(L.T("Luces: el SMC-PAD no respondió.")); return; }
            if (!originals.ContainsKey(bankBase))
            {
                var orig = new byte[48];
                for (int r = 0; r < 16; r++) Array.Copy(recs, r * RecSize + 5, orig, r * 3, 3);
                originals[bankBase] = orig;
                var oc = OriginalCaptured; if (oc != null) oc(bankBase, orig);
            }
            for (int i = 0; i < 16; i++)
            {
                if (stop) return;
                lock (pendLock) { if (pending != null) return; }   // llegó un cambio más nuevo
                int r = -1;
                for (int k = 0; k < 16; k++) if (recs[k * RecSize + 2] == notes[i]) { r = k; break; }
                if (r < 0) continue;
                int at = r * RecSize + 5;
                if (recs[at] == c[i].R && recs[at + 1] == c[i].G && recs[at + 2] == c[i].B) continue;
                if (!WriteReg(5, bankBase + at, new[] { c[i].R, c[i].G, c[i].B })) { Say(L.T("Luces: el SMC-PAD no confirmó el cambio de color.")); return; }
            }
        }

        void Say(string s) { var r = Report; if (r != null) r(s); }

        // Para el hilo; si restore, devuelve los colores originales (al cerrar la app o apagar la opción); luego libera el puerto.
        public void Shutdown(bool restore)
        {
            stop = true;
            lock (pendLock) pending = null;
            signal.Set();
            worker.Join(1500);
            if (restore)
                lock (ioLock)
                {
                    foreach (var kv in originals)
                    {
                        byte[] recs = ReadReg(5, kv.Key, BankBytes);
                        if (recs == null) break;   // el pad no responde: no bloquear el cierre
                        for (int r = 0; r < 16; r++)
                        {
                            int at = r * RecSize + 5;
                            if (recs[at] == kv.Value[r * 3] && recs[at + 1] == kv.Value[r * 3 + 1] && recs[at + 2] == kv.Value[r * 3 + 2]) continue;
                            if (!WriteReg(5, kv.Key + at, new[] { kv.Value[r * 3], kv.Value[r * 3 + 1], kv.Value[r * 3 + 2] })) break;
                        }
                    }
                }
            o.Dispose();
        }

        public void Dispose() { Shutdown(false); }
    }

    // ---------- controles ----------
    class PadButton : Control
    {
        public int Number;
        public string Title = "", Sub = "", Note = "";
        public Color Accent = Theme.Music;
        public bool Selected, Hit, Learning, Fixed, On;

        public PadButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            float k = g.DpiX / 96f;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Bg);
            var r = new RectangleF(2, 2, Width - 5, Height - 5);
            double t = Hit ? 0.8 : On ? 0.45 : Fixed ? 0.22 : 0.13;
            using (var path = Theme.Round(r, 12 * k))
            {
                using (var b = new SolidBrush(Theme.Blend(Theme.Pad, Accent, t))) g.FillPath(b, path);
                var bc = Learning ? Theme.Warn : Selected ? Accent : Theme.Blend(Theme.Line, Accent, .25);
                using (var p = new Pen(bc, (Selected || Learning ? 2.5f : 1f) * k))
                {
                    if (Learning) p.DashStyle = DashStyle.Dash;
                    g.DrawPath(p, path);
                }
            }
            int pad = (int)(12 * k), subH = (int)(20 * k), top = (int)(18 * k);
            TextRenderer.DrawText(g, Number.ToString(), Theme.Small, new Point(pad, pad - 4), Theme.Muted);
            if (Note.Length > 0)
                TextRenderer.DrawText(g, Note, Theme.Small, new Rectangle(0, pad - 4, Width - pad, subH), Theme.Muted, TextFormatFlags.Right);
            var titleRect = new Rectangle(pad, pad + top, Width - 2 * pad, Height - 2 * pad - subH - top);
            TextRenderer.DrawText(g, Title.Length > 0 ? Title : L.T("(vacío)"), Theme.Bold, titleRect, Title.Length > 0 ? (Hit ? Color.White : Theme.Text) : Theme.Muted,
                TextFormatFlags.Bottom | TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Sub, Theme.Small, new Rectangle(pad, Height - pad - subH, Width - 2 * pad, subH), Hit ? Color.White : Theme.Muted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }
    }

    // Aviso flotante (ícono + modo + preset) que no roba el foco, para que Photoshop, el navegador, etc. sigan activos.
    class Osd : Form
    {
        string mode = "", name = "", icon = "star", count = "";
        Color accent = Theme.Music;
        readonly Timer hold = new Timer { Interval = 1800 }, fade = new Timer { Interval = 30 };
        readonly float s = 1f;
        const double Visible0 = 0.97;
        static readonly Font NameFont = new Font("Segoe UI Semibold", 17f);
        static readonly Font ModeFont = new Font("Segoe UI Semibold", 9f);

        public Osd()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Theme.Panel;
            DoubleBuffered = true;
            Opacity = Visible0;
            using (var g = CreateGraphics()) s = g.DpiX / 96f;
            Size = new Size((int)(340 * s), (int)(100 * s));
            SetShape();
            hold.Tick += (o, e) => { hold.Stop(); fade.Start(); };
            fade.Tick += (o, e) => { if (Opacity > 0.12) Opacity -= 0.12; else { fade.Stop(); Hide(); Opacity = Visible0; } };
        }
        void SetShape()
        {
            using (var p = Theme.Round(new RectangleF(0, 0, Width, Height), 18 * s)) Region = new Region(p);
        }
        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x80 | 0x8 | 0x20; // NOACTIVATE | TOOLWINDOW | TOPMOST | TRANSPARENT
                return cp;
            }
        }
        void Prepare(string m, string n, string ic, Color c, string cnt)
        {
            mode = m; name = n; icon = ic; accent = c; count = cnt;
            int textW;
            using (var g = CreateGraphics()) textW = TextRenderer.MeasureText(g, name, NameFont).Width;
            int w = Math.Max((int)(300 * s), Math.Min((int)(600 * s), (int)(124 * s) + textW));
            if (w != Width) { Width = w; SetShape(); }
        }
        // Para pruebas: dibuja el aviso en una imagen sin mostrarlo
        public Bitmap Render(string m, string n, string ic, Color c, string cnt)
        {
            Prepare(m, n, ic, c, cnt);
            var bmp = new Bitmap(Width, Height);
            DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
            return bmp;
        }
        public void ShowInfo(string m, string n, string ic, Color c, string cnt)
        {
            fade.Stop(); Opacity = Visible0;
            Prepare(m, n, ic, c, cnt);
            var wa = Screen.FromPoint(Cursor.Position).WorkingArea;   // en el monitor donde está el mouse
            Location = new Point(wa.Left + (wa.Width - Width) / 2, wa.Bottom - Height - (int)(48 * s));
            if (!Visible) Show();
            Invalidate();
            hold.Stop(); hold.Start();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Panel);
            using (var path = Theme.Round(new RectangleF(1, 1, Width - 3, Height - 3), 17 * s))
            using (var p = new Pen(Theme.Blend(Theme.Line, accent, .55), 1.5f * s))
                g.DrawPath(p, path);
            float t = 66 * s;
            Icons.Draw(g, new RectangleF(17 * s, (Height - t) / 2, t, t), icon);
            int x = (int)(100 * s);
            TextRenderer.DrawText(g, mode + "  ·  " + count, ModeFont, new Point(x, (int)(19 * s)), accent);
            TextRenderer.DrawText(g, name, NameFont, new Rectangle(x, (int)(38 * s), Width - x - (int)(14 * s), (int)(42 * s)), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }

    // ---------- ventana principal ----------
    public class MainForm : Form
    {
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vk);

        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", RunName = "PadBox", OldRunName = "SMC-Pad Control";
        static readonly string[] TypeIds = { "keys", "text", "run", "sound", "note", "chord", "seq", "loop", "cc", "toggle", "pc", "none" };
        static readonly string[] TypeNames = { L.T("Atajo de teclado"), L.T("Escribir texto"), L.T("Abrir programa o web"), L.T("Sonido (mp3, wav…)"), L.T("Nota MIDI"), L.T("Acorde MIDI"), L.T("Melodía paso a paso"), L.T("Ritmo en bucle (on/off)"), L.T("CC momentáneo"), "CC on/off", "Program Change", L.T("Nada") };
        static readonly string[] SoundModes = { L.T("Una vez (suena completo)"), L.T("Mientras mantienes el pad"), L.T("Bucle on/off") };
        static readonly string[] TextTypes = { "text", "run", "seq" };
        static readonly string[] MidiTypes = { "note", "chord", "loop", "cc", "toggle", "pc" };

        readonly string cfgPath = Path.Combine(AppPaths.Data, "padbox-config.json");
        readonly Config cfg;
        MidiIn midiIn;
        MidiOut midiOut;
        IPadLights lights;
        readonly PadButton[] pads = new PadButton[16];
        readonly bool[] toggles = new bool[16];
        readonly Dictionary<int, List<int>> heldKeys = new Dictionary<int, List<int>>();
        readonly Dictionary<int, int[]> heldMidi = new Dictionary<int, int[]>();   // mensajes para soltar: [st, d1, d2, st, d1, d2...]
        readonly Dictionary<int, Looper> loops = new Dictionary<int, Looper>();
        readonly SamplePlayer samples = new SamplePlayer();
        readonly Mixer mixer = new Mixer();
        readonly Dictionary<string, Clip> clips = new Dictionary<string, Clip>(StringComparer.OrdinalIgnoreCase);
        Clip GetClip(string path)
        {
            Clip c;
            if (!clips.TryGetValue(path, out c)) { c = Clip.Load(path); clips[path] = c; }
            return c;
        }
        readonly HashSet<int> heldSounds = new HashSet<int>();   // sonidos en modo "mientras mantienes"
        TextBox txtSound;
        ComboBox cboSoundMode;
        NumericUpDown numSoundVol;
        readonly Dictionary<int, int> seqPos = new Dictionary<int, int>();
        readonly Dictionary<int, DateTime> seqLast = new Dictionary<int, DateTime>();
        readonly int[] lastProgram = new int[3];   // canales 1, 2 y 10
        int sel = PadLayout.FixedPads, detectIdx = -1;
        bool recording, loading, startHidden, exiting, tipShown;
        readonly Osd osd = new Osd();
        NotifyIcon tray;
        readonly Timer saveTimer = new Timer { Interval = 400 }, portTimer = new Timer { Interval = 3000 };

        Button btnMusic, btnOffice, btnRec, btnBrowse, btnTest, btnDetect, btnIcon;
        readonly float uiScale = GetScale();
        static float GetScale() { using (var g = Graphics.FromHwnd(IntPtr.Zero)) return g.DpiX / 96f; }
        ComboBox cboPreset, cboType, cboChord, cboPattern, cboIn, cboOut;
        TextBox txtName, txtKeys, txtValue;
        NumericUpDown numNum, numCh, numVel;
        Label lblPad, lblNote, lblStatus, lblFixed, lblNumCap, lblChordCap, lblVelCap, lblConn, lblHelpKeys, lblHelpText, lblTextCap;
        Panel grpAction, grpKeys, grpText, grpMidi, grpSound;
        CheckBox chkOsd, chkStartup, chkScoop;

        List<Preset> Presets { get { return cfg.OfficeMode ? cfg.Office : cfg.Music; } }
        int Idx
        {
            get { return Math.Max(0, Math.Min(Presets.Count - 1, cfg.OfficeMode ? cfg.OfficeIdx : cfg.MusicIdx)); }
            set { if (cfg.OfficeMode) cfg.OfficeIdx = value; else cfg.MusicIdx = value; }
        }
        Preset Cur { get { return Presets[Idx]; } }
        Color Accent { get { return cfg.OfficeMode ? Theme.Office : Theme.Music; } }
        PadAction SelAction { get { return sel >= PadLayout.FixedPads ? Cur.Pads[sel - PadLayout.FixedPads] : null; } }
        static readonly string[] SwatchColors = { "", Defaults.Red, Defaults.Orange, Defaults.Green, Defaults.Cyan, Defaults.Blue, Defaults.Purple, Defaults.Pink, Defaults.White };
        readonly List<Button> swatches = new List<Button>();
        void RefreshSwatches()
        {
            var a = SelAction;
            for (int k = 0; k < swatches.Count; k++)
            {
                var sw = swatches[k];
                if (SwatchColors[k].Length == 0) sw.BackColor = Icons.Get(Cur.Icon).Color;
                bool on = a != null && (a.Color ?? "") == SwatchColors[k];
                sw.FlatAppearance.BorderSize = on ? 3 : 1;
                sw.FlatAppearance.BorderColor = on ? Color.White : Theme.Line;
            }
            if (a != null) lblPad.ForeColor = PadColor(a);
        }
        int PrevIdx { get { return (Idx - 1 + Presets.Count) % Presets.Count; } }
        int NextIdx { get { return (Idx + 1) % Presets.Count; } }
        // Color del pad en el aparato y en pantalla: el suyo propio, o el del ícono del preset
        Color PadColor(PadAction a)
        {
            Color c;
            if (TryColor(a.Color, out c)) return c;
            return Icons.Get(Cur.Icon).Color;
        }
        static bool TryColor(string hex, out Color c)
        {
            c = Color.Empty;
            if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#') return false;
            int v;
            if (!int.TryParse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out v)) return false;
            c = Color.FromArgb(255, Color.FromArgb(v));
            return true;
        }

        public MainForm(bool hidden)
        {
            startHidden = hidden;
            cfg = LoadConfig();
            if (!Program.TestMode) { SaveConfig(); MigrateStartup(); }   // deja guardada la configuración actualizada
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "PadBox " + Program.Version;
            BackColor = Theme.Bg; ForeColor = Theme.Text; Font = Theme.Base;
            ClientSize = new Size(1100, 760);
            MinimumSize = new Size(820, 740);   // cabe en pantallas verticales de 1296 px al 150 %
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            Icon = MakeIcon();
            BuildUi();
            ResumeLayout(false);
            PerformLayout();
            AdjustBars();
            BuildTray();
            var unused = Handle; // asegura el handle para BeginInvoke desde el hilo MIDI
            saveTimer.Tick += (s, e) => { saveTimer.Stop(); SaveConfig(); };
            portTimer.Tick += (s, e) => ReconnectIfNeeded();
            FillPorts();
            RefreshAll();
            PreloadSounds();
            portTimer.Start();
        }

        // ---------- construcción de la interfaz ----------
        void BuildUi()
        {
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(14), BackColor = Theme.Bg };
            padGrid = grid;
            for (int i = 0; i < 4; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }
            for (int i = 0; i < 16; i++)
            {
                int idx = i;
                var p = new PadButton { Number = i + 1, Dock = DockStyle.Fill, Margin = new Padding(5) };
                p.Click += (s, e) => { sel = idx; RefreshPads(); LoadEditor(); };
                p.AllowDrop = true;   // arrastrar archivos de audio sobre el pad
                p.DragEnter += (s, e) => e.Effect = DroppedAudio(e).Count > 0 ? DragDropEffects.Copy : DragDropEffects.None;
                p.DragDrop += (s, e) => AssignSounds(idx, DroppedAudio(e));
                pads[i] = p;
                grid.Controls.Add(p, i % 4, i / 4);
            }

            // barra superior
            // Barra superior: si la ventana es angosta, los botones pasan a una segunda línea
            top = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Theme.Panel };
            topFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(16, 14, 16, 0), BackColor = Theme.Panel };
            var flow = topFlow;
            top.Controls.Add(flow);
            var title = new Label { Text = "PadBox", Font = Theme.Title, AutoSize = true, Margin = new Padding(0, 1, 18, 0) };
            btnMusic = MkButton(L.T("MÚSICA"), 104); btnMusic.Font = Theme.Bold;
            btnOffice = MkButton(L.T("OFICINA"), 104); btnOffice.Font = Theme.Bold;
            btnMusic.Click += (s, e) => SetMode(false);
            btnOffice.Click += (s, e) => SetMode(true);
            cboPreset = MkCombo(220);
            cboPreset.SelectedIndexChanged += (s, e) => { if (!loading && cboPreset.SelectedIndex >= 0) SelectPreset(cboPreset.SelectedIndex); };
            btnIcon = MkButton("", 40);
            btnIcon.ImageAlign = ContentAlignment.MiddleCenter;
            new ToolTip().SetToolTip(btnIcon, L.T("Cambiar el ícono del preset"));
            int ip = (int)(26 * uiScale);
            var iconMenu = new ContextMenuStrip { ImageScalingSize = new Size(ip, ip), LayoutStyle = ToolStripLayoutStyle.Table };
            ((TableLayoutSettings)iconMenu.LayoutSettings).ColumnCount = 3;
            foreach (var d in Icons.All)
            {
                var key = d.Key;
                iconMenu.Items.Add(d.Name, Icons.Bitmap(key, ip), (s, e) => { Cur.Icon = key; RefreshAll(); ShowOsd(); ScheduleSave(); });
            }
            btnIcon.Click += (s, e) => iconMenu.Show(btnIcon, new Point(0, btnIcon.Height));
            var bNew = MkButton(L.T("Nuevo"), 64); bNew.Click += (s, e) => NewPreset(false);
            var bDup = MkButton(L.T("Duplicar"), 80); bDup.Click += (s, e) => NewPreset(true);
            var bRen = MkButton(L.T("Renombrar"), 92); bRen.Click += (s, e) => RenamePreset();
            var bDel = MkButton(L.T("Borrar"), 64); bDel.Click += (s, e) => DeletePreset();
            flow.Controls.AddRange(new Control[] { title, btnMusic, btnOffice, Inline("Preset:", 18), btnIcon, cboPreset, bNew, bDup, bRen, bDel });

            // editor
            // Con barra vertical si la ventana es baja; el ancho extra deja lugar a la barra para que no aparezca la horizontal
            var editor = new Panel { Dock = DockStyle.Right, Width = 350 + 18, BackColor = Theme.Panel, AutoScroll = true };
            lblPad = new Label { Left = 18, Top = 16, AutoSize = true, Font = Theme.Title };
            editor.Controls.Add(lblPad);
            Cap(editor, L.T("Nombre"), 18, 56);
            txtName = MkText(editor, 18, 76, 312);
            txtName.TextChanged += (s, e) => Edit(a => a.Name = txtName.Text);
            lblFixed = new Label { Left = 18, Top = 118, Width = 312, Height = 260, ForeColor = Theme.Muted };
            editor.Controls.Add(lblFixed);

            grpAction = new Panel { Left = 0, Top = 112, Width = 350, Height = 428 };
            editor.Controls.Add(grpAction);
            Cap(grpAction, L.T("Acción"), 18, 0);
            cboType = MkCombo(312); cboType.Left = 18; cboType.Top = 20; cboType.Margin = Padding.Empty;
            cboType.Items.AddRange(TypeNames);
            cboType.SelectedIndexChanged += (s, e) => OnTypeChanged();
            grpAction.Controls.Add(cboType);

            grpKeys = new Panel { Left = 0, Top = 60, Width = 350, Height = 200 };
            Cap(grpKeys, L.T("Atajo"), 18, 0);
            txtKeys = MkText(grpKeys, 18, 20, 204);
            txtKeys.TextChanged += (s, e) => { Edit(a => a.Value = txtKeys.Text.Trim()); Validate(); };
            btnRec = MkButton(L.T("Grabar"), 100); btnRec.Left = 230; btnRec.Top = 18;
            btnRec.Click += (s, e) => { if (recording) StopRec(); else StartRec(); };
            grpKeys.Controls.Add(btnRec);
            lblHelpKeys = new Label { Left = 18, Top = 58, Width = 312, Height = 130, ForeColor = Theme.Muted, Font = Theme.Small,
                Text = L.T("Pulsa «Grabar» y luego la combinación en tu teclado, o escríbela.\n\nEjemplos: Ctrl+Shift+N · Win+D · Alt+Tab · F5\nTeclas especiales: Space, Enter, Esc, Delete, Left, Right, Add (Num +), VolumeUp, VolumeMute, MediaPlayPause\n\nLa tecla queda presionada mientras sostienes el pad.") };
            grpKeys.Controls.Add(lblHelpKeys);

            grpText = new Panel { Left = 0, Top = 60, Width = 350, Height = 200 };
            lblTextCap = Cap(grpText, L.T("Texto, programa o página web"), 18, 0);
            txtValue = MkText(grpText, 18, 20, 312);
            txtValue.TextChanged += (s, e) => { Edit(a => a.Value = txtValue.Text); Validate(); };
            btnBrowse = MkButton(L.T("Examinar…"), 110); btnBrowse.Left = 18; btnBrowse.Top = 54;
            btnBrowse.Click += (s, e) => Browse();
            grpText.Controls.Add(btnBrowse);
            lblHelpText = new Label { Left = 18, Top = 96, Width = 312, Height = 90, ForeColor = Theme.Muted, Font = Theme.Small };
            grpText.Controls.Add(lblHelpText);
            chkScoop = new CheckBox { Left = 18, Top = 54, AutoSize = true, Text = L.T("«Gemido» al empezar (pitch bend)") };
            chkScoop.CheckedChanged += (s, e) => Edit(a => a.Scoop = chkScoop.Checked);
            grpText.Controls.Add(chkScoop);

            grpMidi = new Panel { Left = 0, Top = 60, Width = 350, Height = 200 };
            lblNumCap = Cap(grpMidi, L.T("Nota"), 18, 0);
            numNum = MkNum(grpMidi, 18, 20, 0, 127);
            numNum.ValueChanged += (s, e) => OnNumChanged();
            lblNote = new Label { Left = 104, Top = 23, AutoSize = true, Font = Theme.Bold };
            grpMidi.Controls.Add(lblNote);
            lblChordCap = Cap(grpMidi, L.T("Acorde"), 170, 0);
            cboChord = MkCombo(160); cboChord.Left = 170; cboChord.Top = 20; cboChord.Margin = Padding.Empty;
            cboChord.Items.AddRange(Mus.ChordIds.Select(Mus.ChordName).ToArray());
            cboChord.SelectedIndexChanged += (s, e) => OnChordChanged();
            grpMidi.Controls.Add(cboChord);
            cboPattern = MkCombo(160); cboPattern.Left = 170; cboPattern.Top = 20; cboPattern.Margin = Padding.Empty;
            cboPattern.Items.AddRange(Looper.Display);
            cboPattern.SelectedIndexChanged += (s, e) => { if (cboPattern.SelectedIndex >= 0) Edit(a => a.Value = Looper.Keys[cboPattern.SelectedIndex]); };
            grpMidi.Controls.Add(cboPattern);
            Cap(grpMidi, L.T("Canal"), 18, 60);
            numCh = MkNum(grpMidi, 18, 80, 1, 16);
            numCh.ValueChanged += (s, e) => Edit(a => a.Ch = (int)numCh.Value);
            lblVelCap = Cap(grpMidi, L.T("Velocidad (0 = según golpe)"), 120, 60);
            numVel = MkNum(grpMidi, 120, 80, 0, 127);
            numVel.ValueChanged += (s, e) => Edit(a => a.Vel = (int)numVel.Value);

            grpSound = new Panel { Left = 0, Top = 60, Width = 350, Height = 200 };
            Cap(grpSound, L.T("Archivo de sonido"), 18, 0);
            txtSound = MkText(grpSound, 18, 20, 312);
            txtSound.ReadOnly = true;
            var bPick = MkButton(L.T("Elegir sonido…"), 140); bPick.Left = 18; bPick.Top = 52;
            bPick.Click += (s, e) => PickSound();
            grpSound.Controls.Add(bPick);
            Cap(grpSound, L.T("Modo"), 18, 94);
            cboSoundMode = MkCombo(200); cboSoundMode.Left = 18; cboSoundMode.Top = 114; cboSoundMode.Margin = Padding.Empty;
            cboSoundMode.Items.AddRange(SoundModes);
            cboSoundMode.SelectedIndexChanged += (s, e) => { if (cboSoundMode.SelectedIndex >= 0) Edit(a => a.Num = cboSoundMode.SelectedIndex); };
            grpSound.Controls.Add(cboSoundMode);
            Cap(grpSound, L.T("Volumen (0 = golpe)"), 228, 94);
            numSoundVol = MkNum(grpSound, 228, 114, 0, 127);
            numSoundVol.Width = 70;
            numSoundVol.ValueChanged += (s, e) => Edit(a => a.Vel = (int)numSoundVol.Value);
            grpSound.Controls.Add(new Label { Left = 18, Top = 152, Width = 312, Height = 44, ForeColor = Theme.Muted, Font = Theme.Small,
                Text = L.T("También puedes arrastrar archivos de audio desde el Explorador y soltarlos sobre un pad.") });
            grpAction.Controls.AddRange(new Control[] { grpKeys, grpText, grpMidi, grpSound });
            Cap(grpAction, L.T("Color en el pad"), 18, 264);
            for (int k = 0; k < SwatchColors.Length; k++)
            {
                var hex = SwatchColors[k];
                var sw = new Button { Left = 18 + k * 34, Top = 284, Width = 28, Height = 28, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false, Cursor = Cursors.Hand,
                    Font = Theme.Small, ForeColor = Color.Black, Text = hex.Length == 0 ? "P" : "" };
                Color col;
                if (TryColor(hex, out col)) sw.BackColor = col;
                new ToolTip().SetToolTip(sw, hex.Length == 0 ? L.T("Color del preset") : L.T("Usar este color"));
                sw.Click += (s, e) => { Edit(a => a.Color = hex); RefreshSwatches(); UpdatePadColors(); };
                swatches.Add(sw);
                grpAction.Controls.Add(sw);
            }
            btnTest = MkButton(L.T("▶  Probar"), 130); btnTest.Left = 18; btnTest.Top = 326;
            btnTest.Click += (s, e) => TestPad();
            grpAction.Controls.Add(btnTest);
            var hint = new Label { Left = 18, Top = 368, Width = 312, Height = 64, ForeColor = Theme.Muted, Font = Theme.Small,
                Text = L.T("Los cambios se guardan solos.\nPad 1: modo · Pad 2: preset anterior · Pad 3: preset siguiente.") };
            grpAction.Controls.Add(hint);

            // barra inferior
            bottom = new Panel { Dock = DockStyle.Bottom, Height = 124, BackColor = Theme.Panel };
            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(18, 4, 0, 0), ForeColor = Theme.Muted, BackColor = Theme.Bg };
            bottomFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(16, 10, 16, 0), BackColor = Theme.Panel };
            var bflow = bottomFlow;
            cboIn = MkCombo(200);
            cboIn.SelectedIndexChanged += (s, e) => { if (loading) return; cfg.InPort = (string)cboIn.SelectedItem; OpenIn(); ScheduleSave(); };
            cboOut = MkCombo(230);
            cboOut.SelectedIndexChanged += (s, e) => { if (loading) return; cfg.OutPort = (string)cboOut.SelectedItem; OpenOut(true); ScheduleSave(); };
            var bRefresh = MkButton(L.T("Actualizar"), 92); bRefresh.Click += (s, e) => FillPorts();
            btnDetect = MkButton(L.T("Detectar pads"), 116); btnDetect.Click += (s, e) => ToggleDetect();
            lblConn = new Label { AutoSize = true, Margin = new Padding(6, 8, 10, 0), Font = Theme.Bold };
            chkOsd = new CheckBox { Text = L.T("Aviso en pantalla al cambiar"), AutoSize = true, Margin = new Padding(0, 7, 14, 0), Checked = !cfg.HideOsd };
            chkOsd.CheckedChanged += (s, e) => { cfg.HideOsd = !chkOsd.Checked; ScheduleSave(); };
            chkStartup = new CheckBox { Text = L.T("Iniciar con Windows"), AutoSize = true, Margin = new Padding(0, 7, 14, 0), Checked = StartupEnabled() };
            chkStartup.CheckedChanged += (s, e) => { if (!loading) SetStartup(chkStartup.Checked); };
            var bReset = MkButton(L.T("Restaurar presets"), 136); bReset.Click += (s, e) => ResetPresets();
            var cboLang = MkCombo(120);
            cboLang.Items.AddRange(new object[] { L.T("Automático"), "Español", "English" });
            cboLang.SelectedIndex = cfg.Lang == "es" ? 1 : cfg.Lang == "en" ? 2 : 0;
            cboLang.SelectedIndexChanged += (s, e) =>
            {
                if (loading) return;
                cfg.Lang = cboLang.SelectedIndex == 1 ? "es" : cboLang.SelectedIndex == 2 ? "en" : null;
                ScheduleSave();
                Status(L.T("El idioma cambiará la próxima vez que abras PadBox. Con «Restaurar presets» los presets quedan en ese idioma."));
            };
            var chkLights = new CheckBox { Text = L.T("Colores en el pad"), AutoSize = true, Margin = new Padding(0, 7, 14, 0), Checked = !cfg.NoPadLights };
            chkLights.CheckedChanged += (s, e) => { cfg.NoPadLights = !chkLights.Checked; ScheduleSave(); if (cfg.NoPadLights) StopLights(true); else StartLights(); };
            cboDevice = MkCombo(240);
            cboDevice.Items.Add(L.T("Automático"));
            foreach (var d in Devices.All) cboDevice.Items.Add(d.Name);
            int di = Devices.All.FindIndex(d => d.Id == cfg.Device);
            cboDevice.SelectedIndex = di >= 0 ? di + 1 : 0;
            cboDevice.SelectedIndexChanged += (s, e) =>
            {
                if (loading) return;
                cfg.Device = cboDevice.SelectedIndex <= 0 ? "auto" : Devices.All[cboDevice.SelectedIndex - 1].Id;
                OpenIn();   // vuelve a abrir con el perfil nuevo (notas, número de pads y luces)
                RefreshAll();
            };
            bflow.Controls.AddRange(new Control[] { lblConn, Inline(L.T("Controlador:"), 0), cboDevice, Inline(L.T("Entrada:"), 8), cboIn, Inline(L.T("Salida música:"), 8), cboOut, bRefresh, btnDetect, chkOsd, chkStartup, chkLights, bReset, Inline(L.T("Idioma:"), 8), cboLang });
            bottom.Controls.Add(bflow);
            bottom.Controls.Add(lblStatus);

            Controls.Add(grid);
            Controls.Add(editor);
            Controls.Add(top);
            Controls.Add(bottom);
        }

        Button MkButton(string text, int w)
        {
            var b = new Button { Text = text, Width = w, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Theme.Field, ForeColor = Theme.Text, Margin = new Padding(0, 0, 6, 0), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            b.FlatAppearance.BorderColor = Theme.Line;
            b.FlatAppearance.MouseOverBackColor = Theme.Blend(Theme.Field, Theme.Music, .3);
            return b;
        }
        ComboBox MkCombo(int w)
        {
            return new ComboBox { Width = w, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Theme.Field, ForeColor = Theme.Text, Margin = new Padding(0, 4, 6, 0) };
        }
        TextBox MkText(Control parent, int x, int y, int w)
        {
            var t = new TextBox { Left = x, Top = y, Width = w, BackColor = Theme.Field, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(t);
            return t;
        }
        NumericUpDown MkNum(Control parent, int x, int y, int min, int max)
        {
            var n = new NumericUpDown { Left = x, Top = y, Width = 78, Minimum = min, Maximum = max, BackColor = Theme.Field, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(n);
            return n;
        }
        Label Cap(Control parent, string text, int x, int y)
        {
            var l = new Label { Text = text, Left = x, Top = y, AutoSize = true, ForeColor = Theme.Muted, Font = Theme.Small };
            parent.Controls.Add(l);
            return l;
        }
        Label Inline(string text, int left)
        {
            return new Label { Text = text, AutoSize = true, ForeColor = Theme.Muted, Margin = new Padding(left, 8, 6, 0) };
        }

        static Icon MakeIcon()
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                Color[] cs = { Theme.Fixed, Theme.Fixed, Theme.Music, Theme.Office };
                for (int i = 0; i < 4; i++)
                    using (var path = Theme.Round(new RectangleF((i % 2) * 16 + 1, (i / 2) * 16 + 1, 14, 14), 3))
                    using (var b = new SolidBrush(cs[i]))
                        g.FillPath(b, path);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        void BuildTray()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(L.T("Abrir"), null, (s, e) => ShowMain());
            menu.Items.Add(L.T("Modo música"), null, (s, e) => SetMode(false));
            menu.Items.Add(L.T("Modo oficina"), null, (s, e) => SetMode(true));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(L.T("Salir"), null, (s, e) => Quit());
            tray = new NotifyIcon { Icon = Icon, Text = "PadBox", Visible = true, ContextMenuStrip = menu };
            tray.DoubleClick += (s, e) => ShowMain();
        }
        void ShowMain() { Show(); WindowState = FormWindowState.Normal; Activate(); }
        public void Quit() { exiting = true; Close(); }

        // ---------- pantallas ----------
        // Pintado con doble búfer de toda la ventana: evita restos de dibujo al moverla entre pantallas
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; }   // WS_EX_COMPOSITED
        }
        Screen lastScreen;
        Panel top, bottom;
        FlowLayoutPanel topFlow, bottomFlow;
        // Las barras de arriba y abajo crecen cuando sus botones no caben en una línea (ventana angosta)
        void AdjustBars()
        {
            if (top == null || bottom == null) return;
            int th = topFlow.GetPreferredSize(new Size(top.ClientSize.Width, 0)).Height + topFlow.Padding.Vertical / 2;
            int bh = bottomFlow.GetPreferredSize(new Size(bottom.ClientSize.Width - bottomFlow.Padding.Horizontal, 0)).Height + bottomFlow.Padding.Vertical + lblStatus.Height + 6;
            if (Math.Abs(top.Height - th) > 1) top.Height = th;
            if (Math.Abs(bottom.Height - bh) > 1) bottom.Height = bh;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustBars();
        }
        protected override void OnShown(EventArgs e)
        {
            AdjustBars();   // ya con la escala de pantalla aplicada
            base.OnShown(e);
        }
        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            var s = Screen.FromControl(this);
            if (lastScreen != null && !s.DeviceName.Equals(lastScreen.DeviceName)) Invalidate(true);   // cambió de pantalla
            lastScreen = s;
        }
        // Al soltar la ventana: si no cabe en la pantalla donde quedó, se ajusta a ella (así no queda partida entre dos)
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            FitToScreen();
            Invalidate(true);
        }
        void FitToScreen()
        {
            if (WindowState != FormWindowState.Normal) return;
            var wa = Screen.FromControl(this).WorkingArea;
            var b = Bounds;
            int w = Math.Min(b.Width, wa.Width), h = Math.Min(b.Height, wa.Height);
            int x = Math.Max(wa.Left, Math.Min(b.Left, wa.Right - w)), y = Math.Max(wa.Top, Math.Min(b.Top, wa.Bottom - h));
            if (x != b.Left || y != b.Top || w != b.Width || h != b.Height) Bounds = new Rectangle(x, y, w, h);
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x7E) Invalidate(true);   // WM_DISPLAYCHANGE: cambió la configuración de pantallas
        }

        protected override void SetVisibleCore(bool value)
        {
            if (startHidden) { startHidden = false; value = false; if (!IsHandleCreated) CreateHandle(); }
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                if (!tipShown) { tray.ShowBalloonTip(3000, L.T("PadBox sigue activo"), L.T("Los pads siguen funcionando. Ábrelo desde el icono junto al reloj."), ToolTipIcon.Info); tipShown = true; }
                return;
            }
            ReleaseAll();
            StopLoops();
            SaveConfig();
            StopLights(true);
            samples.Dispose();
            mixer.Dispose();
            if (midiIn != null) midiIn.Dispose();
            if (midiOut != null) midiOut.Dispose();
            tray.Visible = false;
            base.OnFormClosing(e);
        }

        // ---------- configuración ----------
        Config LoadConfig()
        {
            Config c = null;
            // La app se llamaba "SMC-Pad Control": si existe su configuración y aún no la de PadBox, se usa esa
            var oldCfg = Path.Combine(AppPaths.App, "smc-pad-config.json");
            try { if (!File.Exists(cfgPath) && File.Exists(oldCfg)) File.Copy(oldCfg, cfgPath); } catch (IOException) { }
            try { if (File.Exists(cfgPath)) c = new JavaScriptSerializer().Deserialize<Config>(File.ReadAllText(cfgPath, Encoding.UTF8)); }
            catch { try { File.Copy(cfgPath, cfgPath + ".dañado", true); } catch { } }
            // Configuración nueva: los presets por defecto ya están al día (y en el idioma de la app), sin migraciones
            if (c == null) c = new Config { Version = 14 };
            if (c.InNotes == null || c.InNotes.Count != 16) c.InNotes = Enumerable.Range(36, 16).ToList();
            if (c.Music == null || c.Music.Count == 0) c.Music = Defaults.Music();
            if (c.Office == null || c.Office.Count == 0) c.Office = Defaults.Office();
            // Nombres genéricos para los presets que antes citaban a terceros (ver migraciones de abajo, que ya usan estos nombres).
            // Un preset que el usuario tenga guardado con una canción sigue siendo suyo: no se toca ni se agrega a nadie más.
            if (c.Version < 14)
                foreach (var p in c.Music)
                {
                    if (p.Name != null && p.Name.EndsWith(" (dubstep)")) p.Name = "Dubstep";
                    else if (p.Name == "Drum Rack (Ableton/FL)") p.Name = "Rack de batería";
                }
            if (c.Version < 3)
            {
                // Escritorio ahora trae controles multimedia; se agrega el preset de OBS
                int esc = c.Office.FindIndex(p => p.Name == "Escritorio");
                if (esc >= 0) c.Office[esc] = Defaults.Escritorio(); else c.Office.Insert(0, Defaults.Escritorio());
                if (!c.Office.Any(p => p.Name == "OBS"))
                {
                    int nav = c.Office.FindIndex(p => p.Name == "Navegador");
                    c.Office.Insert(nav >= 0 ? nav + 1 : c.Office.Count, Defaults.Obs());
                }
                c.Version = 3;
            }
            if (c.Version < 5)
            {
                if (!c.Music.Any(p => p.Name == "Piano cumbia villera"))
                {
                    int t = c.Music.FindIndex(p => p.Name == "Cumbia");
                    c.Music.Insert(t >= 0 ? t + 1 : c.Music.Count, Defaults.PianoVillera());
                }
                c.Version = 5;
            }
            if (c.Version < 7)
            {
                // Pads 2 y 3 ahora son preset anterior/siguiente (13 acciones) y los presets de oficina traen colores por función:
                // los presets que vienen con la app se reemplazan por su versión nueva (se conserva el ícono elegido).
                var fresh = Defaults.Music().Concat(Defaults.Office()).ToDictionary(p => p.Name);
                foreach (var list in new[] { c.Music, c.Office })
                    for (int k = 0; k < list.Count; k++)
                    {
                        Preset f;
                        if (list[k].Name != null && fresh.TryGetValue(list[k].Name, out f)) { f.Icon = list[k].Icon; list[k] = f; }
                    }
                c.Version = 7;
            }
            if (c.Version < 8)
            {
                // Presets nuevos de música: Acordeón y Teclado cumbia rebajada, después de Piano cumbia villera
                int at = c.Music.FindIndex(p => p.Name == "Piano cumbia villera");
                at = at >= 0 ? at + 1 : c.Music.Count;
                foreach (var np in new[] { Defaults.Acordeon(), Defaults.Rebajada() })
                    if (!c.Music.Any(p => p.Name == np.Name)) c.Music.Insert(at++, np);
                c.Version = 8;
            }
            if (c.Version < 9)
            {
                if (!c.Music.Any(p => p.Name == "Saxofón"))
                {
                    int at = c.Music.FindIndex(p => p.Name == "Teclado cumbia rebajada");
                    c.Music.Insert(at >= 0 ? at + 1 : c.Music.Count, Defaults.Saxofon());
                }
                c.Version = 9;
            }
            if (c.Version < 10)
            {
                // Pad 4 de cada preset de música = un ritmo que combine; nuevo preset de dubstep.
                // Los presets de música que vienen con la app se reemplazan por su versión nueva (se conserva el ícono).
                var fresh = Defaults.Music().ToDictionary(p => p.Name);
                for (int k = 0; k < c.Music.Count; k++)
                {
                    Preset f;
                    if (c.Music[k].Name != null && fresh.TryGetValue(c.Music[k].Name, out f)) { f.Icon = c.Music[k].Icon; c.Music[k] = f; }
                }
                if (!c.Music.Any(p => p.Name == "Dubstep"))
                {
                    int at = c.Music.FindIndex(p => p.Name == "Saxofón");
                    c.Music.Insert(at >= 0 ? at + 1 : c.Music.Count, Defaults.Dubstep());
                }
                c.Version = 10;
            }
            if (c.Version < 11)
            {
                // Cada preset de música es único (ritmo, instrumento, tonalidad y batería propios). Los presets que traía
                // la app se cambian por la lista nueva; los que creó el usuario se conservan al final.
                var builtin = new HashSet<string> { "Batería GM", "Cumbia", "Piano cumbia villera", "Acordeón",
                    "Teclado cumbia rebajada", "Saxofón", "Dubstep", "Rack de batería", "Cromática C3", "Escala Do mayor",
                    "Pentatónica La menor", "Acordes en Do", "Botones CC (on/off)" };
                var icons = c.Music.Where(p => p.Name != null).GroupBy(p => p.Name).ToDictionary(g => g.Key, g => g.First().Icon);
                var mine = c.Music.Where(p => p.Name == null || !builtin.Contains(p.Name)).ToList();
                c.Music = Defaults.Music();
                foreach (var p in c.Music) { string ic; if (icons.TryGetValue(p.Name, out ic) && !string.IsNullOrEmpty(ic)) p.Icon = ic; }
                c.Music.AddRange(mine);
                c.MusicIdx = 0;
                c.Version = 11;
            }
            if (c.Version < 12)
            {
                // Preset nuevo "Mis sonidos" (pads con archivos de audio), antes de Drum Rack
                if (!c.Music.Any(p => p.Name == "Mis sonidos"))
                {
                    int at = c.Music.FindIndex(p => p.Name == "Rack de batería");
                    var ms = Defaults.MisSonidos(); ms.DrumKit = 1;
                    c.Music.Insert(at >= 0 ? at : c.Music.Count, ms);
                }
                c.Version = 12;
            }
            if (c.Version < 13)
            {
                // Preset nuevo "Drum Pad EDM" (bucles originales sincronizados), después de "Mis sonidos"
                if (!c.Music.Any(p => p.Name == "Drum Pad EDM"))
                {
                    int at = c.Music.FindIndex(p => p.Name == "Mis sonidos");
                    c.Music.Insert(at >= 0 ? at + 1 : c.Music.Count, Defaults.DrumPadEdm());
                }
                c.Version = 13;
            }
            c.Version = 14;
            foreach (var p in c.Music.Concat(c.Office))
            {
                if (p.Name == null) p.Name = "Preset";
                if (string.IsNullOrEmpty(p.Icon) || !Icons.All.Any(d => d.Key == p.Icon)) p.Icon = Icons.Default(p.Name);
                if (p.Pads == null) p.Pads = new List<PadAction>();
                while (p.Pads.Count < PadLayout.Actions) p.Pads.Add(new PadAction());
                if (p.Pads.Count > PadLayout.Actions) p.Pads.RemoveRange(PadLayout.Actions, p.Pads.Count - PadLayout.Actions);
                foreach (var a in p.Pads)
                {
                    if (a.Name == null) a.Name = "";
                    if (a.Value == null) a.Value = "";
                    if (Array.IndexOf(TypeIds, a.Type) < 0) a.Type = "none";
                    if (a.Ch < 1 || a.Ch > 16) a.Ch = 1;
                }
            }
            return c;
        }
        void ScheduleSave() { saveTimer.Stop(); saveTimer.Start(); }
        void SaveConfig()
        {
            try { File.WriteAllText(cfgPath, new JavaScriptSerializer().Serialize(cfg), Encoding.UTF8); }
            catch (Exception ex) { Status(L.T("No se pudo guardar la configuración: ") + ex.Message); }
        }

        static bool StartupEnabled()
        {
            if (AppPaths.Packaged)
            {
                try { return PackagedStartup.Enabled(); }
                catch (Exception) { return false; }
            }
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && (k.GetValue(RunName) != null || k.GetValue(OldRunName) != null);
        }
        // Si estaba activo el inicio automático con el nombre anterior, se pasa a PadBox (apuntando a este .exe)
        void MigrateStartup()
        {
            if (AppPaths.Packaged) return;
            bool old;
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) old = k != null && k.GetValue(OldRunName) != null;
            if (old) SetStartup(true);
        }
        void SetStartup(bool on)
        {
            if (AppPaths.Packaged)
            {
                try
                {
                    if (!on) { PackagedStartup.Disable(); return; }
                    if (PackagedStartup.Enable()) return;
                    Status(L.T("Windows no dejó activar el inicio automático. Actívalo en Configuración → Aplicaciones → Inicio."));
                }
                catch (Exception ex) { Status(L.T("No se pudo cambiar el inicio automático: ") + ex.Message); }
                // no quedó activado: la casilla vuelve a mostrar el estado real
                loading = true; chkStartup.Checked = false; loading = false;
                return;
            }
            try
            {
                using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    k.DeleteValue(OldRunName, false);   // entrada del nombre anterior
                    if (on) k.SetValue(RunName, "\"" + Application.ExecutablePath + "\" --min");
                    else k.DeleteValue(RunName, false);
                }
            }
            catch (Exception ex) { Status(L.T("No se pudo cambiar el inicio automático: ") + ex.Message); }
        }

        // ---------- puertos MIDI ----------
        static bool IsKnown(string n) { return Devices.ForPort(n) != null; }

        void FillPorts()
        {
            loading = true;
            var ins = Winmm.Inputs();
            var outs = Winmm.Outputs();
            if (cfg.InPort == null || !ins.Contains(cfg.InPort))
            {
                // Primero un modelo conocido (su puerto principal, no los "MIDIIN2…"), si no cualquier controlador físico
                var p = ins.FirstOrDefault(n => IsKnown(n) && !n.StartsWith("MIDIIN")) ?? ins.FirstOrDefault(IsKnown)
                    ?? ins.FirstOrDefault(n => !Devices.IsSoftwarePort(n) && !n.StartsWith("MIDIIN")) ?? ins.FirstOrDefault(n => !Devices.IsSoftwarePort(n));
                if (p != null) cfg.InPort = p;
            }
            if (cfg.OutPort == null)
                cfg.OutPort = outs.FirstOrDefault(n => n.IndexOf("loop", StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? outs.FirstOrDefault(n => n.IndexOf("GS Wavetable", StringComparison.OrdinalIgnoreCase) >= 0) ?? "";
            cboIn.Items.Clear(); cboIn.Items.AddRange(ins.ToArray());
            cboOut.Items.Clear(); cboOut.Items.Add(L.T("(ninguna)")); cboOut.Items.AddRange(outs.ToArray());
            cboIn.SelectedItem = ins.Contains(cfg.InPort) ? cfg.InPort : null;
            cboOut.SelectedItem = outs.Contains(cfg.OutPort) ? cfg.OutPort : L.T("(ninguna)");
            loading = false;
            OpenIn();
            OpenOut(false);
        }

        void ReconnectIfNeeded()
        {
            if (midiIn != null || cboIn.DroppedDown || cboOut.DroppedDown) return;
            var ins = Winmm.Inputs();
            if (ins.Contains(cfg.InPort ?? "") || ins.Any(IsKnown)) FillPorts();
        }

        // ---------- perfil del controlador ----------
        DeviceProfile profile = Devices.Generic;
        string profileId;
        TableLayoutPanel padGrid;
        ComboBox cboDevice;
        int PadCount { get { return profile.Pads; } }

        // Elige el perfil (automático según el puerto, o el que el usuario fijó) y carga las notas de sus pads
        void ApplyProfile()
        {
            var p = string.IsNullOrEmpty(cfg.Device) || cfg.Device == "auto"
                ? (Devices.ForPort(cfg.InPort) ?? Devices.Generic) : (Devices.Get(cfg.Device) ?? Devices.Generic);
            if (cfg.PadNotes == null) cfg.PadNotes = new Dictionary<string, List<int>>();
            if (profileId != null && profileId != p.Id) cfg.PadNotes[profileId] = cfg.InNotes.ToList();
            if (profileId != p.Id)
            {
                List<int> saved;
                bool factoryDefault = cfg.InNotes.SequenceEqual(Enumerable.Range(36, 16));
                bool legacy = cfg.PadNotes.Count == 0;   // configuración de antes de los perfiles: sus notas ya eran las del aparato
                if (cfg.PadNotes.TryGetValue(p.Id, out saved) && saved.Count == 16) cfg.InNotes = saved.ToList();
                else if (profileId != null || factoryDefault || !legacy)
                {
                    var n = p.Notes.ToList();
                    while (n.Count < 16) n.Add(-1);   // pads que el aparato no tiene
                    cfg.InNotes = n;
                }
                // (al abrir por primera vez con este perfil se conservan las notas que ya estaban detectadas)
                cfg.PadNotes[p.Id] = cfg.InNotes.ToList();
            }
            profileId = p.Id;
            profile = p;
            ApplyPadCount();
            ScheduleSave();
        }

        // Muestra solo los pads que tiene el aparato (8 = dos filas, 16 = cuatro filas)
        void ApplyPadCount()
        {
            if (padGrid == null) return;
            int rows = PadCount / 4;
            padGrid.SuspendLayout();
            for (int r = 0; r < 4; r++) padGrid.RowStyles[r] = r < rows ? new RowStyle(SizeType.Percent, 100f / rows) : new RowStyle(SizeType.Absolute, 0);
            for (int i = 0; i < 16; i++) pads[i].Visible = i < PadCount;
            padGrid.ResumeLayout();
            if (sel >= PadCount) sel = PadLayout.FixedPads;
            RefreshPads();
        }

        // Launchpad: hay que pasarlo a "modo programador" para que sus pads manden las notas del perfil
        void PrepareDevice()
        {
            if (profile.Lights != "launchpad" || midiIn == null) return;
            int id = Winmm.Outputs().IndexOf(midiIn.Name);
            if (id < 0) return;
            using (var o = new MidiOut())
                if (o.Open(id, midiIn.Name)) o.SendSysEx(new byte[] { 0xF0, 0x00, 0x20, 0x29, 0x02, profile.LaunchpadId, 0x0E, 0x01, 0xF7 });
        }

        void OpenIn()
        {
            StopLights(false);
            if (midiIn != null) { midiIn.Dispose(); midiIn = null; }
            ApplyProfile();
            int id = Winmm.Inputs().IndexOf(cfg.InPort ?? "");
            if (id < 0) { Status(L.T("Conecta tu controlador MIDI por USB…")); UpdateConn(); return; }
            var m = new MidiIn();
            m.Message += (st, d1, d2) => BeginInvoke(new Action(() => OnMidi(st, d1, d2)));
            m.Closed += () => BeginInvoke(new Action(() => { if (midiIn == m) { StopLights(false); midiIn.Dispose(); midiIn = null; Status(L.T("Controlador desconectado. Esperando a que vuelva…")); UpdateConn(); } }));
            m.SysEx += data => { var l = lights; if (l != null) l.OnSysEx(data); };
            if (m.Open(id, cfg.InPort))
            {
                midiIn = m;
                Status(L.T("Conectado a ") + cfg.InPort + " (" + profile.Name + L.T("). Toca un pad."));
                PrepareDevice();
                StartLights();
            }
            else { m.Dispose(); Status(L.T("No se pudo abrir «") + cfg.InPort + L.T("». ¿Lo está usando otro programa (DAW o navegador)? Ciérralo y pulsa Actualizar.")); }
            UpdateConn();
        }

        void OpenOut(bool force)
        {
            string name = cfg.OutPort ?? "";
            if (!force && midiOut != null && midiOut.Name == name) return;
            StopLoops();
            if (midiOut != null) { midiOut.Dispose(); midiOut = null; }
            int id = Winmm.Outputs().IndexOf(name);
            if (id < 0) return;
            var o = new MidiOut();
            if (o.Open(id, name)) { midiOut = o; Array.Clear(lastProgram, 0, 3); lastProgram[2] = 1; ApplyProgram(); }
            else Status(L.T("No se pudo abrir la salida «") + name + L.T("»."));
        }

        // Cambia el instrumento de los canales 1 y 2 si el preset lo pide (p. ej. acordeón en Cumbia) y vuelve al piano después.
        void ApplyProgram()
        {
            if (midiOut == null || cfg.OfficeMode) return;
            SendProgram(0, 0, Cur.Program);
            SendProgram(1, 1, Cur.Program2);
            SendProgram(2, 9, Math.Max(1, Cur.DrumKit));   // canal 10: juego de batería (estándar si el preset no pide otro)
        }
        void SendProgram(int slot, int ch, int prog)
        {
            prog = Math.Max(0, Math.Min(128, prog));
            if (prog == lastProgram[slot]) return;
            midiOut.Send(0xC0 | ch, Math.Max(0, prog - 1), 0);
            lastProgram[slot] = prog;
        }

        void StopLoops()
        {
            foreach (var l in loops.Values) l.Stop();
            loops.Clear();
        }

        void UpdateConn()
        {
            lblConn.Text = midiIn != null ? L.T("● Pad conectado") : L.T("● Sin pad");
            lblConn.ForeColor = midiIn != null ? Theme.Ok : Theme.Err;
        }

        void Status(string s) { lblStatus.Text = s; }

        // ---------- entrada del pad ----------
        void OnMidi(int st, int d1, int d2)
        {
            if (st >= 0xF8) return; // reloj / active sensing
            int t = st & 0xF0;
            bool on = t == 0x90 && d2 > 0, off = t == 0x80 || (t == 0x90 && d2 == 0);
            if (on && detectIdx >= 0) { Learn(d1); return; }
            int i = cfg.InNotes.IndexOf(d1);
            if (i >= PadCount) i = -1;   // pads que este aparato no tiene
            if (i >= 0 && (on || off || t == 0xA0))
            {
                if (on) { PadDown(i, d2); Status("Pad " + (i + 1) + L.T(" · nota ") + Mus.NoteName(d1) + " (" + d1 + L.T(") · fuerza ") + d2); }
                else if (off) PadUp(i);
                return;
            }
            if (on) Status(L.T("El pad mandó la nota ") + Mus.NoteName(d1) + " (" + d1 + L.T("), que no está asignada. Usa «Detectar pads»."));
            if (!cfg.OfficeMode && midiOut != null) midiOut.Send(st, d1, d2);   // otros mensajes pasan tal cual
        }

        void PadDown(int i, int vel)
        {
            pads[i].Hit = true; pads[i].Invalidate();
            if (i == 0) { SetMode(!cfg.OfficeMode); return; }
            if (i == 1) { SelectPreset(PrevIdx); return; }
            if (i == 2) { SelectPreset(NextIdx); return; }
            Exec(i, Cur.Pads[i - PadLayout.FixedPads], vel);
        }
        void PadUp(int i)
        {
            pads[i].Hit = false; pads[i].Invalidate();
            Release(i);
        }

        void Exec(int i, PadAction a, int vel)
        {
            Release(i);
            int ch = Math.Max(1, Math.Min(16, a.Ch)) - 1;
            int v = a.Vel > 0 ? a.Vel : Math.Max(1, vel);
            int val = a.Vel > 0 ? a.Vel : 127;
            switch (a.Type)
            {
                case "keys":
                    var vks = Shortcut.Parse(a.Value);
                    if (vks == null) { Status(L.T("El atajo «") + a.Value + L.T("» no es válido.")); return; }
                    Kbd.Down(vks);
                    heldKeys[i] = vks;
                    break;
                case "text":
                    if (a.Value.Length > 0) Kbd.Type(a.Value);
                    break;
                case "run":
                    if (a.Value.Trim().Length == 0) return;
                    try { Process.Start(new ProcessStartInfo(a.Value.Trim()) { UseShellExecute = true }); }
                    catch (Exception ex) { Status(L.T("No se pudo abrir «") + a.Value + L.T("»: ") + ex.Message); }
                    break;
                case "note":
                case "chord":
                    var notes = a.Type == "chord" ? Mus.Intervals(a.Chord).Select(x => a.Num + x).Where(n => n <= 127) : new[] { a.Num };
                    var rel = new List<int>();
                    foreach (var n in notes) { Out(0x90 | ch, n, v); rel.AddRange(new[] { 0x80 | ch, n, 0 }); }
                    heldMidi[i] = rel.ToArray();
                    break;
                case "seq":
                    var starts = new HashSet<int>();
                    var seq = Mus.ParseSeq(a.Value, starts);
                    if (seq == null || seq.Count == 0) { Status(L.T("La melodía de «") + a.Name + L.T("» tiene notas no válidas.")); return; }
                    int pos; DateTime last;
                    // Vuelve al inicio si llegó al final o si pasó un rato sin tocarlo (Num = segundos; 0 = 2,5 s)
                    double idle = a.Num > 0 ? a.Num : 2.5;
                    if (!seqPos.TryGetValue(i, out pos) || !seqLast.TryGetValue(i, out last) || (DateTime.Now - last).TotalSeconds > idle || pos >= seq.Count) pos = 0;
                    if (a.Scoop && starts.Contains(pos)) Scoop(ch);
                    Out(0x90 | ch, seq[pos], v);
                    heldMidi[i] = new[] { 0x80 | ch, seq[pos], 0 };
                    seqPos[i] = pos + 1; seqLast[i] = DateTime.Now;
                    pads[i].Sub = L.T("Nota ") + (pos + 1) + L.T(" de ") + seq.Count + " · " + Mus.NoteName(seq[pos]);
                    break;
                case "cc":
                    Out(0xB0 | ch, a.Num, val);
                    heldMidi[i] = new[] { 0xB0 | ch, a.Num, 0 };
                    break;
                case "toggle":
                    toggles[i] = !toggles[i];
                    Out(0xB0 | ch, a.Num, toggles[i] ? val : 0);
                    pads[i].On = toggles[i];
                    break;
                case "loop":
                    Looper l;
                    if (loops.TryGetValue(i, out l)) { l.Stop(); loops.Remove(i); toggles[i] = false; }
                    else
                    {
                        l = new Looper();
                        if (midiOut == null) { Status(L.T("Elige una «Salida música» abajo para oír el ritmo.")); break; }
                        if (!l.Start(midiOut, a.Value, ch, a.Num)) { Status(L.T("Ritmo desconocido: ") + a.Value); break; }
                        loops[i] = l; toggles[i] = true;
                    }
                    pads[i].On = toggles[i];
                    break;
                case "pc":
                    Out(0xC0 | ch, a.Num, 0);
                    break;
                case "sound":
                    PlaySound(i, a, vel);
                    break;
            }
        }

        // ---------- sonidos (mp3, wav…) ----------
        // Modo (a.Num): 0 = suena completo, 1 = mientras se mantiene el pad, 2 = bucle on/off. Volumen: a.Vel (0 = según golpe).
        void PlaySound(int i, PadAction a, int vel)
        {
            var path = Sounds.Resolve(a.Value);
            if (path.Length == 0) { Status("Pad " + (i + 1) + L.T(": no tiene sonido. Arrastra un archivo de audio sobre él o elígelo en el editor.")); return; }
            if (!File.Exists(path)) { Status(L.T("No encuentro el sonido «") + a.Value + L.T("».")); return; }
            int volume = a.Vel > 0 ? a.Vel * 1000 / 127 : Math.Max(150, vel * 1000 / 127);
            // WAV de 16 bits a 44,1 kHz: mezclador propio (bucles sincronizados, poca demora)
            var clip = mixer.Ok ? GetClip(path) : null;
            if (clip != null)
            {
                float v = volume / 1000f;
                int grid = Cur.SyncBpm > 0 ? (int)Math.Round(Mixer.Rate * 60.0 / Cur.SyncBpm * 4 * Math.Max(1, Cur.SyncBars)) : 0;
                if (a.Num == 2)
                {
                    toggles[i] = !toggles[i];
                    if (toggles[i]) mixer.Play(i, clip, v, true, grid); else mixer.Stop(i);
                    pads[i].On = toggles[i];
                    return;
                }
                mixer.Play(i, clip, v, false, 0);
                if (a.Num == 1) heldSounds.Add(i);
                return;
            }
            if (!samples.Load(i, path)) { Status(L.T("No se pudo abrir «") + Path.GetFileName(path) + L.T("»: ") + samples.LastError); return; }
            if (a.Num == 2)
            {
                toggles[i] = !toggles[i];
                if (toggles[i]) samples.Play(i, volume, true); else samples.Stop(i);
                pads[i].On = toggles[i];
                return;
            }
            samples.Play(i, volume, false);
            if (a.Num == 1) heldSounds.Add(i);
        }
        // Deja abiertos (precargados) los sonidos del preset actual, para que suenen sin demora
        void PreloadSounds()
        {
            samples.StopAll();
            mixer.StopAll();
            heldSounds.Clear();
            var keep = new List<int>();
            for (int i = PadLayout.FixedPads; i < 16; i++)
            {
                var a = Cur.Pads[i - PadLayout.FixedPads];
                if (a.Type != "sound") continue;
                var path = Sounds.Resolve(a.Value);
                if (path.Length == 0 || !File.Exists(path)) continue;
                if (mixer.Ok && GetClip(path) != null) continue;   // los WAV ya quedan en memoria para el mezclador
                if (samples.Load(i, path)) keep.Add(i);
            }
            samples.Keep(keep);
        }
        static List<string> DroppedAudio(DragEventArgs e)
        {
            var files = e.Data.GetDataPresent(DataFormats.FileDrop) ? e.Data.GetData(DataFormats.FileDrop) as string[] : null;
            return files == null ? new List<string>() : files.Where(Sounds.IsAudio).ToList();
        }
        // Asigna uno o varios archivos a partir del pad indicado (los siguientes pads reciben el resto)
        void AssignSounds(int pad, List<string> files)
        {
            if (files.Count == 0) return;
            if (pad < PadLayout.FixedPads) { Status(L.T("Los pads 1, 2 y 3 son fijos (modo y presets). Suelta el sonido en los pads 4 a 16.")); return; }
            int assigned = 0;
            foreach (var f in files)
            {
                int i = pad + assigned;
                if (i >= 16) break;
                try
                {
                    var a = Cur.Pads[i - PadLayout.FixedPads];
                    a.Type = "sound"; a.Value = Sounds.Import(f); a.Name = Sounds.NameFor(f);
                    a.Num = 0; a.Vel = 0;
                    assigned++;
                }
                catch (Exception ex) { Status(L.T("No se pudo copiar «") + Path.GetFileName(f) + L.T("»: ") + ex.Message); break; }
            }
            if (assigned == 0) return;
            sel = pad;
            PreloadSounds();
            RefreshPads(); LoadEditor(); UpdatePadColors(); ScheduleSave();
            Status(assigned == 1 ? L.T("Sonido asignado al pad ") + (pad + 1) + "." : assigned + L.T(" sonidos asignados desde el pad ") + (pad + 1) + ".");
        }
        void PickSound()
        {
            using (var d = new OpenFileDialog { Filter = Sounds.Filter, Title = L.T("Elige un sonido para el pad ") + (sel + 1), Multiselect = true })
                if (d.ShowDialog(this) == DialogResult.OK) AssignSounds(sel, d.FileNames.ToList());
        }

        // "Gemido" de teclado de cumbia: la nota arranca un semitono abajo y sube rápido a su lugar.
        void Scoop(int ch)
        {
            var o = midiOut;
            if (o == null) return;
            const int depth = 4096; // 1 semitono con el rango estándar de ±2
            int low = 8192 - depth;
            o.Send(0xE0 | ch, low & 0x7F, low >> 7);
            new System.Threading.Thread(() => {
                for (int k = 1; k <= 8; k++)
                {
                    System.Threading.Thread.Sleep(14);
                    int v = 8192 - depth * (8 - k) / 8;
                    o.Send(0xE0 | ch, v & 0x7F, v >> 7);
                }
            }) { IsBackground = true }.Start();
        }

        void Out(int st, int d1, int d2)
        {
            if (midiOut == null) { Status(L.T("Elige una «Salida música» abajo para oír los pads.")); return; }
            midiOut.Send(st, d1, d2);
        }

        void Release(int i)
        {
            List<int> k;
            if (heldKeys.TryGetValue(i, out k)) { Kbd.Up(k); heldKeys.Remove(i); }
            int[] m;
            if (heldMidi.TryGetValue(i, out m))
            {
                for (int j = 0; j + 2 < m.Length; j += 3) Out(m[j], m[j + 1], m[j + 2]);
                heldMidi.Remove(i);
            }
            if (heldSounds.Remove(i)) { samples.Stop(i); mixer.Stop(i); }
        }
        void ReleaseAll()
        {
            foreach (var i in heldKeys.Keys.Concat(heldMidi.Keys).Concat(heldSounds).Distinct().ToList()) Release(i);
        }

        // ---------- modos y presets ----------
        void SetMode(bool office)
        {
            ReleaseAll();
            StopLoops();
            seqPos.Clear();
            cfg.OfficeMode = office;
            Array.Clear(toggles, 0, 16);
            ApplyProgram();
            RefreshAll();
            PreloadSounds();
            ShowOsd();
            ScheduleSave();
        }
        void SelectPreset(int i)
        {
            ReleaseAll();
            StopLoops();
            seqPos.Clear();
            Idx = i;
            Array.Clear(toggles, 0, 16);
            ApplyProgram();
            RefreshAll();
            PreloadSounds();
            ShowOsd();
            ScheduleSave();
        }
        void ShowOsd()
        {
            string mode = cfg.OfficeMode ? L.T("OFICINA") : L.T("MÚSICA");
            string tip = "PadBox · " + mode + " · " + Cur.Name;
            tray.Text = tip.Substring(0, Math.Min(63, tip.Length));
            if (cfg.HideOsd) return;
            osd.ShowInfo(mode, Cur.Name, Cur.Icon, Accent, (Idx + 1) + L.T(" de ") + Presets.Count);
        }

        string Prompt(string title, string def)
        {
            using (var f = new Form { Text = title, FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.Dpi, AutoScaleDimensions = new SizeF(96f, 96f), ClientSize = new Size(340, 96), Font = Theme.Base })
            {
                var t = new TextBox { Left = 12, Top = 14, Width = 316, Text = def };
                var ok = new Button { Text = L.T("Aceptar"), Left = 172, Top = 54, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = L.T("Cancelar"), Left = 253, Top = 54, Width = 75, DialogResult = DialogResult.Cancel };
                f.Controls.AddRange(new Control[] { t, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;
                return f.ShowDialog(this) == DialogResult.OK && t.Text.Trim().Length > 0 ? t.Text.Trim() : null;
            }
        }
        void NewPreset(bool copy)
        {
            var name = Prompt(copy ? L.T("Duplicar preset") : L.T("Nuevo preset"), copy ? Cur.Name + L.T(" (copia)") : L.T("Mi preset"));
            if (name == null) return;
            var p = new Preset { Name = name, Icon = copy ? Cur.Icon : "star", Program = copy ? Cur.Program : 0, Program2 = copy ? Cur.Program2 : 0,
                Pads = copy ? Cur.Pads.Select(a => a.Clone()).ToList() : Enumerable.Range(0, PadLayout.Actions).Select(x => new PadAction()).ToList() };
            Presets.Add(p);
            SelectPreset(Presets.Count - 1);
        }
        void RenamePreset()
        {
            var name = Prompt(L.T("Renombrar preset"), Cur.Name);
            if (name == null) return;
            Cur.Name = name;
            RefreshAll(); ScheduleSave();
        }
        void DeletePreset()
        {
            if (Presets.Count <= 1) { MessageBox.Show(this, L.T("Cada modo necesita al menos un preset."), "PadBox"); return; }
            if (MessageBox.Show(this, L.T("¿Borrar el preset «") + Cur.Name + L.T("»?"), "PadBox", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int i = Idx;
            Presets.RemoveAt(i);
            SelectPreset(Math.Max(0, i - 1));
        }
        void ResetPresets()
        {
            if (MessageBox.Show(this, L.T("¿Restaurar los presets originales de los dos modos? Se pierden los cambios que hiciste en los presets (la detección de pads se conserva)."), "PadBox", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            ReleaseAll();
            cfg.Music = Defaults.Music(); cfg.Office = Defaults.Office();
            cfg.MusicIdx = 0; cfg.OfficeIdx = 0;
            RefreshAll(); ScheduleSave();
        }

        // ---------- colores en el pad ----------
        // Solo para perfiles con luces controlables. Los pedidos salen por la salida con el mismo nombre que la entrada
        // (en el SMC-PAD Pocket las respuestas llegan por esa entrada).
        void StartLights()
        {
            StopLights(false);
            if (cfg.NoPadLights || midiIn == null || Program.TestMode || profile.Lights.Length == 0) return;
            int id = Winmm.Outputs().IndexOf(midiIn.Name);
            if (id < 0) return;
            var o = new MidiOut();
            if (!o.Open(id, midiIn.Name)) return;
            if (profile.Lights == "launchpad")
            {
                lights = new LaunchpadLights(o, profile.LaunchpadId);
                UpdatePadColors();
                return;
            }
            var known = new Dictionary<int, byte[]>();
            if (cfg.PadOriginals != null)
                foreach (var kv in cfg.PadOriginals)
                {
                    int key;
                    try { if (int.TryParse(kv.Key, out key) && kv.Value.Length == 96) known[key] = Enumerable.Range(0, 48).Select(k => Convert.ToByte(kv.Value.Substring(k * 2, 2), 16)).ToArray(); }
                    catch (FormatException) { }
                }
            var pl = new PadLights(o, known);
            lights = pl;
            pl.Report += s => { try { BeginInvoke(new Action(() => Status(s))); } catch { } };
            pl.OriginalCaptured += (key, rgb) => { try { BeginInvoke(new Action(() => {
                if (cfg.PadOriginals == null) cfg.PadOriginals = new Dictionary<string, string>();
                cfg.PadOriginals[key.ToString()] = string.Concat(rgb.Select(b => b.ToString("X2")));
                SaveConfig();
            })); } catch { } };
            UpdatePadColors();
        }
        void StopLights(bool restore)
        {
            var l = lights;
            if (l == null) return;
            l.Shutdown(restore);   // "lights" sigue asignado mientras restaura, para que le lleguen las respuestas del pad
            lights = null;
        }
        void UpdatePadColors()
        {
            if (lights == null) return;
            var c = new Color[16];
            c[0] = Accent;                        // pad 1: color del modo
            c[1] = c[2] = Theme.Fixed;            // pads 2 y 3: amarillo (preset anterior / siguiente)
            for (int i = PadLayout.FixedPads; i < 16; i++) c[i] = PadColor(Cur.Pads[i - PadLayout.FixedPads]);
            lights.Apply(c, cfg.InNotes.ToArray());
        }

        // ---------- detectar pads ----------
        void ToggleDetect()
        {
            if (detectIdx >= 0) { detectIdx = -1; btnDetect.Text = L.T("Detectar pads"); Status(L.T("Detección cancelada.")); RefreshPads(); return; }
            if (midiIn == null) { MessageBox.Show(this, L.T("Primero conecta tu controlador (mira la barra de abajo)."), "PadBox"); return; }
            ReleaseAll();
            detectIdx = 0;
            btnDetect.Text = L.T("Cancelar");
            Status(L.T("DETECTAR: toca el pad físico que quieres como Pad 1 (arriba a la izquierda en pantalla)."));
            RefreshPads();
        }
        void Learn(int note)
        {
            if (cfg.InNotes.Take(detectIdx).Contains(note)) return;
            cfg.InNotes[detectIdx] = note;
            detectIdx++;
            if (detectIdx >= PadCount)
            {
                detectIdx = -1;
                btnDetect.Text = L.T("Detectar pads");
                Status(L.T("¡Listo! Los ") + PadCount + L.T(" pads quedaron detectados."));
                if (cfg.PadNotes == null) cfg.PadNotes = new Dictionary<string, List<int>>();
                cfg.PadNotes[profile.Id] = cfg.InNotes.ToList();
                ScheduleSave();
                UpdatePadColors();
            }
            else Status(L.T("DETECTAR: ahora toca el pad físico para el Pad ") + (detectIdx + 1) + ".");
            RefreshPads();
        }

        // ---------- refresco de la interfaz ----------
        void RefreshAll()
        {
            loading = true;
            StyleMode(btnMusic, !cfg.OfficeMode, Theme.Music);
            StyleMode(btnOffice, cfg.OfficeMode, Theme.Office);
            cboPreset.Items.Clear();
            foreach (var p in Presets) cboPreset.Items.Add(p.Name);
            cboPreset.SelectedIndex = Idx;
            var old = btnIcon.Image;
            btnIcon.Image = Icons.Bitmap(Cur.Icon, (int)(26 * uiScale));
            if (old != null) old.Dispose();
            loading = false;
            RefreshPads();
            LoadEditor();
            UpdatePadColors();
        }
        static void StyleMode(Button b, bool on, Color c)
        {
            b.BackColor = on ? c : Theme.Field;
            b.ForeColor = on ? Color.White : Theme.Muted;
            b.FlatAppearance.BorderColor = on ? c : Theme.Line;
            b.FlatAppearance.MouseOverBackColor = on ? c : Theme.Blend(Theme.Field, c, .3);
        }
        void RefreshPads()
        {
            for (int i = 0; i < 16; i++)
            {
                var p = pads[i];
                p.Selected = i == sel;
                p.Learning = i == detectIdx;
                p.Note = Mus.NoteName(cfg.InNotes[i]);
                if (i < PadLayout.FixedPads)
                {
                    p.Fixed = true; p.Accent = i == 0 ? Accent : Theme.Fixed; p.On = false;
                    p.Title = i == 0 ? L.T("Cambiar modo") : i == 1 ? L.T("Preset anterior") : L.T("Preset siguiente");
                    p.Sub = i == 0 ? (cfg.OfficeMode ? L.T("→ Música") : L.T("→ Oficina")) : i == 1 ? "← " + Presets[PrevIdx].Name : "→ " + Presets[NextIdx].Name;
                }
                else
                {
                    var a = Cur.Pads[i - PadLayout.FixedPads];
                    p.Fixed = false; p.Accent = PadColor(a);
                    p.Title = a.Name; p.Sub = Describe(a);
                    p.On = a.Type == "toggle" && toggles[i];
                }
                p.Invalidate();
            }
        }
        static string Describe(PadAction a)
        {
            switch (a.Type)
            {
                case "keys": return Shortcut.Pretty(a.Value);
                case "text": return L.T("Escribe: ") + a.Value.Replace("\n", " ");
                case "run":
                    var v = a.Value.Trim();
                    if (v.StartsWith("http", StringComparison.OrdinalIgnoreCase)) v = v.Replace("https://", "").Replace("http://", "").Replace("www.", "");
                    else { try { v = Path.GetFileName(v); } catch { } }
                    return L.T("Abre: ") + v;
                case "note": return Mus.NoteName(a.Num) + L.T(" · canal ") + a.Ch;
                case "chord": return L.T("Acorde ") + Mus.ChordName(a.Chord) + L.T(" · canal ") + a.Ch;
                case "loop": return Looper.DisplayName(a.Value) + " · " + a.Num + " BPM · on/off";
                case "seq":
                    var seq = Mus.ParseSeq(a.Value);
                    return seq == null ? L.T("Notas no válidas") : L.T("Melodía · ") + seq.Count + L.T(" notas · toca para avanzar");
                case "cc": return "CC " + a.Num + L.T(" · canal ") + a.Ch;
                case "toggle": return "CC " + a.Num + L.T(" on/off · canal ") + a.Ch;
                case "pc": return L.T("Programa ") + a.Num + L.T(" · canal ") + a.Ch;
                case "sound":
                    if (string.IsNullOrWhiteSpace(a.Value)) return L.T("Sin sonido: arrastra un archivo aquí");
                    return (a.Num == 1 ? L.T("Mantener · ") : a.Num == 2 ? L.T("Bucle · ") : "") + Path.GetFileName(a.Value);
            }
            return "—";
        }

        // ---------- editor ----------
        void LoadEditor()
        {
            loading = true;
            StopRec();
            lblPad.Text = "Pad " + (sel + 1);
            bool fixedPad = sel < PadLayout.FixedPads;
            lblPad.ForeColor = sel == 0 ? Accent : fixedPad ? Theme.Fixed : PadColor(SelAction);
            grpAction.Visible = !fixedPad;
            lblFixed.Visible = fixedPad;
            txtName.Enabled = !fixedPad;
            if (fixedPad)
            {
                txtName.Text = sel == 0 ? L.T("Cambiar modo") : sel == 1 ? L.T("Preset anterior") : L.T("Preset siguiente");
                lblFixed.Text = sel == 0
                    ? L.T("Este pad alterna entre el modo MÚSICA y el modo OFICINA.\n\nCada modo guarda sus propios presets y recuerda cuál usaste por última vez.")
                    : L.T("Este pad pasa al ") + (sel == 1 ? L.T("preset anterior") : L.T("preset siguiente")) + L.T(" del modo actual:\n\n") + string.Join("\n", Presets.Select((p, k) => (k == Idx ? "▶ " : "    ") + p.Name));
                loading = false;
                return;
            }
            var a = SelAction;
            txtName.Text = a.Name;
            cboType.SelectedIndex = Math.Max(0, Array.IndexOf(TypeIds, a.Type));
            txtKeys.Text = a.Type == "keys" ? a.Value : "";
            txtValue.Text = TextTypes.Contains(a.Type) ? a.Value : "";
            chkScoop.Checked = a.Scoop;
            txtSound.Text = a.Type == "sound" ? a.Value : "";
            cboSoundMode.SelectedIndex = a.Type == "sound" ? Math.Max(0, Math.Min(2, a.Num)) : 0;
            numSoundVol.Value = Math.Max(0, Math.Min(127, a.Vel));
            RefreshSwatches();
            numNum.Maximum = a.Type == "loop" ? 220 : 127;   // en los ritmos el número es el tempo (BPM)
            numNum.Value = Math.Max(0, Math.Min((int)numNum.Maximum, a.Num));
            numCh.Value = Math.Max(1, Math.Min(16, a.Ch));
            numVel.Value = Math.Max(0, Math.Min(127, a.Vel));
            cboChord.SelectedIndex = Math.Max(0, Array.IndexOf(Mus.ChordIds, a.Chord));
            cboPattern.SelectedIndex = Math.Max(0, Array.IndexOf(Looper.Keys, (a.Value ?? "").ToLowerInvariant()));
            loading = false;
            UpdateTypeUi();
        }

        void UpdateTypeUi()
        {
            var t = TypeIds[Math.Max(0, cboType.SelectedIndex)];
            bool isNote = t == "note" || t == "chord";
            grpKeys.Visible = t == "keys";
            grpText.Visible = TextTypes.Contains(t);
            grpMidi.Visible = MidiTypes.Contains(t);
            grpSound.Visible = t == "sound";
            btnBrowse.Visible = t == "run";
            lblTextCap.Text = t == "seq" ? L.T("Notas en orden") : L.T("Texto, programa o página web");
            chkScoop.Visible = t == "seq";
            lblHelpText.Top = t == "run" ? 96 : t == "seq" ? 100 : 58;
            lblHelpText.Height = t == "seq" ? 100 : 140;
            lblHelpText.Text = t == "run" ? L.T("Puede ser un programa (.exe), un acceso directo, una carpeta o una página web (https://…).")
                : t == "seq" ? L.T("Cada toque suena la siguiente nota; si paras ") + (SelAction != null && SelAction.Num > 0 ? SelAction.Num.ToString() : L.T("2,5")) + L.T(" s vuelve a empezar.\nNotas: Do Re Mi Fa Sol La Si (o C D E…), con # o b y octava. Ej: Sol#3 La3 Do4\nUsa | para separar frases. Canal ") + (SelAction != null ? SelAction.Ch : 2) + "."
                : L.T("El texto se escribe en la ventana activa, como si lo teclearas.");
            lblNumCap.Text = t == "note" ? L.T("Nota") : t == "chord" ? L.T("Nota raíz") : t == "pc" ? L.T("Programa") : t == "loop" ? L.T("Tempo (BPM)") : L.T("Número de CC");
            lblNote.Visible = isNote;
            lblNote.Text = Mus.NoteName((int)numNum.Value);
            cboChord.Visible = t == "chord";
            cboPattern.Visible = t == "loop";
            lblChordCap.Visible = t == "chord" || t == "loop";
            lblChordCap.Text = t == "loop" ? L.T("Ritmo") : L.T("Acorde");
            lblVelCap.Text = isNote ? L.T("Velocidad (0 = según golpe)") : L.T("Valor (0 = 127)");
            lblVelCap.Visible = numVel.Visible = t != "pc" && t != "loop";
            Validate();
        }
        new void Validate()
        {
            txtKeys.ForeColor = txtKeys.Text.Trim().Length == 0 || Shortcut.Parse(txtKeys.Text) != null ? Theme.Text : Theme.Err;
            bool badSeq = SelAction != null && SelAction.Type == "seq" && Mus.ParseSeq(txtValue.Text) == null;
            txtValue.ForeColor = badSeq ? Theme.Err : Theme.Text;
        }

        void Edit(Action<PadAction> change)
        {
            if (loading || SelAction == null) return;
            change(SelAction);
            RefreshPads();
            ScheduleSave();
        }

        void OnTypeChanged()
        {
            if (loading || SelAction == null) return;
            var a = SelAction;
            var t = TypeIds[Math.Max(0, cboType.SelectedIndex)];
            a.Type = t;
            if (t == "keys") a.Value = txtKeys.Text.Trim();
            else if (TextTypes.Contains(t)) a.Value = txtValue.Text;
            if (t == "seq") { a.Num = 0; if (a.Ch == 10) a.Ch = 2; }
            if (t == "sound")
            {
                a.Value = txtSound.Text; a.Num = 0; a.Vel = 0;
                loading = true; cboSoundMode.SelectedIndex = 0; numSoundVol.Value = 0; loading = false;
            }
            if ((t == "cc" || t == "toggle") && a.Num > 119) a.Num = 20;
            if (t != "loop" && numNum.Maximum > 127)
            {
                if (a.Num > 127) a.Num = 60;
                loading = true; numNum.Maximum = 127; numNum.Value = a.Num; loading = false;
            }
            if (t == "loop")
            {
                if (Array.IndexOf(Looper.Keys, (a.Value ?? "").ToLowerInvariant()) < 0) a.Value = "cumbia";
                if (a.Num < 40) a.Num = 96;
                a.Ch = 10;
                numNum.Maximum = 220;
                numNum.Value = a.Num; numCh.Value = a.Ch;
                cboPattern.SelectedIndex = Array.IndexOf(Looper.Keys, a.Value.ToLowerInvariant());
            }
            UpdateTypeUi();
            RefreshPads();
            ScheduleSave();
        }
        void OnNumChanged()
        {
            if (loading || SelAction == null) return;
            var a = SelAction;
            bool autoName = (a.Type == "note" && a.Name == Mus.NoteName(a.Num)) || (a.Type == "chord" && a.Name == Mus.ChordLabel(a.Num, a.Chord));
            a.Num = (int)numNum.Value;
            lblNote.Text = Mus.NoteName(a.Num);
            if (autoName) txtName.Text = a.Type == "note" ? Mus.NoteName(a.Num) : Mus.ChordLabel(a.Num, a.Chord);
            RefreshPads();
            ScheduleSave();
        }
        void OnChordChanged()
        {
            if (loading || SelAction == null || cboChord.SelectedIndex < 0) return;
            var a = SelAction;
            bool autoName = a.Name == Mus.ChordLabel(a.Num, a.Chord);
            a.Chord = Mus.ChordIds[cboChord.SelectedIndex];
            if (autoName) txtName.Text = Mus.ChordLabel(a.Num, a.Chord);
            RefreshPads();
            ScheduleSave();
        }

        void Browse()
        {
            using (var d = new OpenFileDialog { Filter = L.T("Programas y accesos directos|*.exe;*.lnk;*.bat;*.cmd|Todos los archivos|*.*"), Title = L.T("Elige el programa a abrir") })
                if (d.ShowDialog(this) == DialogResult.OK) txtValue.Text = d.FileName;
        }

        void TestPad()
        {
            var a = SelAction;
            if (a == null) return;
            if (a.Type == "keys" || a.Type == "text")
            {
                Status(L.T("Los atajos y el texto se envían a la ventana activa: abre el programa y toca el pad."));
                return;
            }
            int i = sel;
            Exec(i, a, 100);
            pads[i].Hit = true; pads[i].Invalidate();
            var t = new Timer { Interval = 250 };
            t.Tick += (s, e) => { t.Stop(); t.Dispose(); PadUp(i); };
            t.Start();
        }

        void StartRec()
        {
            recording = true;
            btnRec.Text = L.T("Presiona…");
            btnRec.BackColor = Theme.Warn;
            btnRec.ForeColor = Color.Black;
        }
        void StopRec()
        {
            if (btnRec == null) return;
            recording = false;
            btnRec.Text = L.T("Grabar");
            btnRec.BackColor = Theme.Field;
            btnRec.ForeColor = Theme.Text;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (recording)
            {
                var k = keyData & Keys.KeyCode;
                if (k == Keys.ControlKey || k == Keys.ShiftKey || k == Keys.Menu || k == Keys.LWin || k == Keys.RWin) return true;
                bool win = (GetAsyncKeyState(0x5B) & 0x8000) != 0 || (GetAsyncKeyState(0x5C) & 0x8000) != 0;
                StopRec();
                txtKeys.Text = Shortcut.FromKeys(keyData, win);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Captura de la ventana para pruebas: PadBox.exe --shot archivo.png
        public void Shot(string path)
        {
            using (var bmp = new Bitmap(Width, Height))
            {
                DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
                bmp.Save(path);
            }
        }
    }

    static class Program
    {
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

        public const string Version = "1.1.0";
        public static bool TestMode;   // capturas de prueba: no tocar las luces del pad

        // Diagnóstico: PadBox.exe --lights-test archivo.txt (solo lee; reescribe el color del pad 1 con el mismo valor)
        static string LightsSelfTest()
        {
            var sb = new StringBuilder();
            var ins = Winmm.Inputs(); var outs = Winmm.Outputs();
            int ii = ins.IndexOf("SMC-PAD Pocket"), oi = outs.IndexOf("SMC-PAD Pocket");
            sb.AppendLine("in=" + ii + " out=" + oi);
            if (ii < 0 || oi < 0) return sb.ToString();
            var mi = new MidiIn();
            var mo = new MidiOut();
            if (!mo.Open(oi, "SMC-PAD Pocket")) return sb + "no se pudo abrir la salida";
            var pl = new PadLights(mo, new Dictionary<int, byte[]>());
            mi.SysEx += pl.OnSysEx;
            if (!mi.Open(ii, "SMC-PAD Pocket")) { pl.Shutdown(false); return sb + "no se pudo abrir la entrada"; }
            sb.AppendLine("identidad=" + pl.Identify());
            int bb = pl.ActiveBankBase();
            sb.AppendLine("banco=" + bb);
            if (bb >= 0)
            {
                var recs = pl.ReadReg(5, bb, 416);
                if (recs == null) sb.AppendLine("no se pudo leer el banco");
                else
                {
                    for (int r = 0; r < 16; r++)
                        sb.AppendLine(string.Format("pad{0}: tipo={1} canal={2} nota={3} rgb={4:X2}{5:X2}{6:X2} nivel={7}", r + 1, recs[r * 26], recs[r * 26 + 1], recs[r * 26 + 2], recs[r * 26 + 5], recs[r * 26 + 6], recs[r * 26 + 7], recs[r * 26 + 8]));
                    sb.AppendLine("escritura (mismo color) confirmada=" + pl.WriteReg(5, bb + 5, new[] { recs[5], recs[6], recs[7] }));
                }
            }
            pl.Shutdown(false);
            mi.Dispose();
            return sb.ToString();
        }

        // Main solo marca el proceso como consciente de DPI. Nada aquí puede tocar Theme ni fuentes:
        // si GDI+ arranca antes, guarda 96 DPI y los cuadros de texto salen diminutos en pantallas al 150 %.
        [STAThread]
        static void Main(string[] args)
        {
            try { SetProcessDPIAware(); } catch { }
            // El idioma se fija aquí y no en Run: al compilar Run, .NET ya carga tablas como la de íconos (con sus textos)
            L.Init(args, Path.Combine(AppPaths.Data, "padbox-config.json"));
            Run(args);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Run(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var errLog = Path.Combine(AppPaths.Data, "padbox-error.log");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => { File.AppendAllText(errLog, DateTime.Now + " " + e.Exception + "\r\n"); MessageBox.Show(L.T("Ocurrió un error: ") + e.Exception.Message, "PadBox"); };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => File.AppendAllText(errLog, DateTime.Now + " " + e.ExceptionObject + "\r\n");

            TestMode = args.Any(a => a.StartsWith("--shot"));
            int mt = Array.IndexOf(args, "--mixer-test");   // pruebas: dos bucles sincronizados
            if (mt >= 0 && mt + 1 < args.Length)
            {
                var sb = new StringBuilder();
                using (var mx = new Mixer())
                {
                    sb.AppendLine("mezclador abierto=" + mx.Ok);
                    var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sonidos", "EDM");
                    var drums = Clip.Load(Path.Combine(dir, "01 Bateria.wav")); var bass = Clip.Load(Path.Combine(dir, "04 Bajo.wav"));
                    sb.AppendLine("clips cargados=" + (drums != null) + "/" + (bass != null) + " cuadros=" + (drums != null ? drums.Frames : 0));
                    int grid = (int)Math.Round(Mixer.Rate * 60.0 / 126 * 4 * 2);
                    mx.Play(4, drums, 0.6f, true, grid); long s1 = mx.LastStart;
                    System.Threading.Thread.Sleep(1200);
                    mx.Play(7, bass, 0.6f, true, grid); long s2 = mx.LastStart;
                    sb.AppendLine("ciclo=" + grid + " inicio batería=" + s1 + " inicio bajo=" + s2 + " diferencia=" + (s2 - s1) + " (debe ser múltiplo del ciclo)");
                    System.Threading.Thread.Sleep(6000);
                }
                File.WriteAllText(args[mt + 1], sb.ToString());
                return;
            }
            int st = Array.IndexOf(args, "--sound-test");   // pruebas: abre y reproduce un archivo de audio
            if (st >= 0 && st + 2 < args.Length)
            {
                using (var sp = new SamplePlayer())
                {
                    bool ok = sp.Load(5, args[st + 1]);
                    if (ok) sp.Play(5, 600, false);
                    System.Threading.Thread.Sleep(1500);
                    File.WriteAllText(args[st + 2], ok ? "ok" : "error: " + sp.LastError);
                }
                return;
            }
            int lt = Array.IndexOf(args, "--lights-test");
            if (lt >= 0 && lt + 1 < args.Length) { File.WriteAllText(args[lt + 1], LightsSelfTest()); return; }

            int osdShot = Array.IndexOf(args, "--shot-osd");
            if (osdShot >= 0 && osdShot + 1 < args.Length)
            {
                var osd = new Osd();
                var samples = new[] {
                    osd.Render(L.T("OFICINA"), L.T("Escritorio"), "desktop", Theme.Office, "1 de 6"),
                    osd.Render(L.T("MÚSICA"), "Cumbia", "cumbia", Theme.Music, "2 de 9"),
                    osd.Render(L.T("OFICINA"), "OBS", "obs", Theme.Office, "3 de 6"),
                    osd.Render(L.T("MÚSICA"), L.T("Batería GM"), "drum", Theme.Music, "1 de 9") };
                int cell = 64, cols = 10, rows = (Icons.All.Count + cols - 1) / cols;
                int w = Math.Max(samples.Max(b => b.Width), cols * (cell + 12)) + 24;
                int h = samples.Sum(b => b.Height + 12) + rows * (cell + 30) + 24;
                using (var sheet = new Bitmap(w, h))
                using (var g = Graphics.FromImage(sheet))
                {
                    g.Clear(Color.FromArgb(60, 64, 72));
                    int y = 12;
                    foreach (var b in samples) { g.DrawImage(b, 12, y); y += b.Height + 12; }
                    for (int i = 0; i < Icons.All.Count; i++)
                    {
                        int x = 12 + (i % cols) * (cell + 12), yy = y + (i / cols) * (cell + 30);
                        Icons.Draw(g, new RectangleF(x, yy, cell, cell), Icons.All[i].Key);
                        TextRenderer.DrawText(g, Icons.All[i].Name, Theme.Small, new Rectangle(x - 6, yy + cell + 2, cell + 12, 22), Color.White, TextFormatFlags.HorizontalCenter);
                    }
                    sheet.Save(args[osdShot + 1]);
                }
                return;
            }

            int shot = Array.IndexOf(args, "--shot");
            if (shot >= 0 && shot + 1 < args.Length)
            {
                var f = new MainForm(false);
                int sz = Array.IndexOf(args, "--size"), sw, sh;   // pruebas: --size ancho alto (en unidades al 100 %)
                if (sz >= 0 && sz + 2 < args.Length && int.TryParse(args[sz + 1], out sw) && int.TryParse(args[sz + 2], out sh))
                    f.Size = new Size(sw * f.DeviceDpi / 96, sh * f.DeviceDpi / 96);
                f.Shown += (s, e) => { Application.DoEvents(); f.Shot(args[shot + 1]); Environment.Exit(0); };
                Application.Run(f);
                return;
            }

            bool created;
            using (var mutex = new System.Threading.Mutex(true, "PadBox_SingleInstance", out created))
            {
                if (!created) { MessageBox.Show(L.T("PadBox ya está abierto. Búscalo junto al reloj de Windows."), "PadBox"); return; }
                bool atLogin = false;   // versión de la Store: Windows la abrió al iniciar sesión
                if (AppPaths.Packaged) { try { atLogin = PackagedStartup.LaunchedAtStartup(); } catch (Exception) { } }
                var main = new MainForm(args.Contains("--min") || atLogin);
                int qa = Array.IndexOf(args, "--quit-after");   // pruebas: salir solo (como «Salir» del icono) tras N segundos
                int secs;
                if (qa >= 0 && qa + 1 < args.Length && int.TryParse(args[qa + 1], out secs))
                {
                    var t = new Timer { Interval = secs * 1000 };
                    t.Tick += (s, e) => { t.Stop(); main.Quit(); };
                    t.Start();
                }
                Application.Run(main);
            }
        }
    }
}
