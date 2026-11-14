using System;
using System.IO;
using System.Threading.Tasks;
using Acornima.Jsx;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace Acornima.Tests.NativeAOT;

public class NativeAotTests
{
    [Test]
    [Arguments("zod-4.6.7.module.js")]
    [Arguments("full-syntax.module.jsx")]
    public async Task ParsingAndConversionsWork(string sourceFileName)
    {
        var sourceFile = Path.Combine(AppContext.BaseDirectory, "Data", "zod-4.6.7.module.js");
        var isJsx = ".jsx".Equals(Path.GetExtension(sourceFile), StringComparison.OrdinalIgnoreCase);

        var source = File.ReadAllText(sourceFile);

        IParser parser = isJsx ? new JsxParser() : new Parser();
        var ast = parser.ParseModule(source, sourceFile);

        var (generatedSourceFormatted, generatedSourceUnformatted) = isJsx
            ? (ast.ToJsx(format: true), ast.ToJsx(format: false))
            : (ast.ToJavaScript(format: true), ast.ToJavaScript(format: false));

        var reparsedAst1 = parser.ParseModule(generatedSourceFormatted);
        var reparsedAst2 = parser.ParseModule(generatedSourceUnformatted);

        var astToJsonOptions = isJsx ? JsxAstToJsonOptions.Default : AstToJsonOptions.Default;

        var json1 = reparsedAst1.ToJson(astToJsonOptions);
        var json2 = reparsedAst2.ToJson(astToJsonOptions);

        await Assert.That(json1).EqualTo(json2);
    }
}
