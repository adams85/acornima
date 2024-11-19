using System.IO;

namespace Acornima.Tests.SourceGenerators;

public class SourceGeneratorTest
{
    protected const string MainProject = "Acornima";
    protected const string ExtrasProject = "Acornima.Extras";

    protected static string ToSourcePath(string path, string project = MainProject)
    {
        return Path.Combine("..", "..", "..", "..", "..", "src", project, path);
    }
}
