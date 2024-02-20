using System;
using Test262Harness;

#pragma warning disable CA1822 // Mark members as static

namespace Acornima.Tests.Test262;

public abstract partial class Test262Test
{
    public static bool TestsExperimentalFeature(Test262File file)
    {
        return file.Features.ContainsAny(new[] { "decorators", "regexp-duplicate-named-groups", "import-attributes" });
    }

    private Parser BuildTestExecutor(Test262File file)
    {
        var options = new ParserOptions()
        {
            Tolerant = false,
            EcmaVersion = TestsExperimentalFeature(file) ? EcmaVersion.Experimental : EcmaVersion.Latest,
        };
        return new Parser(options);
    }

    private static void ExecuteTest(Parser parser, Test262File file)
    {
        if (file.Type == ProgramType.Script)
        {
            parser.ParseScript(file.Program, file.FileName, file.Strict);
        }
        else
        {
            parser.ParseModule(file.Program, file.FileName);
        }
    }

    private partial bool ShouldThrow(Test262File testCase, bool strict)
    {
        return testCase.NegativeTestCase is { Phase: TestingPhase.Parse };
    }
}
