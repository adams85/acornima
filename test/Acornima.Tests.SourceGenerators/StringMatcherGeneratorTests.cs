using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acornima.SourceGenerators;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Acornima.Tests.SourceGenerators;

public class StringMatcherGeneratorTests : SourceGeneratorTest
{
    [Fact]
    public Task StringMatchingGeneration()
    {
        var sourceFiles = new[]
        {
            ToSourcePath("StringMatcherAttribute.cs"),
            ToSourcePath("TokenType.cs"),
            ToSourcePath("Tokenizer.Helpers.cs"),
            ToSourcePath("Parser.Helpers.cs"),
            ToSourcePath("Ast/AssignmentExpression.cs"),
            ToSourcePath("Ast/LogicalExpression.cs"),
            ToSourcePath("Ast/NonLogicalBinaryExpression.cs"),
            ToSourcePath("Ast/NonUpdateUnaryExpression.cs"),
            ToSourcePath("Ast/UpdateExpression.cs")
        }
        .Select(File.ReadAllText)
        .ToArray();

        var references =
            new[]
            {
                typeof(object).Assembly,
            }
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToArray();

        return VerifyHelper.Verify<StringMatcherGenerator>(sourceFiles, references);
    }
}
