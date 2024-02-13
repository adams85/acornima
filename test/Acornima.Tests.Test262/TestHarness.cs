using System.Threading.Tasks;

namespace Acornima.Tests.Test262;

/// <summary>
/// Handles initializing testing state.
/// </summary>
public partial class TestHarness
{
    private static partial Task InitializeCustomState()
    {
        return Task.CompletedTask;
    }
}
