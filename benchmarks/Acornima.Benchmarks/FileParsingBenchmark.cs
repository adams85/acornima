using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Acornima.Benchmark;

[MemoryDiagnoser]
public class FileParsingBenchmark
{
    private static readonly Dictionary<string, string> s_files = new()
    {
        { "underscore-1.5.2", null! },
        { "backbone-1.1.0", null! },
        { "mootools-1.4.5", null! },
        { "jquery-1.9.1", null! },
        { "yui-3.12.0", null! },
        { "jquery.mobile-1.4.2", null! },
        { "angular-1.2.5", null! }
    };

    private Parser _acornimaParser = null!;
    private Esprima.JavaScriptParser _esprimaParser = null!;

    [GlobalSetup]
    public void Setup()
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;

        foreach (var fileName in s_files.Keys.ToList())
        {
            s_files[fileName] = File.ReadAllText($"3rdparty/{fileName}.js");
        }

        _acornimaParser = new(new ParserOptions { RegExpParseMode = RegExpParseMode.Validate, Tolerant = true });
        _esprimaParser = new(new Esprima.ParserOptions { RegExpParseMode = Esprima.RegExpParseMode.Validate, Tolerant = true });
    }

    [ParamsSource(nameof(FileNames))]
    public string FileName { get; set; } = null!;

    public static IEnumerable<string> FileNames()
    {
        foreach (var entry in s_files)
        {
            yield return entry.Key;
        }
    }

    [Benchmark]
    public void AcornimaParse()
    {
        _acornimaParser.ParseScript(s_files[FileName]);
    }

    [Benchmark]
    public void EsprimaParse()
    {
        _esprimaParser.ParseScript(s_files[FileName]);
    }
}
