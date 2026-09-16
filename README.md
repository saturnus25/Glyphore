<p align="center">
  <img src="Glyphore/Assets/Glyphore-Logo.png" alt="Glyphoré — Procedural Character Art Studio" width="920" />
</p>
<img width="1919" height="1031" alt="image" src="https://github.com/user-attachments/assets/d263a8c4-4f83-4c56-bad4-251845e8e820" />


# Glyphoré 6.0.0

**English** | [Español](#español)

Glyphoré is a GPU-accelerated procedural character art studio for Windows. It lets you create, customize, animate and export ASCII/Unicode scenes without having to program the effects yourself.

The live preview is rendered directly with OpenGL, while exported character output remains real selectable and copyable text.

## Highlights

- C# / .NET 10 + WinForms
- Native OpenGL 3.3 through WGL / `opengl32.dll`
- GPU-resident real-time preview
- 52 effect families and 353 presets
- 22 color palettes and 27 character ramps
- Spanish and English interface
- Self-contained Windows x64 builds
- FIGlet/ASCII title rendering through Figgle 0.6.6 (Apache-2.0)

Effects include 3D shapes, procedural terrain, SDF scenes, Warp Grid 3D, fire, fireworks, lightning, oceans and waves, cellular automata, fractals, flow fields, configurable galaxies and black holes, oscilloscopes, strange attractors, Voronoi cells, auroras, snowstorms, DNA helices, Conway-style life, reaction-diffusion patterns, boid flocks, orbital systems, falling sand, cloth, volumetric clouds, procedural cities, an advanced Raymarch Lab and more.

## Discord Rich Presence

Discord Rich Presence is integrated through Discord Desktop's local RPC transport using managed .NET named pipes. When enabled (the default), Glyphoré connects automatically if Discord is running and publishes only a short activity category such as **Editing a scene**, **Creating an ASCII title**, **Editing a mask**, **Previewing an animation**, **Exporting an animation** or **Idle**. Filenames, paths, title text, custom layer names and other project content are never included.

The active presence uses the `glyphore_logo` asset. After five minutes without interaction inside Glyphoré it switches to `glyphore_idle`; active export/render work suppresses Idle. Discord may be opened or restarted after Glyphoré and the local connection is retried automatically. **Settings → Discord Rich Presence** can disable the feature immediately and persists that choice. The integration adds no Discord SDK/native DLL and does not use OAuth, bots, tokens, HTTP telemetry or client secrets.

## Real-time editing

Effect parameters can be modified while the animation is running.

Glyphoré provides:

- effect-specific controls
- sliders with editable numeric values
- unrestricted direct numeric input where appropriate
- readable selectors for discrete options
- plain-language tooltips
- live preset switching
- reset controls
- responsive parameter panels

Slider ranges are intended as convenient editing ranges rather than hard limits. Numeric fields can be used to experiment beyond them. Typed values apply automatically after a short pause, while slider movement immediately takes ownership of the value so stale text cannot reappear later.

## Scenes and layers

Glyphoré scenes can contain multiple procedural effect layers. The layer list is ordered from top to bottom and lets you:

- add, duplicate, remove and rename effect layers
- select several layers at once or select all of them for bulk editing
- reorder layers
- show or hide selected layers
- adjust opacity and blend mode across the current selection
- assign a palette per layer, with visible actions to apply the current palette to the selection or to every layer
- assign a character ramp per layer, with the same multi-selection workflow and visible actions to apply the current ramp to selected layers or to every layer
- choose Normal or Additive composition
- optionally enforce strict layer order so the first layer in the list has visual priority, or keep the legacy brightness/coverage mix
- edit each layer with the same effect, preset, parameter and camera controls used by single-effect scenes

Procedural layers are composited on the GPU while keeping both color and glyph ownership per layer. Different layers can therefore use different palettes and different ASCII/Unicode ramps in the same scene, and copied/exported frames preserve those character choices. A one-layer scene follows the same direct rendering path as the original single-effect workflow.

Layers can also own non-layer **Masks**. Masks stay in parent-layer space and support direct preview editing, feather/inversion, gradients and reusable shapes including rectangle, ellipse, rounded rectangle, diamond, ring, triangle presets, regular polygons with 3–32 sides, configurable stars with 3–32 points and procedural noise. Mask rotation can optionally snap softly to 15° increments while Ctrl/Alt temporarily bypasses snapping.

Scenes can be saved as versioned `.glyphore` files and opened later. Scene files preserve the output settings, layer order, visibility, opacity, blend mode, selected effects, presets, seeds, effect parameters and compatible camera values. `Ctrl+S` always opens the save dialog (pre-filled with the current scene name when available), and `Ctrl+O` opens a scene.

## Undo and redo

Scene editing has a built-in history with visible **Undo** and **Redo** buttons plus `Ctrl+Z`, `Ctrl+Y` and `Ctrl+Shift+Z`. The history follows layer operations, parameter changes, palettes, character ramps, camera values and scene-level visual settings. Continuous edits are coalesced so dragging a control does not create hundreds of separate history entries.

## Global transform and post-processing

The final scene can be rotated and given X/Y perspective independently of individual layers. The transform is stored in `.glyphore`, participates in Undo/Redo and is reflected by copied/exported character frames.

After all layers have been combined, the live composition can also be processed with exposure, contrast, saturation, bloom, bloom radius, vignette, scanlines, grain, RGB aberration, posterization, threshold, blur, sharpening, cell pixelation and dithering. Post-processing is stored in `.glyphore` scenes and can be bypassed with one toggle.

## ASCII Title Studio

The **ASCII Title Studio** has its own live GPU preview and can either copy generated ASCII directly or add it as a normal scene layer. ASCII **prefabs** and visual **styles/presets** are intentionally separate: prefabs are real text-to-ASCII fonts that transform the entered text into multi-line character art (including block, outline, dollar, ANSI-shadow, modular, dot-matrix, slanted, dripping, cosmic, compact and wire styles), while styles control palette, glow, crystal highlights, shadows and optional animation. Generated prefab characters are preserved in copied/exported character frames, common Latin accents are supported, and the studio runs as a modeless detached child window so the main editor stays usable.

Titles support size and position, fixed rotation, X/Y perspective, outline, glow, shadow, crystal-like highlights, shimmer, waves, reveal/typewriter and glitch. Animation can be disabled entirely for a static design. The studio is a detached modeless Glyphoré window with its own minimize/taskbar lifecycle; it shares the application icon, never blocks the main editor, and is closed automatically when the main application exits.

## 3D camera

Supported 3D effects share interactive camera controls.

- Drag the preview to orbit or change the view
- Use the mouse wheel to zoom
- Adjust yaw and pitch numerically
- Reset the camera when needed

Camera controls are available where they make sense, including SDF Lab, 3D Shapes, 3D Terrain, Warp Grid 3D and Raymarch Lab.

## Terrain

Glyphoré includes procedural terrain controls for its 3D grid effects, including height, scale, detail, seeded variation, smooth hills, mountain ridges, valleys, terraces, island falloff, ridge strength, edge falloff and water level.

## Lightning

The Lightning effect supports both structural and temporal control.

You can adjust:

- bolt count
- branches
- thickness
- path chaos and branching
- flash rate
- flicker chaos
- flash duration
- base glow
- maximum intensity
- aftershocks
- surrounding glow

This allows anything from isolated dry lightning to unstable storms, Tesla-like arcs and nearly continuous electrical effects.

## Character and color system

Glyphoré includes 27 character ramps, Unicode-compatible character output, 22 built-in color palettes, live palette editing and custom color selection.

The preview also has an adjustable glyph display size. Its 115% default is chosen to approximate a compact terminal-like appearance so block ramps do not show artificial gaps between cells; changing it affects only the preview, not copied or exported text.

The OpenGL preview uses a glyph atlas for performance, but actual text is reconstructed when copying or exporting ASCII/Unicode data.

## Import and export

Glyphoré can export scenes to:

- PowerShell
- selectable HTML
- JSON
- standalone C#
- ANSI
- plain TXT

A visible **Export** button is available in the interface, with `Ctrl+E` as a shortcut. Project scenes are saved separately as `.glyphore` files so they can be reopened and edited rather than treated as flattened exports. Raster/video exports can use PNG Sequence and FFmpeg-backed formats detected at runtime, including alpha-capable profiles when the installed FFmpeg build supports them. Native scene video frames are streamed directly to FFmpeg through a reusable RGBA buffer, and native text/code exports are written frame-by-frame instead of retaining every RGB/alpha/text frame in memory. PowerShell/C#/ANSI can optionally use pseudo-transparency by precompositing character colors against a chosen console background (including Windows console presets or a custom picker); HTML/JSON keep real per-cell alpha.

PowerShell imports are parsed statically for recognized frame data. Glyphoré does **not** execute arbitrary imported PowerShell scripts.

## Generated content and attribution

Animations and other visual content generated with Glyphoré may be used freely, including in personal and commercial projects.

Attribution is not required, but it is explicitly requested and greatly appreciated when reasonably possible.

Suggested credit:

```text
Created with Glyphoré — https://github.com/saturnus25/Glyphore
```

Supported exporters can optionally include Glyphoré attribution. See [`OUTPUT-NOTICE.md`](OUTPUT-NOTICE.md) for more information.

## Performance

The live preview is designed to remain GPU-resident. Procedural effects are evaluated on the GPU and rendered through the character atlas without copying every preview frame back to system memory. GPU readback is performed only when actual character data is required, such as when copying or exporting text.

The status bar displays a compact overview of the current effect, preset, ASCII resolution, target/real FPS, frame submission time and GPU. More detailed renderer information is available through its tooltip.

## Interface

Glyphoré uses a custom dark interface based around its charcoal and copper/amber visual identity.

The interface includes a responsive parameter sidebar, a scene/layer panel, live preview, bilingual controls and tooltips, effect and preset selectors, interactive 3D camera controls, direct numeric editing, keyboard shortcuts and Glyphoré branding/iconography.

## AI-assisted development

Glyphoré has been developed with the help of AI tools across different parts of the process, including code generation and review, debugging, effect design and documentation.

This project is not simply AI-generated code published without review. Its features, effects, behavior and design decisions have been manually tested, adjusted and directed throughout development.

## Build

Install **.NET desktop development** and the **.NET 10 SDK** from Visual Studio Installer.

Open:

```text
Glyphore.sln
```

and build in `Release`, or run:

```powershell
.\Build-Release.ps1
```

You can also use:

```text
Build-Release.cmd
```

The self-contained Windows x64 executable is written to:

```text
publish\win-x64\Glyphore.exe
```

The build script also creates:

```text
dist\Glyphore-6.0.0-win-x64.zip
```

## Requirements

- Windows x64
- OpenGL 3.3 capable GPU and driver

The self-contained Release build does not require a separate .NET installation.

## Troubleshooting

If Glyphoré encounters an unhandled startup or UI error, diagnostic information is written to:

```text
%LOCALAPPDATA%\Glyphore\crash.log
```

When reporting rendering problems, including the GPU model, driver version, affected effect/preset and a screenshot is useful.

## License

Glyphoré source code is licensed under the [MIT License](LICENSE).

Third-party component licenses can be reviewed offline from **About → Third-Party Licenses**. The complete license texts are embedded as resources inside `Glyphore.exe`; the application does not require an external `LICENSE` file to display them. Those licenses apply only to the listed third-party components and do not replace Glyphoré's MIT license.

---

# Español

Glyphoré es un estudio de arte procedural con caracteres acelerado por GPU para Windows. Permite crear, modificar, animar y exportar escenas ASCII/Unicode sin tener que programar los efectos manualmente.

La vista previa se renderiza directamente con OpenGL, mientras que las exportaciones siguen siendo texto real, seleccionable y copiable.

## Características

- C# / .NET 10 + WinForms
- OpenGL 3.3 nativo mediante WGL / `opengl32.dll`
- Preview GPU en tiempo real
- 52 familias de efectos y 353 presets
- 22 paletas de color y 27 rampas de caracteres
- Interfaz en español e inglés
- Builds self-contained para Windows x64
- Renderizado FIGlet/títulos ASCII mediante Figgle 0.6.6 (Apache-2.0)

Entre los efectos se incluyen figuras 3D, terreno procedural, escenas SDF, Warp Grid 3D, fuego, fireworks, rayos, océanos y ondas, autómatas celulares, fractales, flow fields, galaxias y agujeros negros configurables, osciloscopios, strange attractors, celdas Voronoi, auroras, tormentas de nieve, hélices de ADN, patrones tipo Conway, reaction-diffusion, enjambres boid, sistemas orbitales, arena, tela, nubes volumétricas, ciudades procedurales, un Raymarch Lab avanzado y más.

## Discord Rich Presence

Discord Rich Presence se integra mediante el RPC local de Discord Desktop usando named pipes administrados de .NET. Cuando está activado (por defecto), Glyphoré se conecta automáticamente si Discord está abierto y publica únicamente una categoría breve de actividad, como **Editing a scene**, **Creating an ASCII title**, **Editing a mask**, **Previewing an animation**, **Exporting an animation** o **Idle**. Nunca se envían nombres de archivos, rutas, texto del título, nombres personalizados de capas ni contenido del proyecto.

La presencia activa usa `glyphore_logo`. Tras cinco minutos sin interacción dentro de Glyphoré cambia a `glyphore_idle`; una exportación/render activo impide entrar en Idle. Discord puede abrirse o reiniciarse después de Glyphoré y la conexión local se reintenta automáticamente. **Ajustes → Discord Rich Presence** permite desactivarlo al instante y guarda la preferencia. La integración no añade Discord SDK/DLL nativa y no usa OAuth, bots, tokens, telemetría HTTP ni client secrets.

## Edición en tiempo real

Los parámetros pueden modificarse mientras la animación está funcionando.

Glyphoré incluye:

- controles específicos para cada efecto
- sliders con valores numéricos editables
- entrada numérica directa sin límites artificiales cuando corresponde
- selectores legibles para opciones discretas
- tooltips sencillos
- cambio de presets en vivo
- controles de reinicio
- panel de parámetros responsive

El rango de un slider funciona como un rango cómodo de edición, no necesariamente como un límite absoluto. Las cajas numéricas permiten experimentar fuera de ese rango. Los valores escritos se aplican automáticamente tras una pequeña pausa, y mover el slider actualiza inmediatamente la caja para que nunca reaparezcan valores antiguos.

## Escenas y capas

Las escenas de Glyphoré pueden contener varias capas de efectos procedurales. La lista se ordena de arriba hacia abajo y permite:

- añadir, duplicar, eliminar y renombrar capas de efectos
- seleccionar varias capas a la vez o seleccionarlas todas para editarlas en bloque
- reordenar las capas
- mostrar u ocultar las capas seleccionadas
- ajustar opacidad y modo de mezcla para toda la selección
- asignar una paleta por capa, con botones visibles para aplicar la paleta actual a la selección o a todas las capas
- asignar una rampa de caracteres por capa, con el mismo flujo de multiselección y botones visibles para aplicarla a las capas seleccionadas o a todas
- elegir composición Normal o Additive
- forzar opcionalmente el orden estricto de capas para que la primera de la lista tenga prioridad visual, o conservar la mezcla anterior por brillo/cobertura
- editar cada capa con los mismos efectos, presets, parámetros y controles de cámara del flujo clásico de un solo efecto

Las capas procedurales se componen en la GPU conservando por capa tanto el color como qué glifo debe ocupar cada celda. Por tanto, distintas capas pueden usar paletas y rampas ASCII/Unicode diferentes dentro de una misma escena, y los frames copiados o exportados conservan esos caracteres. Una escena con una sola capa utiliza el mismo camino de renderizado directo que el flujo original.

Las capas también pueden tener **Masks** que no son capas independientes. Permanecen en el espacio de su capa padre y admiten edición directa en preview, feather/inversión, gradientes y formas reutilizables: rectángulo, elipse, rectángulo redondeado, rombo, anillo, presets de triángulo, polígonos regulares de 3–32 lados, estrellas configurables de 3–32 puntas y ruido procedural. La rotación puede usar snapping suave a incrementos de 15° y Ctrl/Alt lo ignora temporalmente.

Las escenas pueden guardarse como archivos `.glyphore` versionados y abrirse posteriormente. Conservan los ajustes de salida, orden de capas, visibilidad, opacidad, modo de mezcla, efectos, presets, seeds, parámetros y valores de cámara compatibles. `Ctrl+S` abre siempre el diálogo de guardado (proponiendo el nombre actual cuando existe) y `Ctrl+O` abre una escena.

## Deshacer y rehacer

La edición de escenas incluye historial propio con botones visibles de **Deshacer** y **Rehacer**, además de `Ctrl+Z`, `Ctrl+Y` y `Ctrl+Shift+Z`. El historial cubre operaciones de capas, parámetros, paletas, rampas de caracteres, cámara y ajustes visuales de la escena. Los cambios continuos se agrupan para que arrastrar un control no genere cientos de pasos independientes.

## Transformación global y postprocesado

La escena final puede rotarse y recibir perspectiva X/Y de forma independiente a cada capa. La transformación se guarda en `.glyphore`, participa en Deshacer/Rehacer y se refleja en los frames de caracteres copiados/exportados.

Después de combinar todas las capas, la composición final de la preview también puede procesarse con exposición, contraste, saturación, bloom, radio de bloom, viñeta, scanlines, grano, aberración RGB, posterización, threshold, desenfoque, nitidez, pixelado por celdas y dithering. El postprocesado se guarda dentro de las escenas `.glyphore` y puede desactivarse con un solo toggle.

## Estudio de títulos ASCII

El **Estudio de títulos ASCII** tiene preview GPU propia y permite tanto copiar directamente el ASCII generado como añadirlo como una capa normal de la escena. Los **prefabs ASCII** y los **estilos/presets visuales** están separados: los prefabs son fuentes reales de texto-a-ASCII que transforman lo escrito en arte de caracteres multilínea (bloques, contorno, dólares, sombra ANSI, modular, dot-matrix, inclinado, dripping, cosmic, compacto y wireframe), mientras que el estilo controla paleta, resplandor, cristal, sombras y animación opcional. Los caracteres del prefab se conservan al copiar/exportar, se admiten acentos latinos comunes y la ventana funciona de forma modeless/desacoplada para que el editor principal siga siendo usable.

Los títulos permiten tamaño y posición, rotación fija, perspectiva X/Y, contorno, resplandor, sombra, reflejos tipo cristal, shimmer, ondas, revelado/typewriter y glitch. La animación puede desactivarse por completo para crear un diseño estático. El estudio funciona como una ventana modeless desacoplada de Glyphoré, con minimizado y entrada de barra de tareas propios; comparte el icono de la aplicación, no bloquea el editor principal y se cierra automáticamente al cerrar Glyphoré.

## Cámara 3D

Los efectos 3D compatibles utilizan controles de cámara interactivos compartidos.

- Arrastra sobre la preview para rodear o cambiar la vista
- Usa la rueda del ratón para hacer zoom
- Ajusta yaw y pitch numéricamente
- Reinicia la cámara cuando sea necesario

Actualmente se utiliza en los efectos donde tiene sentido, incluyendo SDF Lab, 3D Shapes, 3D Terrain, Warp Grid 3D y Raymarch Lab.

## Terreno

Glyphoré permite generar y deformar terreno procedural mediante controles de altura, escala, detalle, variación por seed, colinas suaves, crestas, valles, terrazas, islas, caída de bordes y nivel de agua.

## Rayos

El efecto Lightning permite controlar tanto la estructura del rayo como su comportamiento temporal.

Puedes modificar:

- cantidad de rayos
- ramas
- grosor
- caos y bifurcación de la trayectoria
- frecuencia de parpadeo
- caos del parpadeo
- duración del destello
- luz base
- intensidad máxima
- réplicas
- halo luminoso

Esto permite crear desde rayos secos aislados hasta tormentas inestables, arcos tipo Tesla o efectos eléctricos casi continuos.

## Caracteres y color

Glyphoré incluye 27 rampas de caracteres, salida compatible con Unicode, 22 paletas integradas, edición de paletas en vivo y selección personalizada de colores.

La preview permite ajustar el tamaño visual de los glifos. El valor predeterminado del 115% está elegido para aproximarse al aspecto compacto de una terminal, evitando huecos artificiales en rampas como Blocks; este ajuste solo cambia la preview y no altera el texto copiado o exportado.

La preview OpenGL utiliza un atlas de glifos para mantener el rendimiento, pero el texto real se reconstruye al copiar o exportar datos ASCII/Unicode.

## Importar y exportar

Glyphoré permite exportar escenas a:

- PowerShell
- HTML seleccionable
- JSON
- C# standalone
- ANSI
- TXT

La interfaz incluye un botón visible **Exportar**, además del atajo `Ctrl+E`. Las escenas del proyecto se guardan aparte como archivos `.glyphore`, de forma que puedan abrirse y editarse de nuevo en lugar de quedar como una exportación aplanada. Las exportaciones raster/vídeo pueden usar PNG Sequence y perfiles de FFmpeg detectados en tiempo de ejecución, incluyendo formatos con alpha cuando la build instalada los admite. Los frames de vídeo de una escena nativa se envían directamente a FFmpeg mediante un único buffer RGBA reutilizable, y las exportaciones nativas de texto/código se escriben frame a frame en lugar de retener en memoria todos los textos y planos RGB/alpha. PowerShell/C#/ANSI pueden usar opcionalmente pseudo-transparencia precomponiendo el color de los caracteres contra un fondo de consola elegido (presets de consola de Windows o color personalizado); HTML/JSON conservan alpha real por celda.

La importación de PowerShell analiza estáticamente los datos de frames reconocidos. Glyphoré **no ejecuta scripts PowerShell arbitrarios** durante la importación.

## Contenido generado y atribución

Las animaciones y demás contenido visual generado con Glyphoré pueden utilizarse libremente, incluyendo proyectos personales y comerciales.

La atribución no es obligatoria, pero se solicita expresamente y se agradece siempre que sea razonablemente posible.

Crédito sugerido:

```text
Creado con Glyphoré — https://github.com/saturnus25/Glyphore
```

Los exportadores compatibles pueden incluir opcionalmente la atribución de Glyphoré. Consulta [`OUTPUT-NOTICE.md`](OUTPUT-NOTICE.md) para más información.

## Rendimiento

La preview está diseñada para permanecer residente en GPU. Los efectos procedurales se calculan en la GPU y se renderizan mediante el atlas de caracteres sin copiar cada frame de preview de vuelta a la memoria del sistema. El readback solo se realiza cuando se necesitan los caracteres reales, como al copiar o exportar texto.

La barra de estado muestra de forma compacta el efecto y preset actuales, resolución ASCII, FPS objetivo/reales, tiempo de envío del frame y GPU. Los detalles completos del renderer están disponibles mediante su tooltip.

## Interfaz

Glyphoré utiliza una interfaz oscura personalizada basada en su identidad visual carbón y cobre/ámbar.

Incluye panel de parámetros responsive, panel de escena/capas, preview en vivo, controles y tooltips bilingües, selectores de efectos y presets, cámara 3D interactiva, edición numérica directa, atajos de teclado e iconografía propia de Glyphoré.

## Desarrollo asistido por IA

Glyphoré ha sido desarrollado con apoyo de herramientas de inteligencia artificial en distintas partes del proceso, incluyendo generación y revisión de código, depuración, diseño de efectos y documentación.

El proyecto no es simplemente código generado y publicado sin revisar. Las funciones, efectos, comportamiento y decisiones de diseño han sido probados, ajustados y dirigidos manualmente durante el desarrollo.

## Compilar

Instala **.NET desktop development** y el **.NET 10 SDK** desde Visual Studio Installer.

Abre:

```text
Glyphore.sln
```

y compila en `Release`, o ejecuta:

```powershell
.\Build-Release.ps1
```

También puedes utilizar:

```text
Build-Release.cmd
```

El ejecutable self-contained para Windows x64 queda en:

```text
publish\win-x64\Glyphore.exe
```

El script también genera:

```text
dist\Glyphore-6.0.0-win-x64.zip
```

## Requisitos

- Windows x64
- GPU y driver compatibles con OpenGL 3.3

La build self-contained no requiere instalar .NET por separado.

## Solución de problemas

Si Glyphoré encuentra un error no controlado durante el arranque o en la interfaz, guarda información de diagnóstico en:

```text
%LOCALAPPDATA%\Glyphore\crash.log
```

Al reportar problemas gráficos, resulta útil incluir el modelo de GPU, versión del driver, efecto/preset afectado y una captura.

## Licencia

El código fuente de Glyphoré está publicado bajo la [licencia MIT](LICENSE).

Las licencias de componentes de terceros pueden consultarse sin conexión desde **About → Third-Party Licenses**. El texto completo de cada licencia se integra como recurso dentro de `Glyphore.exe`; la aplicación no necesita un archivo `LICENSE` externo para mostrarlas. Estas licencias se aplican únicamente a los componentes indicados y no sustituyen la licencia MIT de Glyphoré.
