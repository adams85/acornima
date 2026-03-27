using System.Text;

// Unicode Property Data Generator for Acornima
// Parses Unicode 17.0.0 UCD files and generates C# source with static code point range data
// for Script, Script_Extensions, and Binary properties.

var dataDir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory)!, "..", "..", "..", "unicode-data");
if (!Directory.Exists(dataDir))
{
    dataDir = Path.Combine(Directory.GetCurrentDirectory(), "unicode-data");
}
dataDir = Path.GetFullPath(dataDir);

Console.WriteLine($"Reading Unicode data from: {dataDir}");

// 1. Parse Scripts.txt → per-script ranges
var scriptRanges = ParsePropertyFile(Path.Combine(dataDir, "Scripts.txt"));
Console.WriteLine($"Scripts: {scriptRanges.Count} scripts parsed");

// Compute "Unknown" script = all code points NOT assigned to any explicit script
// Scripts.txt says: @missing: 0000..10FFFF; Unknown
var allScriptRanges = new List<(int Start, int End)>();
foreach (var ranges in scriptRanges.Values)
{
    allScriptRanges.AddRange(ranges);
}
allScriptRanges = NormalizeRanges(allScriptRanges);
// Invert to get Unknown
var unknownRanges = InvertRanges(allScriptRanges, 0, 0x10FFFF);
scriptRanges["Unknown"] = unknownRanges;
Console.WriteLine($"Unknown script: {unknownRanges.Count} ranges (complement of all other scripts)");

// 2. Parse ScriptExtensions.txt and build per-script extension ranges
// Script_Extensions for a script = Script ranges + ScriptExtensions ranges
var scriptExtRaw = ParseScriptExtensionsFile(Path.Combine(dataDir, "ScriptExtensions.txt"));

// 3. Parse PropertyValueAliases.txt for script abbreviation→long name mapping
var scriptAliases = ParseScriptAliases(Path.Combine(dataDir, "PropertyValueAliases.txt"));

// Build Script_Extensions ranges: start with Script ranges, then add ScriptExtensions entries
var scriptExtRanges = new Dictionary<string, List<(int Start, int End)>>();
foreach (var (script, ranges) in scriptRanges)
{
    scriptExtRanges[script] = new List<(int, int)>(ranges);
}

// Map short names to long names for ScriptExtensions entries
foreach (var (shortName, codePoints) in scriptExtRaw)
{
    var longName = scriptAliases.TryGetValue(shortName, out var ln) ? ln : shortName;
    if (!scriptExtRanges.TryGetValue(longName, out var list))
    {
        list = new List<(int, int)>();
        scriptExtRanges[longName] = list;
    }
    list.AddRange(codePoints);
}

// Normalize (sort and merge) all script extension ranges
foreach (var key in scriptExtRanges.Keys.ToList())
{
    scriptExtRanges[key] = NormalizeRanges(scriptExtRanges[key]);
}

Console.WriteLine($"Script_Extensions: {scriptExtRanges.Count} scripts with extensions");

// 4. Parse binary property files
var binaryRanges = new Dictionary<string, List<(int Start, int End)>>();
MergeBinaryProperties(binaryRanges, ParsePropertyFile(Path.Combine(dataDir, "PropList.txt")));
MergeBinaryProperties(binaryRanges, ParsePropertyFile(Path.Combine(dataDir, "DerivedCoreProperties.txt")));
MergeBinaryProperties(binaryRanges, ParsePropertyFile(Path.Combine(dataDir, "emoji-data.txt")));
MergeBinaryProperties(binaryRanges, ParsePropertyFile(Path.Combine(dataDir, "DerivedBinaryProperties.txt")));
MergeBinaryProperties(binaryRanges, ParsePropertyFile(Path.Combine(dataDir, "DerivedNormalizationProps.txt")));
Console.WriteLine($"Binary properties: {binaryRanges.Count} properties parsed");

// Normalize binary property ranges
foreach (var key in binaryRanges.Keys.ToList())
{
    binaryRanges[key] = NormalizeRanges(binaryRanges[key]);
}

