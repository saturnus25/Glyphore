# Changelog

All notable changes to Glyphoré are documented in this file.

## 6.0.0

This release contains the changes made since AsciiForge 5.2.0.

### Project rename and identity

- Renamed the project from **AsciiForge** to **Glyphoré**. Tooling-safe filenames, namespaces, assembly names and binaries use `Glyphore` without the accent.
- Reworked the application identity around the Glyphoré charcoal and copper/amber palette, with a new icon, branded header and refreshed WinForms interface.
- Updated solution/project metadata, build scripts, export attribution, repository links and release assets for Glyphoré 6.0.0.
- Added a Windows GitHub Actions workflow for .NET 10 Release builds.

### Scenes, layers and non-destructive editing

- Added a real scene architecture with ordered procedural layers, per-layer visibility, opacity, blend mode, palette and character ramp.
- Added multi-selection and bulk editing, strict layer ordering, layer duplication/reordering/renaming and a detachable Scene Layers window.
- Added versioned `.glyphore` scene save/load with backward-compatible migration through scene format v9.
- Added scene-wide Undo/Redo with `Ctrl+Z`, `Ctrl+Y` and `Ctrl+Shift+Z`; continuous edits are coalesced into useful history steps.
- Added global scene rotation and X/Y perspective plus a post-processing stage with exposure, contrast, saturation, bloom, vignette, scanlines, grain, RGB aberration, posterization, threshold, blur, sharpening, pixelation and dithering.
- Added reusable per-layer masks with inversion, feathering, gradients and direct preview manipulation. Shapes include rectangles, ellipses, rounded rectangles, diamonds, rings, configurable triangles, 3–32-sided polygons, 3–32-point stars and procedural noise.
- Added mask rotation snapping, snap markers, live drag updates and mask-aware scene persistence.

### ASCII Title Studio

- Added a modeless ASCII Title Studio with its own live GPU preview, detachable preview window and direct **Copy ASCII** workflow.
- Added FIGlet-style title rendering through Figgle 0.6.6 and Figgle.Fonts 0.6.6, including 250+ selectable fonts and graceful handling of common Latin accents.
- Separated text prefabs from visual styles so typography, palette, glow and animation can be edited independently.
- Added outline, glow, shadow, custom colors, 3D extrusion, rotation, X/Y perspective, shimmer, waves, reveal/typewriter, glitch and Fade Reveal controls.
- Added configurable letter spacing, static/animated modes, replay controls, 3D quality up to 128 samples and deterministic animation in detached previews and exports.
- Added FIT, FILL and STRETCH preview framing plus editor-only preview zoom without changing exported geometry.

### Effects, cameras and controls

- Added nine procedural effect families: Conway Life, Reaction-Diffusion, Boids, N-Body Gravity, Falling Sand, Cloth Simulation, Volumetric Clouds, Procedural City and Raymarch Lab.
- Expanded the application to **52 effect families and 353 presets**.
- Added Raymarch Lab with Mandelbox-style folding, kaleidoscopic geometry, gyroid modes, camera controls, movable lighting and volumetric glow.
- Generalized interactive 3D camera controls across SDF Lab, 3D Shapes, 3D Terrain, Warp Grid 3D and Raymarch Lab.
- Expanded Warp Grid 3D terrain with seeded variation, hills, ridges, terraces, island modes, edge falloff and water level controls, plus new terrain presets.
- Expanded Black Hole, Rotating Galaxy, Oscilloscope and Lightning controls and retuned their presets.
- Replaced numeric-only controls with readable selectors where settings represent discrete modes.
- Restored unrestricted typed values for slider-backed settings; slider ranges now act as convenient editing ranges rather than hard limits.

### Export, alpha and performance

- Added RGBA raster/video export so masks, fades, glow and transparent backgrounds preserve alpha.
- Added runtime FFmpeg capability detection and profiles for MP4, GIF, WebM and supported alpha-capable VP9, ProRes 4444, qtrle and APNG outputs. PNG Sequence remains available without FFmpeg.
- Streamed native raster frames directly to FFmpeg through a reusable RGBA buffer, preventing memory use from growing with animation length.
- Reworked PowerShell, HTML, JSON, standalone C#, ANSI and TXT generation to write frames incrementally instead of retaining the full animation in memory.
- Added real per-cell alpha to HTML and JSON. PowerShell, standalone C# and ANSI can optionally approximate partial alpha by precompositing colors against a selected terminal background.
- Preserved the actual per-layer colors and character ownership in copied/exported frames.
- Added an export progress window with separate rendering, frame-preparation and FFmpeg stages.
- Removed the old arbitrary 1000×500, 240 FPS and fixed-duration editor limits; impossible frame counts are now validated explicitly.

