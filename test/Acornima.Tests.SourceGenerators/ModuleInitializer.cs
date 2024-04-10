using System.Runtime.CompilerServices;
using VerifyTests;

namespace Acornima.Tests.SourceGenerators;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
