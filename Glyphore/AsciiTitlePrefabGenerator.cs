using Figgle;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Glyphore;

/// <summary>
/// FIGlet-backed title font renderer. A title prefab is only the geometry/font used to
/// transform input text into multi-line character art. Palette, glow, outline, wave,
/// shimmer and all other visual styling stay completely independent.
/// </summary>
internal static class AsciiTitlePrefabGenerator
{
    private const string Prefix = "FIGlet · ";

    private static readonly Lazy<IReadOnlyDictionary<string, FiggleFont>> FontMap = new(BuildFontMap);
    private static readonly Dictionary<(string Font, int Rune), bool> RuneSupportCache = new();
    private static readonly object RuneSupportLock = new();

    // Compatibility for scenes created during the early custom-prefab experiments.
    private static readonly Dictionary<string, string> LegacyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Classic Block"] = "Block",
        ["Oldschool Outline"] = "Old Banner",
        ["Newschool Dollar"] = "Big Money-ne",
        ["ANSI Shadow"] = "ANSI Shadow",
        ["Modular"] = "Modular",
        ["Dot Matrix"] = "Dot Matrix",
        ["Slant"] = "Slant",
        ["Bloody"] = "Bloody",
        ["Cosmic"] = "Cosmic",
        ["Bigfig"] = "Bigfig",
        ["Wireframe"] = "Isometric1",
        ["Double Line"] = "Double",
        ["FIGlet · Oldschool"] = "Old Banner",
        ["FIGlet · Dollar"] = "Big Money-ne",
        ["FIGlet · Wireframe"] = "Isometric1",
        ["FIGlet · Double Line"] = "Double"
    };

    // The dropdown is built from the actual Figgle font bundle (250+ fonts), rather than
    // a small set of fake effects. Common fonts are promoted to the top, then the rest
    // are listed alphabetically.
    public static string[] Names
    {
        get
        {
            var map = FontMap.Value;
            string[] preferred =
            [
                "Standard", "Slant", "ANSI Shadow", "Big", "Block", "Doom", "Bloody",
                "Dot Matrix", "Old Banner", "Banner", "Small", "Mini", "Shadow", "Script",
                "Graffiti", "Poison", "Cosmic", "Bigfig", "Modular", "Double"
            ];

            var ordered = new List<string>(map.Count);
            foreach (string name in preferred)
                if (map.ContainsKey(name) && !ordered.Contains(name, StringComparer.OrdinalIgnoreCase))
                    ordered.Add(name);
            foreach (string name in map.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                if (!ordered.Contains(name, StringComparer.OrdinalIgnoreCase))
                    ordered.Add(name);
            return ordered.Select(name => Prefix + name).ToArray();
        }
    }

    public static bool IsGeneratedPrefab(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return true;
        return LegacyAliases.ContainsKey(name);
    }

    public static string Generate(string? text, string? prefab, int letterSpacing = 0)
    {
        string source = string.IsNullOrEmpty(text) ? " " : text.Replace("\r", string.Empty);
        FiggleFont font = ResolveFont(prefab, out string resolvedName);
        var output = new List<string>();
        int spacing = Math.Max(0, letterSpacing);

        string[] sourceLines = source.Split('\n');
        for (int i = 0; i < sourceLines.Length; i++)
        {
            string rendered;
            if (spacing == 0 && !ContainsWhitespaceElement(sourceLines[i]))
            {
                // Compatibility path: zero tracking is byte-for-byte the same rendering route
                // Glyphoré used before title_letter_spacing existed, including FIGfont fitting
                // and smushing rules chosen by Figgle.
                string prepared = PrepareInput(font, resolvedName, sourceLines[i]);
                rendered = RenderPrepared(font, prepared);
            }
            else
            {
                // Compose whitespace explicitly instead of asking the FIGfont to render a tiny
                // space FIGcharacter. This keeps word spacing independent from letter tracking.
                // With zero tracking, complete words still take Figgle's original smushing path.
                rendered = RenderLineWithSpacing(font, resolvedName, sourceLines[i], spacing);
            }

            output.AddRange(rendered.Replace("\r", string.Empty).Split('\n').Select(x => x.TrimEnd()));
            if (i != sourceLines.Length - 1) output.Add(string.Empty);
        }

        return TrimOuterBlankLines(output);
    }

    private static string RenderPrepared(FiggleFont font, string prepared)
    {
        try
        {
            return font.Render(prepared);
        }
        catch
        {
            // Last-resort path: never turn Latin accented letters into question marks.
            return font.Render(FoldToPortableAscii(prepared));
        }
    }

    private static string RenderLineWithSpacing(FiggleFont font, string fontName, string source, int spacing)
    {
        if (source.Length == 0)
            return RenderPrepared(font, PrepareInput(font, fontName, source));

        var elements = new List<string>();
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(source.Normalize(NormalizationForm.FormC));
        while (enumerator.MoveNext())
            elements.Add(enumerator.GetTextElement());

        if (elements.Count <= 1 && !elements.Any(IsWhitespaceElement))
            return RenderPrepared(font, PrepareInput(font, fontName, source));

        var parts = new List<(FigCharacterBlock? Block, int WordSpaces)>();
        if (spacing == 0)
        {
            // Render each complete word in one call so Figgle retains the font's fitting and
            // smushing rules inside that word. Whitespace is represented separately below.
            var word = new StringBuilder();
            foreach (string element in elements)
            {
                if (!IsWhitespaceElement(element))
                {
                    word.Append(element);
                    continue;
                }

                FlushWord();
                parts.Add((null, 1));
            }
            FlushWord();

            void FlushWord()
            {
                if (word.Length == 0) return;
                parts.Add((RenderFigCharacterBlock(font, fontName, word.ToString()), 0));
                word.Clear();
            }
        }
        else
        {
            foreach (string element in elements)
            {
                if (IsWhitespaceElement(element))
                    parts.Add((null, 1));
                else
                    parts.Add((RenderFigCharacterBlock(font, fontName, element), 0));
            }
        }

        FigCharacterBlock[] blocks = parts
            .Where(part => part.Block.HasValue)
            .Select(part => part.Block!.Value)
            .ToArray();
        int height = Math.Max(1, blocks.Length == 0 ? 1 : blocks.Max(block => block.Rows.Length));
        int wordGap = CalculateWordGap(font, fontName, height, spacing);
        var rows = Enumerable.Range(0, height).Select(_ => new StringBuilder()).ToArray();
        bool previousWasGlyph = false;
        foreach ((FigCharacterBlock? nullableBlock, int wordSpaces) in parts)
        {
            if (!nullableBlock.HasValue)
            {
                foreach (StringBuilder row in rows)
                    row.Append(' ', wordGap * wordSpaces);
                previousWasGlyph = false;
                continue;
            }

            if (previousWasGlyph && spacing > 0)
            {
                foreach (StringBuilder row in rows)
                    row.Append(' ', spacing);
            }

            FigCharacterBlock block = nullableBlock.Value;
            for (int y = 0; y < rows.Length; y++)
            {
                string row = y < block.Rows.Length ? block.Rows[y] : string.Empty;
                rows[y].Append(row);
                if (row.Length < block.Width)
                    rows[y].Append(' ', block.Width - row.Length);
            }
            previousWasGlyph = true;
        }

        return string.Join("\n", rows.Select(row => row.ToString().TrimEnd()));
    }

    private static int CalculateWordGap(FiggleFont font, string fontName, int renderedHeight, int letterSpacing)
    {
        // FIGfont height is the stable, font-independent metric for an em-like advance. Sample
        // glyph widths keep very narrow/tall fonts readable without any font-name exceptions.
        int[] sampleWidths = new[] { "A", "I", "M", "W" }
            .Select(sample => RenderFigCharacterBlock(font, fontName, sample).Width)
            .OrderBy(width => width)
            .ToArray();
        int typicalWidth = sampleWidths[sampleWidths.Length / 2];
        int heightGap = (int)Math.Ceiling(renderedHeight * 0.75);
        int widthGap = (int)Math.Ceiling(typicalWidth * 0.65);
        // A word boundary remains distinguishable even at high tracking values, but it receives
        // one explicit gap—not tracking on both sides of a FIGfont's tiny SPACE glyph.
        return Math.Max(letterSpacing + 2, Math.Max(2, Math.Max(heightGap, widthGap)));
    }

    private static bool ContainsWhitespaceElement(string source)
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(source.Normalize(NormalizationForm.FormC));
        while (enumerator.MoveNext())
            if (IsWhitespaceElement(enumerator.GetTextElement()))
                return true;
        return false;
    }

    private static bool IsWhitespaceElement(string element)
    {
        bool foundRune = false;
        foreach (Rune rune in element.EnumerateRunes())
        {
            foundRune = true;
            if (!Rune.IsWhiteSpace(rune)) return false;
        }
        return foundRune;
    }

    private static FigCharacterBlock RenderFigCharacterBlock(FiggleFont font, string fontName, string textElement)
    {
        string prepared = PrepareInput(font, fontName, textElement);
        string rendered = RenderPrepared(font, prepared).Replace("\r", string.Empty);
        string[] rows = rendered.Split('\n');

        // font.Render commonly terminates with a newline. Remove only those synthetic final
        // empty rows. Keep each FIGcharacter's horizontal metrics intact: tracking is extra
        // blank columns BETWEEN complete rendered blocks, never a rewrite of their columns.
        int rowCount = rows.Length;
        while (rowCount > 1 && rows[rowCount - 1].Length == 0)
            rowCount--;
        if (rowCount != rows.Length)
            rows = rows.Take(rowCount).ToArray();

        int width = Math.Max(1, rows.Length == 0 ? 1 : rows.Max(row => row.Length));
        return new FigCharacterBlock(rows, width);
    }

    private readonly record struct FigCharacterBlock(string[] Rows, int Width);

    /// <summary>
    /// Renders a generated FIGlet prefab as geometry, then fills every occupied cell with
    /// glyphs from the selected character set. FIGlet's own implementation characters never
    /// leak into the visible title.
    /// </summary>
    public static string GenerateWithCharset(string? text, string? prefab, string? charset, int letterSpacing = 0)
    {
        string geometry = Generate(text, prefab, letterSpacing);
        var ramp = new List<Rune>();
        foreach (Rune rune in (charset ?? string.Empty).EnumerateRunes())
            if (!Rune.IsWhiteSpace(rune))
                ramp.Add(rune);

        if (ramp.Count == 0)
        {
            var blank = new StringBuilder(geometry.Length);
            foreach (Rune rune in geometry.EnumerateRunes())
                blank.Append(rune.Value == '\n' ? '\n' : ' ');
            return blank.ToString();
        }

        var sb = new StringBuilder(geometry.Length);
        int row = 0;
        int column = 0;
        foreach (Rune rune in geometry.EnumerateRunes())
        {
            if (rune.Value == '\n')
            {
                sb.Append('\n');
                row++;
                column = 0;
                continue;
            }

            if (Rune.IsWhiteSpace(rune))
            {
                sb.Append(' ');
                column++;
                continue;
            }

            // Stable spatial mapping gives multi-glyph ramps some texture while preserving the
            // hard invariant that every visible glyph belongs to the selected character set.
            int hash = unchecked((rune.Value * 397) ^ (column * 31) ^ (row * 131));
            int index = (hash & int.MaxValue) % ramp.Count;
            sb.Append(ramp[index].ToString());
            column++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Produces a conservative Discord payload: fenced monospace text plus an ASCII-only
    /// fallback for box/block glyphs that Discord fonts may not contain consistently.
    /// </summary>
    public static string ToDiscordSafeAscii(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (Rune rune in text.Replace("\r", string.Empty).EnumerateRunes())
        {
            int v = rune.Value;
            if (v == '\n') { sb.Append('\n'); continue; }
            if (v == '\t') { sb.Append("    "); continue; }
            if (v >= 32 && v <= 126) { sb.Append((char)v); continue; }

            string replacement = v switch
            {
                // Blocks and shades
                >= 0x2580 and <= 0x259F => "#",
                // Box drawing: horizontal, vertical, corners/junctions
                >= 0x2500 and <= 0x257F => BoxDrawingFallback(v),
                // Common arrows/decorative glyphs
                0x00B7 or 0x2022 or 0x2219 => ".",
                _ => FoldRuneToAscii(rune)
            };
            sb.Append(replacement);
        }
        return sb.ToString();
    }

    private static string BoxDrawingFallback(int value)
    {
        // Horizontal-only families.
        if (value is 0x2500 or 0x2501 or 0x254C or 0x254D or 0x2550 or 0x2574 or 0x2576) return "-";
        // Vertical-only families.
        if (value is 0x2502 or 0x2503 or 0x254E or 0x254F or 0x2551 or 0x2575 or 0x2577) return "|";
        return "+";
    }

    private static IReadOnlyDictionary<string, FiggleFont> BuildFontMap()
    {
        var map = new Dictionary<string, FiggleFont>(StringComparer.OrdinalIgnoreCase);

        // Figgle 0.6.6 keeps the mega-font bundle in the separate Figgle.Fonts assembly.
        // Do not bind to the generated FiggleFonts type at compile time: its generated
        // namespace/type surface can vary between package builds. Discover it from the
        // referenced assembly instead, while still strongly typing the actual FiggleFont
        // instances returned by the bundle.
        Assembly fontsAssembly = LoadFiggleFontsAssembly();
        Type? type = GetLoadableTypes(fontsAssembly)
            .FirstOrDefault(t => t.Name.Equals("FiggleFonts", StringComparison.Ordinal));

        if (type is null)
            throw new InvalidOperationException(
                "Figgle.Fonts is installed, but its FiggleFonts catalog type could not be found.");

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (!typeof(FiggleFont).IsAssignableFrom(property.PropertyType)) continue;
            try
            {
                if (property.GetValue(null) is FiggleFont font)
                    map[PrettyName(property.Name)] = font;
            }
            catch
            {
                // One broken font accessor must not make the complete prefab catalog unusable.
            }
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (!typeof(FiggleFont).IsAssignableFrom(field.FieldType)) continue;
            try
            {
                if (field.GetValue(null) is FiggleFont font)
                    map.TryAdd(PrettyName(field.Name), font);
            }
            catch
            {
                // Same isolation policy as property accessors above.
            }
        }

        if (map.Count == 0)
            throw new InvalidOperationException("Figgle.Fonts did not expose any usable FIGlet fonts.");

        return map;
    }

    private static Assembly LoadFiggleFontsAssembly()
    {
        Assembly? alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, "Figgle.Fonts", StringComparison.OrdinalIgnoreCase));

        if (alreadyLoaded is not null) return alreadyLoaded;

        try
        {
            return Assembly.Load(new AssemblyName("Figgle.Fonts"));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "The Figgle.Fonts 0.6.6 assembly could not be loaded. Restore NuGet packages and rebuild Glyphoré.", ex);
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }

    private static FiggleFont ResolveFont(string? prefab, out string resolvedName)
    {
        string requested = string.IsNullOrWhiteSpace(prefab) ? "Standard" : prefab.Trim();
        if (requested.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            requested = requested[Prefix.Length..].Trim();
        if (LegacyAliases.TryGetValue(prefab ?? string.Empty, out string? alias)) requested = alias;

        var map = FontMap.Value;
        if (map.TryGetValue(requested, out FiggleFont? exact))
        {
            resolvedName = requested;
            return exact;
        }

        // Accept compact member-style names too (ANSIShadow -> ANSI Shadow).
        string compact = Compact(requested);
        foreach (var pair in map)
        {
            if (!Compact(pair.Key).Equals(compact, StringComparison.OrdinalIgnoreCase)) continue;
            resolvedName = pair.Key;
            return pair.Value;
        }

        if (map.TryGetValue("Standard", out FiggleFont? standard))
        {
            resolvedName = "Standard";
            return standard;
        }

        KeyValuePair<string, FiggleFont> fallback = map
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .First();
        resolvedName = fallback.Key;
        return fallback.Value;
    }

    private static string PrepareInput(FiggleFont font, string fontName, string source)
    {
        if (source.Length == 0) return " ";
        var sb = new StringBuilder(source.Length);
        foreach (Rune rune in source.EnumerateRunes())
        {
            if (rune.Value is >= 32 and <= 126)
            {
                sb.Append(rune.ToString());
                continue;
            }

            if (CanRenderRune(font, fontName, rune)) sb.Append(rune.ToString());
            else sb.Append(FoldRuneToAscii(rune));
        }
        return sb.Length == 0 ? " " : sb.ToString();
    }

    private static bool CanRenderRune(FiggleFont font, string fontName, Rune rune)
    {
        var key = (fontName, rune.Value);
        lock (RuneSupportLock)
            if (RuneSupportCache.TryGetValue(key, out bool cached)) return cached;

        bool supported;
        try
        {
            string candidate = font.Render(rune.ToString()).Replace("\r", string.Empty).TrimEnd();
            string question = font.Render("?").Replace("\r", string.Empty).TrimEnd();
            supported = !string.IsNullOrWhiteSpace(candidate) && !candidate.Equals(question, StringComparison.Ordinal);
        }
        catch { supported = false; }

        lock (RuneSupportLock) RuneSupportCache[key] = supported;
        return supported;
    }

    private static string FoldToPortableAscii(string source)
    {
        var sb = new StringBuilder(source.Length);
        foreach (Rune rune in source.EnumerateRunes())
            sb.Append(rune.Value is >= 32 and <= 126 ? rune.ToString() : FoldRuneToAscii(rune));
        return sb.ToString();
    }

    private static string FoldRuneToAscii(Rune rune)
    {
        return rune.Value switch
        {
            0x00A1 => "!",
            0x00BF => "?",
            0x00DF => "ss",
            0x00C6 => "AE",
            0x00E6 => "ae",
            0x0152 => "OE",
            0x0153 => "oe",
            0x00D8 => "O",
            0x00F8 => "o",
            0x0141 => "L",
            0x0142 => "l",
            _ => FoldDiacriticRune(rune)
        };
    }

    private static string FoldDiacriticRune(Rune rune)
    {
        string decomposed = rune.ToString().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (c is >= ' ' and <= '~') sb.Append(c);
        }
        return sb.Length == 0 ? "_" : sb.ToString();
    }

    private static string PrettyName(string name)
    {
        name = name.Replace('_', ' ');
        var sb = new StringBuilder(name.Length + 8);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    private static string Compact(string value)
        => new(value.Where(char.IsLetterOrDigit).ToArray());

    private static string TrimOuterBlankLines(IEnumerable<string> rows)
    {
        var list = rows.Select(r => r.TrimEnd()).ToList();
        while (list.Count > 0 && string.IsNullOrWhiteSpace(list[0])) list.RemoveAt(0);
        while (list.Count > 0 && string.IsNullOrWhiteSpace(list[^1])) list.RemoveAt(list.Count - 1);
        return list.Count == 0 ? " " : string.Join(Environment.NewLine, list);
    }
}
