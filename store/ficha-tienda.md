# PadBox en la Microsoft Store: textos y respuestas para Partner Center

Copia y pega estos textos en Partner Center (partner.microsoft.com → Apps y juegos → PadBox).

---

## 1. Propiedades

- **Categoría:** Música (secundaria opcional: Productividad)
- **Política de privacidad (URL):** https://yekam97.github.io/padbox/privacidad/
- **Sitio web:** https://yekam97.github.io/padbox/
- **Contacto de soporte:** https://github.com/yekam97/PadBox/issues
- **Requisitos de sistema → hardware recomendado:** «Controlador MIDI USB» no es una opción de la lista; escríbelo en la descripción (ya está incluido abajo).
- **Requisitos de hardware → Teclado y Mouse:** marcarlos como requeridos.

## 2. Clasificación por edades (cuestionario IARC)

- Categoría de la app: **Utilidades, productividad, comunicación u otra** (o «Aplicación de música / creatividad» si aparece).
- ¿Violencia, sexo, lenguaje vulgar, drogas, apuestas? **No** a todo.
- ¿Los usuarios pueden comunicarse o compartir contenido entre sí? **No.**
- ¿Comparte la ubicación del usuario? **No.**
- ¿Permite compras digitales? **No.**
- ¿Navegador web sin restricciones? **No** (un pad puede abrir una página que el usuario elija, en su propio navegador).
- Resultado esperado: **3+ / Todos.**

## 3. Ficha en español (idioma: es)

**Nombre:** PadBox

**Descripción breve (hasta 1000 caracteres; aparece arriba en la página):**

Convierte tu controlador MIDI de pads en un instrumento con ritmos y sonidos propios, y en un panel de atajos para Windows y tus programas. El pad 1 cambia entre Música y Oficina; los pads 2 y 3 cambian de preset.

**Descripción:**

PadBox convierte cualquier controlador MIDI de pads en dos herramientas en una: un instrumento para tocar y crear, y un panel de atajos para trabajar más rápido en Windows.

MODO MÚSICA
• 13 presets: batería rock, cumbia, cumbia villera, vallenato, cumbia rebajada, acordeón, piano, saxofón, guitarra blues, dubstep y más, cada uno con su propio ritmo.
• Drum Pad EDM: 13 sonidos originales creados para PadBox, con bucles que entran sincronizados.
• Tus propios sonidos: arrastra un mp3 o wav sobre cualquier pad. Usa solo sonidos que tengas derecho a usar.
• Melodías paso a paso: cada toque suena la siguiente nota.
• Puede enviar sus notas a otros programas de música mediante un puerto MIDI virtual.

PadBox no incluye canciones de terceros.

MODO OFICINA
• Atajos con un toque para Windows y el navegador, y presets con atajos habituales de programas como OBS, Adobe Photoshop, Adobe Illustrator y Microsoft Word/Excel.
• Controles de música y volumen.
• Cada grupo de funciones tiene su color para encontrarlo de un vistazo.
• Asigna cualquier atajo, texto, programa o página web a cualquier pad.

CONTROLADORES
• Probado con el M-VAVE SMC-PAD Pocket, con colores RGB en los pads según el preset.
• Incluye perfiles para otros modelos de M-VAVE, Akai (MPD218, LPD8, MPK Mini) y Novation Launchpad, que todavía no se han probado con el aparato real.
• Cualquier otro controlador MIDI de 8 o 16 pads: con «Detectar pads», PadBox aprende cuál es cada pad.

Requiere un controlador MIDI USB. PadBox no recoge datos ni necesita internet.

PadBox es una app independiente: no está afiliada a M-VAVE, Akai, Novation, OBS, Adobe ni a otras marcas mencionadas, ni cuenta con su respaldo. Sus nombres se usan solo para indicar compatibilidad y pertenecen a sus dueños.

**Novedades de esta versión:**

Primera versión de PadBox en la Microsoft Store.

**Características del producto (una por línea, hasta 20):**

- Dos modos en un solo pad: Música y Oficina
- 13 presets musicales con ritmos en bucle
- Sonidos EDM originales con bucles sincronizados
- Carga tus propios sonidos mp3 y wav
- Atajos para Windows y tus programas
- Colores en los pads según el preset (SMC-PAD Pocket; Launchpad sin probar)
- Pensado para controladores MIDI de 8 o 16 pads
- Aviso en pantalla al cambiar de modo o de preset
- Envía notas a otros programas por un puerto MIDI virtual
- Sin cuentas, sin anuncios y sin conexión a internet

**Palabras clave (hasta 7, sin nombres de otros productos):** controlador MIDI, drum pad, pads, cumbia, atajos, macro, teclas rápidas

**Capturas de pantalla:** sube las 3 de `PadBox\docs\` (captura-primera-vez.png, captura-oficina.png, captura-edm.png).

**Video (opcional, «Tráiler»):** `PadBox\video\out\PadBox-promo.mp4` (30 s, 1080p).

## 4. Ficha en inglés (opcional, si agregas el idioma en-US)

**Short description:** Turn your MIDI pad controller into an instrument with its own rhythms and sounds, and into a shortcut panel for Windows and your programs. Pad 1 switches between Music and Office; pads 2 and 3 switch presets.

**Description:** PadBox turns a MIDI pad controller into two tools in one: an instrument to play and create, and a shortcut panel to work faster in Windows. Music mode includes 13 presets with looped rhythms, a set of original sounds with synced loops, and your own mp3/wav sounds (use only sounds you have the right to use). It does not include third-party songs. Office mode gives one-touch shortcuts for Windows and browsers, plus presets with common shortcuts for programs such as OBS, Adobe Photoshop, Adobe Illustrator and Microsoft Word/Excel, color-coded by function. Tested on the M-VAVE SMC-PAD Pocket (with RGB pad colors). It also includes profiles for other M-VAVE, Akai and Novation models that have not yet been tested on real hardware, and pad detection for other 8 or 16-pad MIDI controllers. Requires a USB MIDI controller. No data collection, no internet needed. The app's interface is in Spanish. PadBox is independent and is not affiliated with or endorsed by M-VAVE, Akai, Novation, OBS, Adobe or other brands mentioned; their names are used only to indicate compatibility and belong to their owners.

## 5. Paquete

- Sube `PadBox\dist\PadBox-<versión>.msix` (lo crea `store\build-msix.ps1`).
- `store\identity.json` ya tiene los valores de **Identidad del producto** de Partner Center (GonStudio.PadBox).
- Disponibilidad: todos los mercados; precio: **Gratis**.

## 6. Notas para la certificación (Submission options → Notes for certification)

PadBox is a desktop app (full trust) that reads a USB MIDI pad controller through the Windows multimedia MIDI API and, in its "Office" mode, sends the keyboard shortcuts the user assigns to each pad (SendInput), only when that pad is pressed. runFullTrust is required for MIDI input, keyboard shortcut output and the optional pad lighting control. The app works without a controller: testers can click any pad on screen to see its settings, use the "Probar" (Test) button to hear sounds and loops through the Windows synthesizer, and switch presets/modes with the buttons at the top. The interface is in Spanish. No account, no internet access, no data collection.

## 7. Justificación de la capacidad restringida runFullTrust

PadBox is a classic Win32 (.NET Framework) desktop application packaged with MSIX. It needs full trust to open USB MIDI devices with the winmm MIDI API, to send keyboard shortcuts to the foreground app when the user presses a pad (Office mode), to play audio with waveOut, and to control the RGB lights of supported controllers via MIDI SysEx.
