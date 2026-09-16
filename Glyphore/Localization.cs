namespace Glyphore;

internal static class Localization
{
    public static bool English { get; set; }

    private static readonly Dictionary<string, (string Es, string En)> Ui = new(StringComparer.OrdinalIgnoreCase)
    {
        ["group.language"]=("Idioma / Language","Language / Idioma"),
        ["group.scene"]=("Escena y capas","Scene and layers"),
        ["group.effect"]=("Efecto y preset","Effect and preset"),
        ["group.output"]=("Salida","Output"),
        ["group.general"]=("Parámetros generales / oscilaciones","General parameters / oscillations"),
        ["group.specific"]=("Ajustes específicos del efecto","Effect-specific controls"),
        ["group.charset"]=("Caracteres ASCII / Unicode","ASCII / Unicode characters"),
        ["group.color"]=("Color / gradiente","Color / gradient"),
        ["group.shape"]=("Figura","Shape"),
        ["group.postprocess"]=("Postprocesado global","Global post-processing"),
        ["group.transform"]=("Transformación global","Global transform"),
        ["button.restart"]=("↻ Reiniciar","↻ Restart"), ["button.pause"]=("Pausar","Pause"), ["button.resume"]=("Continuar","Resume"),
        ["button.settings"]=("Ajustes","Settings"),
        ["button.benchmark"]=("Benchmark","Benchmark"), ["button.random"]=("🎲 Seed","🎲 Seed"), ["button.copy"]=("Copiar frame","Copy frame"), ["button.addcolor"]=("+ Color","+ Color"),
        ["button.export"]=("Exportar…","Export…"), ["button.import"]=("Importar PS1","Import PS1"), ["button.save"]=("Guardar frame","Save frame"),
        ["button.resetcamera"]=("Reiniciar cámara","Reset camera"), ["button.postreset"]=("Reset post FX","Reset post FX"), ["button.transformreset"]=("Restablecer","Reset transform"),
        ["button.titlestudio"]=("✦ Estudio de títulos ASCII…","✦ ASCII Title Studio…"),
        ["button.layeradd"]=("+ Capa","+ Layer"), ["button.layerduplicate"]=("Duplicar","Duplicate"), ["button.layerremove"]=("Eliminar","Remove"),
        ["button.layerselectall"]=("Seleccionar todo","Select all"), ["button.layerclear"]=("Limpiar selección","Clear selection"), ["button.layerrename"]=("Renombrar","Rename"),
        ["button.paletteselected"]=("Aplicar a seleccionadas","Apply to selected"), ["button.paletteall"]=("Aplicar a todas","Apply to all"),
        ["button.charsetselected"]=("Aplicar a seleccionadas","Apply to selected"), ["button.charsetall"]=("Aplicar a todas","Apply to all"),
        ["button.sceneopen"]=("Abrir escena…","Open scene…"), ["button.scenesave"]=("Guardar escena","Save scene"),
        ["button.maskadd"]=("+ Mask","+ Mask"), ["button.maskremove"]=("Quitar mask","Remove mask"),
        ["button.undo"]=("Deshacer","Undo"), ["button.redo"]=("Rehacer","Redo"),
        ["label.width"]=("Ancho","Width"), ["label.height"]=("Alto","Height"), ["label.duration"]=("Duración","Duration"),
        ["label.opacity"]=("Opacidad","Opacity"),
        ["label.glyphsize"]=("Tamaño visual (%)","Display size (%)"),
        ["check.invert"]=("Invertir rampa","Invert ramp"), ["check.color"]=("Color en preview/export","Color in preview/export"),
        ["check.layervisible"]=("Visible","Visible"),
        ["check.respectlayerorder"]=("Respetar orden de capas","Respect layer order"),
        ["check.previewscenebackground"]=("Sincronizar Preview con fondo de escena","Preview follows scene background"),
        ["check.credit"]=("Incluir crédito de Glyphoré en exportaciones compatibles","Include Glyphoré credit in compatible exports"),
        ["check.postprocess"]=("Activar postprocesado","Enable post-processing"),
        ["specific.none"]=("Este efecto usa los controles generales.","This effect uses the general controls."),
        ["status.init"]=("Inicializando OpenGL…","Initializing OpenGL…")
    };


