using System;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Helpers;

internal static class ConsoleExtensions
{
    public static string ReadString(this IConsole console)
    {
        if (console.IsInputRedirected)
        {
            return console.In.ReadToEnd();
        }

        bool isWindowsOS;
#if NET462
        isWindowsOS = true;
#else
        isWindowsOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

        console.WriteLine(isWindowsOS
                ? "Input config JSON, press CTRL-Z in an empty line and finally ENTER."
                : "Input config JSON, then press Ctrl+D.");
        console.WriteLine();

        if (isWindowsOS)
        {
            void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                console.CancelKeyPress -= Console_CancelKeyPress;
            }
            console.CancelKeyPress += Console_CancelKeyPress;
        }

        var result = console.In.ReadToEnd();

        console.WriteLine();

        return result;
    }
}
