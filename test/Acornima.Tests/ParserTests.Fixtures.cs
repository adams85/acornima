using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Acornima.Ast;
using DiffEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Acornima.Tests;

public partial class ParserTests
{
    // Do manually set it to true to update local test files with the current results.
    // Only use this when the test is deemed wrong.
    private const bool WriteBackExpectedTree = false;

    internal const string FixturesDirName = "Fixtures.Parser";

    private static JsonSerializerSettings JsonDeserializationSettings { get; } = new() { MaxDepth = 256 };

    internal static Lazy<Dictionary<string, FixtureMetadata>> Metadata { get; } = new(FixtureMetadata.ReadMetadata);

    internal static string GetFixturesPath()
    {
#if NETFRAMEWORK
        var assemblyPath = new Uri(typeof(RegExpTests).GetTypeInfo().Assembly.CodeBase).LocalPath;
        var assemblyDirectory = new FileInfo(assemblyPath).Directory;
#else
        var assemblyPath = typeof(RegExpTests).GetTypeInfo().Assembly.Location;
        var assemblyDirectory = new FileInfo(assemblyPath).Directory;
#endif
        var root = assemblyDirectory?.Parent?.Parent?.Parent?.FullName;
        return root ?? "";
    }

    public static IEnumerable<object[]> Fixtures(string relativePath)
    {
        var fixturesPath = Path.GetFullPath(Path.Combine(GetFixturesPath(), relativePath));

        var files = Directory.GetFiles(fixturesPath, "*.js", SearchOption.AllDirectories);

        return files
            .Select(x => new object[] { x.Substring(fixturesPath.Length + 1) })
            .ToList();
    }