// 5. Get the list of binary properties that ECMAScript recognizes (from UnicodeProperties.cs)
var ecmaBinaryProps = new HashSet<string>
{
    "ASCII", "ASCII_Hex_Digit", "Alphabetic", "Any", "Assigned",
    "Bidi_Control", "Bidi_Mirrored", "Case_Ignorable", "Cased",
    "Changes_When_Casefolded", "Changes_When_Casemapped", "Changes_When_Lowercased",
    "Changes_When_NFKC_Casefolded", "Changes_When_Titlecased", "Changes_When_Uppercased",
    "Dash", "Default_Ignorable_Code_Point", "Deprecated", "Diacritic",
    "Emoji", "Emoji_Component", "Emoji_Modifier", "Emoji_Modifier_Base", "Emoji_Presentation",
    "Extended_Pictographic",
    "Extender", "Grapheme_Base", "Grapheme_Extend", "Hex_Digit",
    "IDS_Binary_Operator", "IDS_Trinary_Operator", "ID_Continue", "ID_Start",
    "Ideographic", "Join_Control", "Logical_Order_Exception", "Lowercase", "Math",
    "Noncharacter_Code_Point", "Pattern_Syntax", "Pattern_White_Space",
    "Quotation_Mark", "Radical", "Regional_Indicator", "Sentence_Terminal",
    "Soft_Dotted", "Terminal_Punctuation", "Unified_Ideograph", "Uppercase",
    "Variation_Selector", "White_Space", "XID_Continue", "XID_Start"
};

// Filter binary properties to only ECMAScript-recognized ones
var filteredBinary = new Dictionary<string, List<(int Start, int End)>>();
foreach (var prop in ecmaBinaryProps)
{
    if (prop is "Any" or "Assigned" or "ASCII")
    {
        // These are special-cased at runtime, don't need static data
        continue;
    }

    if (binaryRanges.TryGetValue(prop, out var ranges))
    {
        filteredBinary[prop] = ranges;
    }
    else
    {
        Console.WriteLine($"WARNING: Binary property '{prop}' not found in UCD data");
    }
}

Console.WriteLine($"Filtered binary properties for output: {filteredBinary.Count}");

// 6. Count total ranges for size estimate
var totalScriptRanges = scriptRanges.Values.Sum(r => r.Count);
var totalScriptExtRanges = scriptExtRanges.Values.Sum(r => r.Count);
var totalBinaryRanges = filteredBinary.Values.Sum(r => r.Count);
Console.WriteLine($"Total ranges - Script: {totalScriptRanges}, ScriptExt: {totalScriptExtRanges}, Binary: {totalBinaryRanges}");
Console.WriteLine($"Estimated raw size: {(totalScriptRanges + totalScriptExtRanges + totalBinaryRanges) * 8 / 1024} KB");

// 7. Generate C# source
// Navigate from tools/UnicodeDataGenerator/unicode-data → src/Acornima/
var outputPath = Path.GetFullPath(Path.Combine(dataDir, "..", "..", "..", "src", "Acornima", "UnicodeProperties.Generated.cs"));
GenerateCSharp(outputPath, scriptRanges, scriptExtRanges, filteredBinary, scriptAliases);
Console.WriteLine($"Generated: {outputPath}");

// === Helper methods ===

static Dictionary<string, List<(int Start, int End)>> ParsePropertyFile(string path)
{
    var result = new Dictionary<string, List<(int Start, int End)>>();

    foreach (var line in File.ReadLines(path))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var commentIndex = line.IndexOf('#');
        var data = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
        var parts = data.Split(';');
        if (parts.Length < 2)
            continue;

        var rangePart = parts[0].Trim();
        var propName = parts[1].Trim();

        var (start, end) = ParseRange(rangePart);

        if (!result.TryGetValue(propName, out var list))
        {
            list = new List<(int, int)>();
            result[propName] = list;
        }
        list.Add((start, end));
    }

    // Normalize all ranges
    foreach (var key in result.Keys.ToList())
    {
        result[key] = NormalizeRanges(result[key]);
    }

    return result;
}

