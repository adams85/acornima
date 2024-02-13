using System;
using System.Linq;
using Acornima.Cli.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli;

[Command("acornima", Description = "A command line tool for testing Acornima features.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption(Inherited = true)]
[Subcommand(typeof(ParseCommand), typeof(TokenizeCommand))]
public class Program
{
    public static int Main(string[] args)
    {
        var console = PhysicalConsole.Singleton;

        using var app = new CommandLineApplication<Program>(console, Environment.CurrentDirectory);
        app.Conventions.UseDefaultConventions();
        try
        {
            return app.Execute(args);
        }
        catch (ParseErrorException ex)
        {
            console.Error.WriteLine(ex.Message);
            return -1;
        }
        catch (CommandParsingException ex)
        {
            console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    public int OnExecute(CommandLineApplication app)
    {
        var args = new[] { ParseCommand.CommandName }.Concat(app.RemainingArguments).ToArray();
        return app.Execute(args);
    }
}