### Discord Rich Presence

- Added optional Discord Rich Presence through Discord Desktop's local RPC named pipes using managed .NET APIs only.
- Added activity states for scene editing, ASCII Title Studio, masks, preview/replay, export and five-minute Glyphoré-local idle.
- Presence never includes filenames, paths, title text, custom layer names or other project content.
- Added **Settings → Discord Rich Presence**; the setting is enabled by default, can be changed immediately and is stored locally.
- The integration adds no Discord SDK, native DLL, OAuth, bot, token, client secret or HTTP telemetry.

### Interface, reliability and maintenance

- Restored the native Windows frame so Windows owns dragging, resizing, Snap, minimize/maximize, DPI and multi-monitor behavior.
- Added themed scrolling, safer mouse-wheel routing, responsive layouts and improved high-contrast controls.
- Added adjustable preview glyph size and editor-only preview backgrounds.
- Added an About window with offline third-party license viewing; the complete Apache-2.0 text for Figgle is embedded in the single-file executable.
- Refactored the main form, renderer, controls, export pipeline and shader bindings into focused components.
- Added release smoke tests for embedded licenses, title persistence, transparent export, Discord RPC and detached/native window behavior.
- Hardened OpenGL resource cleanup, WGL extension validation, renderer cache updates, shader compatibility and startup diagnostics.
- Fixed ordered layer composition, title coordinate/scaling/clipping issues, export memory growth, transparent-edge artifacts, preview cache races, WinForms analyzer errors and multiple compile regressions found during the 6.0.0 stabilization passes.

## 5.2.0

- Added a clear **Export / Exportar** button to the Output section while keeping `Ctrl+E` as a shortcut.
- Added optional AsciiForge attribution to PowerShell, HTML, JSON and standalone C# exports. It is enabled by default and can be disabled.
- Added `OUTPUT-NOTICE.md`: generated animations may be used freely, while attribution is explicitly requested when reasonably possible.
- Expanded Warp Grid 3D with procedural heightfield terrain: terrain height, scale, smoothness, valley depth and fine detail.
- Added six terrain-focused Warp Grid 3D presets: Mountain Range, Rolling Hills, Deep Valleys, Rugged Peaks, Soft Dunes and Alien Highlands.
- Refactored I/O/export code out of the main form and moved effect IDs and shader-uniform mappings into dedicated registries.
- Replaced parallel uniform arrays with paired uniform bindings.
- Added OpenGL uniform-location caching to reduce repeated driver lookups during rendering.
- Kept the SDF Lab orbit camera and finite repetition changes from 5.1.2.
- Updated project and release metadata to 5.2.0.

## 5.1.2

- Reworked SDF Lab repetition so repeated scenes are finite and the camera no longer starts inside repeated geometry.
- Added SDF camera yaw, pitch and copy spacing controls.
- Added direct mouse orbit in SDF Lab: drag the preview to rotate the camera and use the mouse wheel to zoom.
- Retuned all SDF Lab presets for readable outside views of repeated forms.

## 5.1.1

- Added live Spanish / English UI switching.
- Added English parameter labels, tooltips, dialogs and repository README.
- Kept effect and preset identifiers language-neutral for compatible presets/exports.

## 5.1.0

- Added 11 new GPU effect families: 3D Shapes, 3D Terrain, SDF Lab, Flow Field, Lightning, Black Hole, Strange Attractor, Voronoi Cells, Snowstorm, DNA Helix and Warp Grid 3D.
- Added dozens of presets for the new engines.
- 3D Shapes supports sphere, cube, octahedron, torus, cylinder, capsule and pyramid with XYZ rotation, camera, perspective and lighting controls.
- Kept the 5.0.5 drift fix, simple control tooltips, wave/water engines and expanded Cellular Automaton.