static Dictionary<string, List<(int Start, int End)>> ParseScriptExtensionsFile(string path)
{
    // ScriptExtensions.txt has format: codepoint(s) ; Script1 Script2 ... # comment
    // We need to map each code point to all listed scripts
    var result = new Dictionary<string, List<(int Start, int End)>>();

    foreach (var line in File.ReadLines(path))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var commentIndex = line.IndexOf('#');
        var data = commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
        var parts = data.Split(';');
        if (parts.Length < 2)
            continue;

        var rangePart = parts[0].Trim();
        var scripts = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var (start, end) = ParseRange(rangePart);

        foreach (var script in scripts)
        {
            if (!result.TryGetValue(script, out var list))
            {
                list = new List<(int, int)>();
                result[script] = list;
            }
            list.Add((start, end));
        }
    }

    return result;
}

static Dictionary<string, string> ParseScriptAliases(string path)
{
    // Format: sc ; Adlm ; Adlam [; additional_alias]
    var result = new Dictionary<string, string>();

    foreach (var line in File.ReadLines(path))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var parts = line.Split(';');
        if (parts.Length < 3 || parts[0].Trim() != "sc")
            continue;

        var shortName = parts[1].Trim();
        var longName = parts[2].Trim();
        result[shortName] = longName;

        // Handle additional aliases (e.g., Qaac for Coptic, Qaai for Inherited)
        for (var i = 3; i < parts.Length; i++)
        {
            var alias = parts[i].Trim();
            if (!string.IsNullOrEmpty(alias))
            {
                result[alias] = longName;
            }
        }
    }

    return result;
}

static List<(int Start, int End)> InvertRanges(List<(int Start, int End)> ranges, int start, int end)
{
    var result = new List<(int, int)>();
    if (ranges.Count == 0)
    {
        result.Add((start, end));
        return result;
    }

    if (ranges[0].Start > start)
    {
        result.Add((start, ranges[0].Start - 1));
    }

    for (var i = 1; i < ranges.Count; i++)
    {
        result.Add((ranges[i - 1].End + 1, ranges[i].Start - 1));
    }

    if (ranges[^1].End < end)
    {
        result.Add((ranges[^1].End + 1, end));
    }

    return result;
}

static (int Start, int End) ParseRange(string rangePart)
{
    var dotDot = rangePart.IndexOf("..", StringComparison.Ordinal);
    if (dotDot >= 0)
    {
        var start = Convert.ToInt32(rangePart.Substring(0, dotDot), 16);
        var end = Convert.ToInt32(rangePart.Substring(dotDot + 2), 16);
        return (start, end);
    }
    else
    {
        var cp = Convert.ToInt32(rangePart, 16);
        return (cp, cp);
    }
}

static List<(int Start, int End)> NormalizeRanges(List<(int Start, int End)> ranges)
{
    if (ranges.Count <= 1)
        return ranges;

    ranges.Sort((a, b) => a.Start.CompareTo(b.Start));

    var result = new List<(int, int)>();
    var current = ranges[0];

    for (var i = 1; i < ranges.Count; i++)
    {
        if (ranges[i].Start <= current.End + 1)
        {
            current = (current.Start, Math.Max(current.End, ranges[i].End));
        }
        else
        {
            result.Add(current);
            current = ranges[i];
        }
    }
    result.Add(current);

    return result;
}

static void MergeBinaryProperties(Dictionary<string, List<(int Start, int End)>> target,
    Dictionary<string, List<(int Start, int End)>> source)
{
    foreach (var (prop, ranges) in source)
    {
        if (!target.TryGetValue(prop, out var list))
        {
            list = new List<(int, int)>();
            target[prop] = list;
        }
        list.AddRange(ranges);
    }
}