    [Theory]
    [MemberData(nameof(Fixtures), FixturesDirName)]
    public void ExecuteTestCase(string fixture)
    {
        static T CreateParserOptions<T>(bool tolerant, RegExpParseMode regExpParseMode, EcmaVersion ecmaVersion) where T : ParserOptions, new() => new T
        {
            Tolerant = tolerant,
            RegExpParseMode = regExpParseMode,
            AllowReturnOutsideFunction = tolerant,
            EcmaVersion = ecmaVersion,
        };

        var (parserOptionsFactory, parserFactory, conversionDefaultOptions) = (new Func<bool, RegExpParseMode, EcmaVersion, ParserOptions>(CreateParserOptions<ParserOptions>),
            new Func<ParserOptions, Parser>(opts => new Parser(opts)),
            AstToJsonOptions.Default);

        string treeFilePath, failureFilePath, moduleFilePath;
        var jsFilePath = Path.Combine(GetFixturesPath(), FixturesDirName, fixture);

        if (!Metadata.Value.TryGetValue(jsFilePath, out var metadata))
        {
            metadata = FixtureMetadata.Default;
        }

        if (metadata.Skip)
        {
            return;
        }

        var jsFileDirectoryName = Path.GetDirectoryName(jsFilePath)!;
        if (jsFilePath.EndsWith(".source.js", StringComparison.Ordinal))
        {
            treeFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".tree.json";
            failureFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".failure.json";
            moduleFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(jsFilePath))) + ".module.json";
        }
        else
        {
            treeFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".tree.json";
            failureFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".failure.json";
            moduleFilePath = Path.Combine(jsFileDirectoryName, Path.GetFileNameWithoutExtension(jsFilePath)) + ".module.json";
        }

        var script = File.ReadAllText(jsFilePath);
        if (jsFilePath.EndsWith(".source.js", StringComparison.Ordinal))
        {
            var parser = new Parser();
            var program = parser.ParseScript(script);
            var source = program.Body.First().As<VariableDeclaration>().Declarations.First().As<VariableDeclarator>().Init!.As<StringLiteral>().Value;
            script = source;
        }

        var expected = "";
        var invalid = false;

        var filename = Path.GetFileNameWithoutExtension(jsFilePath);

        var isModule =
            filename.Contains("module") ||
            filename.Contains("export") ||
            filename.Contains("import");

        if (!filename.Contains(".module"))
        {
            isModule &= !jsFilePath.Contains("dynamic-import") && !jsFilePath.Contains("script");
        }

        var sourceType = isModule
            ? SourceType.Module
            : SourceType.Script;

        var regExpParseMode = !metadata.IgnoresRegex ? RegExpParseMode.AdaptToInterpreted : RegExpParseMode.Skip;
        var ecmaVersion = jsFilePath.Contains("experimental") ? EcmaVersion.Experimental : EcmaVersion.Latest;

        var parserOptions = parserOptionsFactory(false, regExpParseMode, ecmaVersion);

        var conversionOptions = metadata.CreateConversionOptions(conversionDefaultOptions);
        if (File.Exists(moduleFilePath))
        {
            sourceType = SourceType.Module;
            expected = File.ReadAllText(moduleFilePath);
            if (WriteBackExpectedTree)
            {
                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions, metadata.Minimize);
                if (!CompareTrees(actual, expected))
                    File.WriteAllText(moduleFilePath, actual);
            }
        }
        else if (File.Exists(treeFilePath))
        {
            expected = File.ReadAllText(treeFilePath);
            if (WriteBackExpectedTree)
            {
                var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions, metadata.Minimize);
                if (!CompareTrees(actual, expected))
                    File.WriteAllText(treeFilePath, actual);
            }
        }
        else if (File.Exists(failureFilePath))
        {
            invalid = true;
            expected = File.ReadAllText(failureFilePath);
            if (WriteBackExpectedTree)
            {
                try
                {
                    ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
                }
                catch (ParseErrorException ex)
                {
                    var expectedJsonObject = JObject.Parse(expected);

                    var parseError = ex.Error!;
                    var actualJsonObject = new JObject
                    {
                        ["index"] = parseError.Index,
                        ["lineNumber"] = parseError.LineNumber,
                        ["column"] = parseError.Column,
                        ["message"] = ex.Message,
                        ["description"] = parseError.Description,
                    };

                    if (!JToken.DeepEquals(expectedJsonObject, actualJsonObject))
                        File.WriteAllText(failureFilePath, actualJsonObject.ToString(Formatting.None));
                }
            }
        }
        else
        {
            // cannot compare
            return;
        }

        invalid |=
            filename.Contains("error") ||
            filename.Contains("invalid") && (!filename.Contains("invalid-yield-object-") && !filename.Contains("attribute-invalid-entity"));

        if (!invalid)
        {
            parserOptions = parserOptionsFactory(true, parserOptions.RegExpParseMode, parserOptions.EcmaVersion);

            var actual = ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions);
            CompareTreesAndAssert(actual, expected);

            actual = ParseAndFormat(sourceType, script, parserOptions with { OnToken = delegate { } }, parserFactory, conversionOptions);
            CompareTreesAndAssert(actual, expected);
        }
        else
        {
            parserOptions = parserOptionsFactory(false, parserOptions.RegExpParseMode, parserOptions.EcmaVersion);

            // TODO: check the accuracy of the message and of the location
            Assert.Throws<SyntaxErrorException>(() => ParseAndFormat(sourceType, script, parserOptions, parserFactory, conversionOptions));
        }
    }

    private static string ParseAndFormat(SourceType sourceType, string source,
        ParserOptions parserOptions, Func<ParserOptions, Parser> parserFactory,
        AstToJsonOptions conversionOptions, bool minimize = false)
    {
        var parser = parserFactory(parserOptions);
        var program = sourceType == SourceType.Script ? (Program)parser.ParseScript(source) : parser.ParseModule(source);

        var json = program.ToJsonString(conversionOptions, indent: "  ");
        if (minimize)
        {
            json = JsonConvert.DeserializeObject<JObject>(json, JsonDeserializationSettings)!.ToString(Formatting.None);
        }
        return json;
    }

    private static bool CompareTreesInternal(JObject? actualJObject, JObject? expectedJObject)
    {
        return JToken.DeepEquals(actualJObject, expectedJObject);
    }

    private static bool CompareTrees(string actual, string expected)
    {
        var actualJObject = JsonConvert.DeserializeObject<JObject>(actual, JsonDeserializationSettings);
        var expectedJObject = JsonConvert.DeserializeObject<JObject>(expected, JsonDeserializationSettings);

        return CompareTreesInternal(actualJObject, expectedJObject);
    }

    private static void CompareTreesAndAssert(string actual, string expected)
    {
        var actualJObject = JObject.Parse(actual);
        var expectedJObject = JObject.Parse(expected);

        var areEqual = CompareTreesInternal(actualJObject, expectedJObject);
        if (!areEqual)
        {
            var actualString = actualJObject.ToString();
            var expectedString = expectedJObject.ToString();

            var file1 = Path.GetTempFileName() + ".json";
            var file2 = Path.GetTempFileName() + ".json";
            File.WriteAllText(file1, expectedString);
            File.WriteAllText(file2, actualString);
            // TODO: verify
            DiffRunner.Launch(file1, file2);

            Assert.Equal(expectedString, actualString);
        }
    }

    internal sealed class FixtureMetadata
    {
        public static readonly FixtureMetadata Default = new()
        {
            IncludesLocation = true,
            IncludesRange = true,
            IgnoresRegex = false,
            Skip = false,
            Minimize = false,
        };

        private sealed class Group
        {
            public HashSet<string> Flags { get; } = new();
            public HashSet<string> Files { get; } = new();
        }

        public static Dictionary<string, FixtureMetadata> ReadMetadata()
        {
            var fixturesDirPath = Path.Combine(GetFixturesPath(), FixturesDirName);
            var compatListFilePath = Path.Combine(fixturesDirPath, "fixtures-metadata.json");

            var baseUri = new Uri(fixturesDirPath + "/");

            Group[]? groups;
            using (var reader = new StreamReader(compatListFilePath))
                groups = (Group[]?)JsonSerializer.CreateDefault().Deserialize(reader, typeof(Group[]));

            return (groups ?? Array.Empty<Group>())
                .SelectMany(group =>
                {
                    var metadata = CreateFrom(group.Flags);

                    return group.Files.Select(file =>
                    (
                        filePath: new Uri(baseUri, file).LocalPath,
                        metadata
                    ));
                })
                .ToDictionary(item => item.filePath, item => item.metadata);
        }

        private static FixtureMetadata CreateFrom(HashSet<string> flags)
        {
            return new FixtureMetadata
            {
                IncludesLocation = flags.Contains("IncludesLocation"),
                IncludesRange = flags.Contains("IncludesRange"),
                IgnoresRegex = flags.Contains("IgnoresRegex"),
                Skip = flags.Contains("Skip"),
                Minimize = flags.Contains("Minimize"),
            };
        }

        private FixtureMetadata() { }

        public bool IncludesLocation { get; init; }
        public bool IncludesRange { get; init; }
        public bool IgnoresRegex { get; init; }
        public bool Skip { get; init; }
        public bool Minimize { get; init; }

        public AstToJsonOptions CreateConversionOptions(AstToJsonOptions defaultOptions) => defaultOptions with
        {
            IncludeLineColumn = IncludesLocation,
            IncludeRange = IncludesRange,
        };
    }
}
