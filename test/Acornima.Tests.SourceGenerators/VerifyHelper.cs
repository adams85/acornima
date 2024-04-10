using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace Acornima.Tests.SourceGenerators;

public static class VerifyHelper
{
    public static Task Verify<T>(params string[] sources) where T : class, IIncrementalGenerator, new()
    {
        return Verify<T>(sources, references: null);
    }

    public static Task Verify<T>(string[] sources, MetadataReference[]? references) where T : class, IIncrementalGenerator, new()
    {
        var syntaxTrees = sources.Select(x => CSharpSyntaxTree.ParseText(x));

        Compilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees,
            references);

        var generator = new T();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verifier
            .Verify(driver.GetRunResult())
            .UseDirectory("Snapshots");
    }
}