static void GenerateCSharp(string outputPath,
    Dictionary<string, List<(int Start, int End)>> scriptRanges,
    Dictionary<string, List<(int Start, int End)>> scriptExtRanges,
    Dictionary<string, List<(int Start, int End)>> binaryRanges,
    Dictionary<string, string> scriptAliases)
{
    var sb = new StringBuilder();
    sb.AppendLine("// <auto-generated>");
    sb.AppendLine("// Generated by UnicodeDataGenerator from Unicode 17.0.0 data files.");
    sb.AppendLine("// Do not edit manually.");
    sb.AppendLine("// </auto-generated>");
    sb.AppendLine();
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using Acornima.Helpers;");
    sb.AppendLine();
    sb.AppendLine("namespace Acornima;");
    sb.AppendLine();
    sb.AppendLine("internal static class UnicodePropertyData");
    sb.AppendLine("{");

    // Collect all ranges into a single flat array, tracking offsets
    var allRanges = new List<int>(); // pairs of (start, end)
    // Using KeyValuePair<int, int> instead of tuples for net462 compatibility
    var scriptIndex = new Dictionary<string, KeyValuePair<int, int>>();
    var scriptExtIndex = new Dictionary<string, KeyValuePair<int, int>>();
    var binaryIndex = new Dictionary<string, KeyValuePair<int, int>>();

    // Scripts
    foreach (var (name, ranges) in scriptRanges.OrderBy(kv => kv.Key))
    {
        var offset = allRanges.Count / 2;
        foreach (var (start, end) in ranges)
        {
            allRanges.Add(start);
            allRanges.Add(end);
        }
        scriptIndex[name] = new KeyValuePair<int, int>(offset, ranges.Count);
    }

    // Script Extensions
    foreach (var (name, ranges) in scriptExtRanges.OrderBy(kv => kv.Key))
    {
        var offset = allRanges.Count / 2;
        foreach (var (start, end) in ranges)
        {
            allRanges.Add(start);
            allRanges.Add(end);
        }
        scriptExtIndex[name] = new KeyValuePair<int, int>(offset, ranges.Count);
    }

    // Binary properties
    foreach (var (name, ranges) in binaryRanges.OrderBy(kv => kv.Key))
    {
        var offset = allRanges.Count / 2;
        foreach (var (start, end) in ranges)
        {
            allRanges.Add(start);
            allRanges.Add(end);
        }
        binaryIndex[name] = new KeyValuePair<int, int>(offset, ranges.Count);
    }

    // Write the main data array
    sb.AppendLine("    /// <summary>");
    sb.AppendLine("    /// All code point ranges stored as pairs of (start, end) values.");
    sb.AppendLine("    /// Individual properties reference slices of this array via offset and count.");
    sb.AppendLine("    /// </summary>");
    sb.AppendLine("    internal static ReadOnlySpan<int> AllRanges => new int[]");
    sb.AppendLine("    {");

    const int valuesPerLine = 16;
    for (var i = 0; i < allRanges.Count; i += valuesPerLine)
    {
        sb.Append("        ");
        var end = Math.Min(i + valuesPerLine, allRanges.Count);
        for (var j = i; j < end; j++)
        {
            sb.Append($"0x{allRanges[j]:X6}");
            if (j < allRanges.Count - 1) sb.Append(", ");
        }
        sb.AppendLine();
    }
    sb.AppendLine("    };");
    sb.AppendLine();

    // Write Script lookup
    WriteIndexDictionary(sb, "ScriptLookup", "Script (sc) property ranges", scriptIndex, scriptAliases);
    sb.AppendLine();

    // Write Script Extensions lookup
    WriteIndexDictionary(sb, "ScriptExtensionsLookup", "Script_Extensions (scx) property ranges", scriptExtIndex, scriptAliases);
    sb.AppendLine();

    // Write Binary property lookup (no aliases needed, use canonical names)
    WriteBinaryIndexDictionary(sb, "BinaryPropertyLookup", "Binary property ranges", binaryIndex);

    sb.AppendLine("}");

    // Ensure output directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
}

static void WriteIndexDictionary(StringBuilder sb, string name, string description,
    Dictionary<string, KeyValuePair<int, int>> index,
    Dictionary<string, string> scriptAliases)
{
    // Build reverse alias map (long name → list of short names)
    var reverseAliases = new Dictionary<string, List<string>>();
    foreach (var kv in scriptAliases)
    {
        if (!reverseAliases.TryGetValue(kv.Value, out var list))
        {
            list = new List<string>();
            reverseAliases[kv.Value] = list;
        }
        list.Add(kv.Key);
    }

    sb.AppendLine($"    /// <summary>{description}. Maps property name to KeyValuePair(offset into AllRanges, range count).</summary>");
    sb.AppendLine($"    internal static Dictionary<string, KeyValuePair<int, int>> {name} {{ get; }} = new(StringComparer.Ordinal)");
    sb.AppendLine("    {");

    foreach (var kv in index.OrderBy(x => x.Key))
    {
        sb.AppendLine($"        {{ \"{kv.Key}\", new KeyValuePair<int, int>({kv.Value.Key}, {kv.Value.Value}) }},");

        // Also add all short aliases
        if (reverseAliases.TryGetValue(kv.Key, out var aliases))
        {
            foreach (var alias in aliases)
            {
                if (alias != kv.Key)
                {
                    sb.AppendLine($"        {{ \"{alias}\", new KeyValuePair<int, int>({kv.Value.Key}, {kv.Value.Value}) }},");
                }
            }
        }
    }

    sb.AppendLine("    };");
}