    private static readonly Dictionary<string, (string Es, string En)> Tips = new(StringComparer.OrdinalIgnoreCase)
    {
        ["button.random"]=("Cambia la semilla para obtener otra variación del mismo efecto.","Changes the seed to create another variation of the same effect."),
        ["button.restart"]=("Vuelve a empezar la animación desde el principio.","Restarts the animation from the beginning."),
        ["button.pause"]=("Congela o continúa la animación.","Pauses or resumes the animation."),
        ["button.benchmark"]=("Mide cuánto tarda en dibujarse el efecto con la GPU.","Measures how quickly the GPU can draw the effect."),
        ["button.copy"]=("Copia el frame visible como texto ASCII real.","Copies the visible frame as real ASCII text."),
        ["button.addcolor"]=("Añade otro color al gradiente.","Adds another color to the gradient."),
        ["button.export"]=("Guarda la animación en PowerShell, HTML, JSON, C#, ANSI o texto.","Exports the animation as PowerShell, HTML, JSON, C#, ANSI or text."),
        ["button.import"]=("Carga frames de un PowerShell reconocido sin ejecutar el script.","Loads frames from a recognized PowerShell file without executing the script."),
        ["button.save"]=("Guarda el frame visible como texto ASCII.","Saves the visible frame as ASCII text."),
        ["button.resetcamera"]=("Devuelve la cámara de este efecto a una vista segura y fácil de leer.","Returns this effect camera to a safe, readable view."),
        ["button.layeradd"]=("Añade otra capa procedural usando el efecto actual.","Adds another procedural layer using the current effect."),
        ["button.layerduplicate"]=("Duplica las capas seleccionadas con todos sus parámetros.","Duplicates the selected layers with all of their parameters."),
        ["button.layerremove"]=("Elimina las capas seleccionadas. La escena siempre conserva al menos una.","Removes the selected layers. A scene always keeps at least one."),
        ["button.layerselectall"]=("Selecciona todas las capas para editarlas en bloque.","Selects every layer for bulk editing."),
        ["button.layerclear"]=("Quita la selección múltiple sin borrar capas.","Clears the multi-selection without deleting layers."),
        ["button.layerrename"]=("Renombra la capa o máscara seleccionada.","Renames the selected layer or mask."),
        ["button.paletteselected"]=("Aplica la paleta actual a las capas seleccionadas.","Applies the current palette to the selected layers."),
        ["button.paletteall"]=("Aplica la paleta actual a todas las capas.","Applies the current palette to every layer."),
        ["button.charsetselected"]=("Aplica la rampa de caracteres actual a las capas seleccionadas.","Applies the current character ramp to the selected layers."),
        ["button.charsetall"]=("Aplica la rampa de caracteres actual a todas las capas.","Applies the current character ramp to every layer."),
        ["button.sceneopen"]=("Abre una escena .glyphore guardada anteriormente.","Opens a previously saved .glyphore scene."),
        ["button.scenesave"]=("Guarda capas, parámetros, cámara y ajustes de salida en una escena .glyphore.","Saves layers, parameters, camera and output settings to a .glyphore scene."),
        ["button.maskadd"]=("Añade una máscara como hija de la capa activa.","Adds a mask as a child of the active layer."),
        ["button.maskremove"]=("Quita la máscara seleccionada de su capa padre.","Removes the selected mask from its parent layer."),
        ["check.invert"]=("Intercambia los caracteres usados para zonas claras y oscuras.","Swaps the characters used for bright and dark areas."),
        ["check.color"]=("Activa o desactiva el color sin quitar el arte ASCII.","Turns color on or off without removing the ASCII art."),
        ["check.respectlayerorder"]=("Activado: la primera capa manda donde tiene contenido visible; su fondo negro o casi negro sigue siendo transparente. Desactivado: mezcla por cobertura/brillo, dejando que las capas inferiores atraviesen también las zonas tenues.","On: the first layer wins where it has visible content; its black or near-black background stays transparent. Off: uses brightness/coverage mixing, allowing lower layers to show through dim areas too."),
        ["check.previewscenebackground"]=("Hace que el fondo de la preview siga el color de fondo de la escena. Elegir un fondo exclusivo de preview desactiva esta sincronización.","Makes the preview background follow the scene background color. Choosing a preview-only background disables this sync."),
        ["check.credit"]=("Añade un crédito discreto en formatos que permiten comentarios o metadatos. Puedes desactivarlo.","Adds a small credit in formats that support comments or metadata. You can turn it off.")
    };

