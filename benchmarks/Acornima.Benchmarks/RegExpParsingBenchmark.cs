using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Acornima.Tests.Helpers;
using BenchmarkDotNet.Attributes;

namespace Acornima.Benchmark;

#pragma warning disable CA1861, CA1865

[MemoryDiagnoser]
public class RegExpParsingBenchmark
{
    private static IEnumerable<(string Pattern, string Flags)> GetRegExps(string testCasesFilePath)
    {
        using var reader = new StreamReader(testCasesFilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(new[] { '\t' }, StringSplitOptions.None);
            yield return (DecodeStringIfEscaped(parts[0]), DecodeStringIfEscaped(parts[1]));
        }

        static string DecodeStringIfEscaped(string value) => JavaScriptString.IsStringLiteral(value)
            ? JavaScriptString.Decode(value)
            : value;
    }

    private (string Pattern, string Flags)[] _regExps = null!;

    [GlobalSetup]
    public void Setup()
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;

        _regExps = GetRegExps(Path.Combine("Data", "testcases.txt")).ToArray();
    }

    [Benchmark]
    public void Validate()
    {
        for (int i = 0; i < _regExps.Length; i++)
        {
            ref readonly var regExp = ref _regExps[i];
            Tokenizer.ValidateRegExp(regExp.Pattern, regExp.Flags, out _);
        }
    }
}