static void WriteBinaryIndexDictionary(StringBuilder sb, string name, string description,
    Dictionary<string, KeyValuePair<int, int>> index)
{
    // ECMAScript uses both canonical names and short aliases for binary properties
    var aliases = new Dictionary<string, string>
    {
        { "AHex", "ASCII_Hex_Digit" },
        { "Alpha", "Alphabetic" },
        { "Bidi_C", "Bidi_Control" },
        { "Bidi_M", "Bidi_Mirrored" },
        { "CI", "Case_Ignorable" },
        { "CWCF", "Changes_When_Casefolded" },
        { "CWCM", "Changes_When_Casemapped" },
        { "CWL", "Changes_When_Lowercased" },
        { "CWKCF", "Changes_When_NFKC_Casefolded" },
        { "CWT", "Changes_When_Titlecased" },
        { "CWU", "Changes_When_Uppercased" },
        { "DI", "Default_Ignorable_Code_Point" },
        { "Dep", "Deprecated" },
        { "Dia", "Diacritic" },
        { "Ext", "Extender" },
        { "Gr_Base", "Grapheme_Base" },
        { "Gr_Ext", "Grapheme_Extend" },
        { "Hex", "Hex_Digit" },
        { "IDSB", "IDS_Binary_Operator" },
        { "IDST", "IDS_Trinary_Operator" },
        { "IDC", "ID_Continue" },
        { "IDS", "ID_Start" },
        { "Ideo", "Ideographic" },
        { "Join_C", "Join_Control" },
        { "LOE", "Logical_Order_Exception" },
        { "Lower", "Lowercase" },
        { "NChar", "Noncharacter_Code_Point" },
        { "Pat_Syn", "Pattern_Syntax" },
        { "Pat_WS", "Pattern_White_Space" },
        { "QMark", "Quotation_Mark" },
        { "RI", "Regional_Indicator" },
        { "STerm", "Sentence_Terminal" },
        { "SD", "Soft_Dotted" },
        { "Term", "Terminal_Punctuation" },
        { "UIdeo", "Unified_Ideograph" },
        { "Upper", "Uppercase" },
        { "VS", "Variation_Selector" },
        { "space", "White_Space" },
        { "XIDC", "XID_Continue" },
        { "XIDS", "XID_Start" },
        { "EBase", "Emoji_Modifier_Base" },
        { "EComp", "Emoji_Component" },
        { "EMod", "Emoji_Modifier" },
        { "EPres", "Emoji_Presentation" },
        { "ExtPict", "Extended_Pictographic" },
    };

    sb.AppendLine($"    /// <summary>{description}. Maps property name to KeyValuePair(offset into AllRanges, range count).</summary>");
    sb.AppendLine($"    internal static Dictionary<string, KeyValuePair<int, int>> {name} {{ get; }} = new(StringComparer.Ordinal)");
    sb.AppendLine("    {");

    foreach (var kv in index.OrderBy(x => x.Key))
    {
        sb.AppendLine($"        {{ \"{kv.Key}\", new KeyValuePair<int, int>({kv.Value.Key}, {kv.Value.Value}) }},");
    }

    // Add aliases
    foreach (var alias in aliases.OrderBy(x => x.Key))
    {
        if (index.TryGetValue(alias.Value, out var entry))
        {
            sb.AppendLine($"        {{ \"{alias.Key}\", new KeyValuePair<int, int>({entry.Key}, {entry.Value}) }},");
        }
    }

    sb.AppendLine("    };");
}