    private static readonly Dictionary<string, string> LabelEn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Rotación global"]="Global rotation", ["Perspectiva X"]="Perspective X", ["Perspectiva Y"]="Perspective Y",
        ["Exposición"]="Exposure", ["Contraste"]="Contrast", ["Saturación"]="Saturation", ["Bloom / resplandor"]="Bloom / glow", ["Radio bloom"]="Bloom radius", ["Viñeta"]="Vignette", ["Scanlines"]="Scanlines", ["Grano"]="Grain", ["Aberración RGB"]="RGB aberration", ["Posterizar"]="Posterize", ["Threshold"]="Threshold", ["Desenfoque"]="Blur", ["Nitidez"]="Sharpen", ["Pixelado"]="Pixelate", ["Dithering"]="Dithering",

        ["Velocidad"]="Speed", ["Escala"]="Scale", ["Gamma"]="Gamma", ["Aspecto"]="Aspect", ["Amplitud"]="Amplitude",
        ["Frecuencia X"]="X frequency", ["Frecuencia Y"]="Y frequency", ["Freq. diagonal"]="Diagonal frequency", ["Freq. radial"]="Radial frequency", ["Freq. temporal"]="Time frequency",
        ["Fase"]="Phase", ["Turbulencia"]="Turbulence", ["Warp"]="Warp", ["Deriva X"]="X drift", ["Deriva Y"]="Y drift", ["Pulso"]="Pulse", ["Densidad"]="Density", ["Iteraciones"]="Iterations",
        ["Altura"]="Height", ["Anchura"]="Width", ["Viento"]="Wind", ["Partículas"]="Particles", ["Tamaño partículas"]="Particle size", ["Ascenso partículas"]="Particle lift",
        ["Cantidad"]="Amount", ["Tamaño"]="Size", ["Gravedad"]="Gravity", ["Rebote"]="Bounce", ["Estela"]="Trail", ["Explosiones"]="Explosions", ["Chispas"]="Sparks", ["Persistencia"]="Persistence",
        ["Cantidad gotas"]="Drop count", ["Tamaño anillo"]="Ring size", ["Grosor"]="Thickness", ["Desvanecimiento"]="Fade", ["Brazos"]="Arms", ["Núcleo"]="Core", ["Brillo núcleo"]="Core brightness", ["Twist"]="Twist", ["Halo"]="Halo",
        ["Anchura brazos"]="Arm width", ["Radio disco"]="Disk radius", ["Aplanado"]="Flattening", ["Densidad estrellas"]="Star density", ["Polvo"]="Dust",
        ["Radio mayor"]="Major radius", ["Radio tubo"]="Tube radius", ["Rotación X"]="X rotation", ["Rotación Y"]="Y rotation", ["Rotación base"]="Base rotation", ["Rotación base X"]="Base X rotation", ["Rotación base Y"]="Base Y rotation", ["Rotación base Z"]="Base Z rotation", ["Velocidad rotación"]="Rotation speed", ["Velocidad giro"]="Spin speed", ["Velocidad giro X"]="X spin speed", ["Velocidad giro Y"]="Y spin speed", ["Velocidad giro Z"]="Z spin speed", ["Centro cámara X"]="Camera center X", ["Centro cámara Y"]="Camera center Y", ["Luz horizontal"]="Light horizontal", ["Luz vertical"]="Light vertical", ["Detalle"]="Detail", ["Estrellas"]="Stars", ["Tamaño estrella"]="Star size", ["Profundidad"]="Depth",
        ["Longitud estela"]="Trail length", ["Separación"]="Spacing", ["Brillo cabeza"]="Head brightness", ["Anillos"]="Rings", ["Altura horizonte"]="Horizon height", ["Perspectiva"]="Perspective", ["Ondulación"]="Waves",
        ["Grosor pulso"]="Pulse thickness", ["Caída"]="Falloff", ["Expansión"]="Expansion", ["Regla"]="Rule", ["Velocidad pasos"]="Step speed", ["Historial"]="History", ["Densidad inicial"]="Initial density", ["Contraste vivo"]="Alive contrast", ["Bloques / scroll"]="Blocks / scroll",
        ["Cantidad ondas"]="Wave count", ["Longitud"]="Length", ["Dirección"]="Direction", ["Apertura"]="Spread", ["Cresta"]="Crest", ["Altura olas"]="Wave height", ["Tamaño ola"]="Wave size", ["Capas"]="Layers", ["Dirección viento"]="Wind direction", ["Caos"]="Chaos", ["Espuma"]="Foam",
        ["Fuentes"]="Sources", ["Frecuencia"]="Frequency", ["Amortiguación"]="Damping", ["Movimiento fuentes"]="Source motion", ["Interferencia"]="Interference", ["Forma de onda"]="Waveform", ["Forma"]="Shape", ["Segunda señal"]="Second signal", ["Desfase"]="Phase offset",
        ["Distorsión"]="Distortion", ["Brillo líneas"]="Line brightness", ["Mix fundamental"]="Mix fundamental", ["Mix armónico 2"]="Mix harmonic 2", ["Mix frecuencia 2"]="Mix frequency 2", ["Mix fase 2"]="Mix phase 2", ["Mix armónico 3"]="Mix harmonic 3", ["Mix frecuencia 3"]="Mix frequency 3", ["Mix fase 3"]="Mix phase 3", ["Bandas"]="Bands", ["Flujo"]="Flow", ["Curvatura"]="Curvature", ["Centelleo"]="Shimmer",
        ["Figura"]="Shape", ["Giro X"]="X spin", ["Giro Y"]="Y spin", ["Giro Z"]="Z spin", ["Cámara"]="Camera", ["Altura cámara"]="Camera height", ["Distancia cámara"]="Camera distance", ["Luz"]="Light", ["Modo"]="Mode", ["Agua"]="Water", ["Rejilla"]="Grid",
        ["Repetición"]="Repeat", ["Repeticiones"]="Repeats", ["Fusión"]="Blend", ["Giro"]="Spin", ["Giro figura"]="Shape spin", ["Cámara horizontal"]="Camera yaw", ["Cámara vertical"]="Camera pitch", ["Trazas"]="Traces", ["Escala flujo"]="Flow scale", ["Fuerza"]="Strength", ["Curl"]="Curl", ["Ramas"]="Branches", ["Bifurcación"]="Forking", ["Destello"]="Flash",
        ["Frecuencia parpadeo"]="Flash rate", ["Caos parpadeo"]="Flicker chaos", ["Duración destello"]="Flash duration", ["Luz base"]="Base glow", ["Intensidad"]="Intensity", ["Réplicas"]="Aftershocks",
        ["Horizonte"]="Horizon", ["Disco"]="Disk", ["Anillos disco"]="Disk rings", ["Anchura anillos"]="Ring width", ["Ángulo disco"]="Disk angle", ["Inclinación disco"]="Disk inclination", ["Lente"]="Lens", ["Halo brillante"]="Bright halo", ["Radio halo"]="Halo radius", ["Anchura halo"]="Halo width", ["Brillo halo"]="Halo brightness", ["Jets"]="Jets", ["Grosor jets"]="Jet thickness", ["Brillo jets"]="Jet brightness", ["Longitud jets"]="Jet length", ["Ángulo jets"]="Jet angle", ["Tipo"]="Type", ["Puntos"]="Points", ["Zoom"]="Zoom", ["Rotación"]="Rotation", ["Brillo"]="Brightness", ["Células"]="Cells", ["Bordes"]="Edges", ["Relleno"]="Fill", ["Deformación"]="Warp", ["Ráfagas"]="Gusts",
        ["Vueltas"]="Turns", ["Radio"]="Radius", ["Peldaños"]="Rungs", ["Inclinación"]="Tilt", ["Onda"]="Wave",
        ["Modo fractal"]="Fractal mode", ["Escala fractal"]="Fractal scale", ["Resplandor fractal"]="Fractal glow", ["Altura terreno"]="Terrain height", ["Tipo terreno"]="Terrain type", ["Escala terreno"]="Terrain scale", ["Suavidad terreno"]="Terrain smoothness", ["Crestas"]="Ridges", ["Valles"]="Valleys", ["Terrazas"]="Terraces", ["Isla / falloff"]="Island / falloff", ["Detalle terreno"]="Terrain detail", ["Nivel de agua"]="Water level",
        ["Tamaño título"]="Title size", ["Espaciado de letras"]="Letter spacing", ["Posición X"]="Position X", ["Posición Y"]="Position Y", ["Contorno"]="Outline", ["Contorno rojo"]="Outline red", ["Contorno verde"]="Outline green", ["Contorno azul"]="Outline blue", ["Contorno alfa"]="Outline alpha",
        ["Resplandor"]="Glow", ["Sombra X"]="Shadow X", ["Sombra Y"]="Shadow Y", ["Sombra"]="Shadow", ["Sombra rojo"]="Shadow red", ["Sombra verde"]="Shadow green", ["Sombra azul"]="Shadow blue", ["Sombra alfa"]="Shadow alpha",
        ["Profundidad 3D"]="3D depth", ["Dirección 3D X"]="3D direction X", ["Dirección 3D Y"]="3D direction Y", ["Opacidad 3D"]="3D opacity", ["Calidad 3D"]="3D quality", ["3D rojo"]="3D red", ["3D verde"]="3D green", ["3D azul"]="3D blue", ["3D alfa"]="3D alpha", ["Cristal"]="Crystal", ["Mezclar colores personalizados con la paleta"]="Mix custom colors with palette",
        ["Onda Y"]="Wave Y", ["Onda X"]="Wave X", ["Longitud onda"]="Wavelength", ["Velocidad onda"]="Wave speed", ["Fase onda"]="Wave phase",
        ["Shimmer"]="Shimmer intensity", ["Velocidad shimmer"]="Shimmer speed", ["Intervalo shimmer (s)"]="Shimmer interval (s)", ["Anchura shimmer"]="Shimmer width", ["Aleatoriedad shimmer"]="Shimmer randomness", ["Fase shimmer"]="Shimmer phase", ["Shimmer rojo"]="Shimmer red", ["Shimmer verde"]="Shimmer green", ["Shimmer azul"]="Shimmer blue", ["Shimmer alfa"]="Shimmer alpha",
        ["Typewriter"]="Reveal / typewriter", ["Glitch"]="Glitch"
    };


    private static readonly Dictionary<string, string> EffectHelpEs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["3D Shapes"]="Renderiza figuras tridimensionales con giro, perspectiva y luz. Arrastra la preview para rodearlas y usa la rueda para acercarte.",
        ["3D Terrain"]="Genera un paisaje 3D procedural que avanza bajo la cámara. Arrastra la preview para cambiar la dirección y usa la rueda para acercarte.",
        ["SDF Lab"]="Experimenta con sólidos 3D y repeticiones. Arrastra la preview para rodear la escena y usa la rueda para acercarte.",
        ["Flow Field"]="Dibuja trazas que siguen remolinos y corrientes invisibles.",
        ["Lightning"]="Genera rayos ramificados y destellos eléctricos.",
        ["Black Hole"]="Crea un agujero negro con disco, lente, estrellas y jets.",
        ["Strange Attractor"]="Dibuja sistemas caóticos que forman curvas y nubes matemáticas.",
        ["Voronoi Cells"]="Crea mosaicos de células móviles y sus fronteras.",
        ["Snowstorm"]="Simula nieve con profundidad, viento y ráfagas.",
        ["DNA Helix"]="Dibuja una doble hélice animada con sensación de profundidad.",
        ["Warp Grid 3D"]="Muestra una rejilla en perspectiva con terreno configurable. Arrastra la preview para cambiar la vista y usa la rueda para acercarte.",
        ["Fire"]="Genera llamas, chispas y fuego movido por viento.",
        ["Fireworks"]="Lanza cohetes y explosiones de chispas.",
        ["Cellular Automaton"]="Crea patrones de celdas donde cada fila nace de la anterior siguiendo una regla.",
        ["Wave Field"]="Mezcla varias ondas que viajan en distintas direcciones.",
        ["Ocean Waves"]="Simula oleaje irregular con varias capas y espuma.",
        ["Ripple Tank"]="Genera ondas circulares desde varios puntos y muestra cómo interfieren.",
        ["Oscilloscope"]="Dibuja señales como seno, cuadrada, triangular o diente de sierra.",
        ["Water Caustics"]="Imita las líneas de luz que se ven bajo agua en movimiento.",
        ["Aurora"]="Crea cortinas ondulantes de luz como una aurora boreal.",
        ["Horizon"]="Dibuja una rejilla en perspectiva que se pierde en el horizonte.",
        ["Ripples"]="Cruza ondas circulares suaves.",
        ["Radio Waves"]="Muestra pulsos que salen de un centro y se expanden.",
        ["Conway Life"]="Simula generaciones de Conway a partir de una semilla procedural.", ["Reaction-Diffusion"]="Genera manchas químicas y patrones orgánicos de reacción-difusión.",
        ["Boids"]="Forma enjambres de agentes que orbitan, se agrupan y se dispersan.", ["N-Body Gravity"]="Crea sistemas orbitales con cuerpos y estelas gravitatorias.",
        ["Falling Sand"]="Simula granos cayendo y acumulándose en un montón.", ["Cloth Simulation"]="Dibuja una malla flexible deformada por tensión, ondas y viento.",
        ["Volumetric Clouds"]="Genera masas de nubes procedurales con detalle y viento.", ["Procedural City"]="Construye un skyline nocturno con edificios, ventanas y parallax.",
        ["ASCII Title"]="Renderiza texto real como una capa procedural estilizable y animable.",
        ["Raymarch Lab"]="Explora fractales 3D raymarched con cámara, iluminación, plegados y resplandor."
    };

    private static readonly Dictionary<string, string> EffectHelpEn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["3D Shapes"]="Renders rotating 3D shapes with perspective and lighting. Drag the preview to orbit and use the wheel to zoom.", ["3D Terrain"]="Generates a procedural 3D landscape moving under the camera. Drag the preview to change viewing direction and use the wheel to zoom.",
        ["SDF Lab"]="Experiments with 3D solids and repetition. Drag the preview to orbit the scene and use the wheel to zoom.", ["Flow Field"]="Draws trails that follow invisible currents and vortices.",
        ["Lightning"]="Generates branching lightning bolts and electrical flashes.", ["Black Hole"]="Creates a black hole with disk, lensing, stars and jets.",
        ["Strange Attractor"]="Draws chaotic systems that form mathematical curves and clouds.", ["Voronoi Cells"]="Creates moving cellular mosaics and their borders.",
        ["Snowstorm"]="Simulates snow with depth, wind and gusts.", ["DNA Helix"]="Draws an animated double helix with a sense of depth.",
        ["Warp Grid 3D"]="Shows a perspective grid with configurable terrain. Drag the preview to change the view and use the wheel to zoom.", ["Fire"]="Generates flames, sparks and wind-driven fire.", ["Fireworks"]="Launches rockets and spark explosions.",
        ["Cellular Automaton"]="Creates cell patterns where each row grows from the previous one using a rule.", ["Wave Field"]="Mixes several waves travelling in different directions.",
        ["Ocean Waves"]="Simulates irregular layered ocean waves and foam.", ["Ripple Tank"]="Creates circular waves from several sources and shows their interference.",
        ["Oscilloscope"]="Draws sine, square, triangle, sawtooth and mixed signals.", ["Water Caustics"]="Imitates moving light patterns seen under water.", ["Aurora"]="Creates flowing light curtains like an aurora.",
        ["Horizon"]="Draws a perspective grid fading into the horizon.", ["Ripples"]="Combines soft circular waves.", ["Radio Waves"]="Shows pulses expanding outward from a center point.",
        ["Conway Life"]="Simulates Conway generations from a procedural seed.", ["Reaction-Diffusion"]="Generates organic chemical reaction-diffusion patterns.",
        ["Boids"]="Builds animated flocks of agents that orbit, group and spread.", ["N-Body Gravity"]="Creates orbital systems with bodies and gravitational trails.",
        ["Falling Sand"]="Simulates grains falling and accumulating into a pile.", ["Cloth Simulation"]="Draws a flexible mesh deformed by tension, waves and wind.",
        ["Volumetric Clouds"]="Generates layered procedural cloud masses with wind.", ["Procedural City"]="Builds a night skyline with buildings, windows and parallax.",
        ["ASCII Title"]="Renders real text as a stylable and animatable procedural layer.",
        ["Raymarch Lab"]="Explores raymarched 3D fractals with camera, lighting, folding and glow."
    };

    public static string Text(string key) => Ui.TryGetValue(key, out var v) ? (English ? v.En : v.Es) : key;
    public static string Tip(string key) => Tips.TryGetValue(key, out var v) ? (English ? v.En : v.Es) : "";
    public static string Label(ParamDesc d) => English ? LabelEn.GetValueOrDefault(d.Label, HumanizeKey(d.Key)) : d.Label;
    public static string Help(ParamDesc d)
    {
        if (!English) return d.Help;
        if (d.Key == "drift_x") return "Moves the internal pattern horizontally.";
        if (d.Key == "drift_y") return "Moves the internal pattern vertically.";
        if (d.Key == "speed") return "Makes the whole animation run slower or faster.";
        if (d.Key == "scale") return "Makes the pattern look larger or smaller.";
        if (d.Key == "turbulence") return "Adds irregular motion and breaks up shapes that look too perfect.";
        if (d.Key == "warp") return "Twists and deforms the pattern.";
        if (d.Key == "pulse") return "Makes the effect grow and shrink as if it were breathing.";
        if (d.Key == "fire_wind") return "Tilts the flames sideways.";
        if (d.Key.StartsWith("firework_")) return $"Controls {Label(d).ToLowerInvariant()} for the firework explosions.";
        if (d.Key.StartsWith("ocean_")) return $"Controls {Label(d).ToLowerInvariant()} for the ocean surface.";
        if (d.Key.StartsWith("wave_")) return $"Controls {Label(d).ToLowerInvariant()} for the wave field.";
        if (d.Key.StartsWith("shape3d_")) return $"Controls {Label(d).ToLowerInvariant()} for the 3D object.";
        if (d.Key.StartsWith("terrain_")) return $"Controls {Label(d).ToLowerInvariant()} for the 3D terrain.";
        if (d.Key == "warpgrid_terrain_height") return "Raises and lowers grid vertices to create hills, mountains and valleys.";
        if (d.Key == "warpgrid_terrain_scale") return "Changes the size of terrain features. Lower values make broader hills; higher values make tighter terrain.";
        if (d.Key == "warpgrid_terrain_smooth") return "Makes the terrain rounder and softer or sharper and rockier.";
        if (d.Key == "warpgrid_terrain_valleys") return "Deepens low areas without making the peaks taller.";
        if (d.Key == "warpgrid_terrain_detail") return "Adds smaller bumps and irregularities on top of the large terrain shapes.";
        if (d.Key == "warpgrid_terrain_mode") return "Chooses the overall terrain style: hills, ridges, terraces or an island.";
        if (d.Key == "warpgrid_terrain_ridges") return "Makes mountain ridges and sharp crests more visible.";
        if (d.Key == "warpgrid_terrain_terraces") return "Turns smooth slopes into stepped terrain. Zero keeps them continuous.";
        if (d.Key == "warpgrid_terrain_island") return "Lowers terrain toward the edges to create islands or isolated plateaus.";
        if (d.Key == "warpgrid_terrain_water") return "Visually fills terrain below this height with a subtle water surface.";
        if (d.Key.StartsWith("blackhole_")) return $"Controls {Label(d).ToLowerInvariant()} around the black hole.";
        if (d.Key == "lightning_speed") return "Changes how many main lightning flashes happen per second.";
        if (d.Key == "lightning_flicker_chaos") return "Makes flash timing and strength less regular and more unpredictable.";
        if (d.Key == "lightning_flash_duration") return "Changes how long each main lightning strike stays visible.";
        if (d.Key == "lightning_base_glow") return "Keeps part of the bolt visible between flashes. Set it to zero for complete darkness between strikes.";
        if (d.Key == "lightning_brightness") return "Changes the peak strength of each strike without changing its shape.";
        if (d.Key == "lightning_aftershock") return "Adds smaller secondary flashes after the main strike.";
        if (d.Key == "title_wave_phase") return "Selects the phase of the wave cycle.";
        if (d.Key == "title_letter_spacing") return "Controls the spacing between letters.";
        if (d.Key == "title_shimmer_frequency") return "Controls the time between shimmer sweeps.";
        if (d.Key == "title_shimmer_width") return "Controls the thickness of the shimmer band.";
        if (d.Key == "title_shimmer_randomness") return "Adds variation to the interval between shimmer sweeps.";
        if (d.Key == "title_shimmer_speed") return "Controls how quickly the shimmer band crosses the title.";
        if (d.Key == "title_shimmer_phase") return "Selects the shimmer position within its cycle.";
        if (d.Key == "title_extrude_depth") return "Controls the depth of the extrusion.";
        if (d.Key == "title_extrude_quality") return "Controls the number of samples used by the extrusion. Values above 16 can significantly reduce rendering performance.";
        if (d.Key == "title_extrude_x") return "Controls the horizontal direction of the 3D extrusion.";
        if (d.Key == "title_extrude_y") return "Controls the vertical direction of the 3D extrusion.";
        if (d.Key == "title_extrude_opacity") return "Controls the opacity of the 3D extrusion.";
        if (d.Key == "title_mix_custom_palette") return "Decides whether the palette influences custom colors.";
        if (d.Key == "title_shadow") return "Controls the strength of the title shadow.";
        if (d.Key == "title_outline") return "Adds an outline around the title.";
        if (d.Key.StartsWith("lightning_")) return $"Controls {Label(d).ToLowerInvariant()} for the lightning effect.";
        if (d.Key.StartsWith("ca_")) return $"Controls {Label(d).ToLowerInvariant()} for the cellular automaton.";
        return $"Changes the {Label(d).ToLowerInvariant()} of this effect.";
    }

    public static ParamDesc Param(ParamDesc d) => English ? d with { Label = Label(d), Help = Help(d) } : d;
    public static string EffectHelp(string effect) => English
        ? EffectHelpEn.GetValueOrDefault(effect, "Generates this kind of animation. Change the controls and watch the result live.")
        : EffectHelpEs.GetValueOrDefault(effect, "Genera este tipo de animación. Cambia los controles y observa el resultado en directo.");

    private static string HumanizeKey(string key)
    {
        string[] knownPrefixes = ["shape3d_","firework_","fire_","ball_","rain_","galaxy_","donut_","star_","matrix_","tunnel_","horizon_","radio_","ca_","wave_","ocean_","tank_","scope_","caustic_","aurora_","terrain_","sdf_","flow_","lightning_","blackhole_","attractor_","voronoi_","snow_","dna_","warpgrid_","life_","rd_","boids_","nbody_","sand_","cloth_","cloud_","city_","title_","raymarch_","scene_"];
        foreach (var p in knownPrefixes) if (key.StartsWith(p, StringComparison.OrdinalIgnoreCase)) { key = key[p.Length..]; break; }
        return string.Join(' ', key.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
    }
}
