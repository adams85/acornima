using Acornima.Helpers;

namespace Acornima.Ast;

using static ExceptionHelper;

public enum ImportPhase
{
    None,
    Source,
    Defer
}

internal static class ImportPhaseExtensions
{
    public static string GetImportPhaseToken(this ImportPhase phase)
    {
        return phase switch
        {
            ImportPhase.Source => "source",
            ImportPhase.Defer => "defer",
            _ => ThrowArgumentOutOfRangeException<string>(nameof(phase), phase.ToString(), null)
        };
    }
}
