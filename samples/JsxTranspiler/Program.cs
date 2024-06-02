using System;
using Acornima;
using Acornima.Cli.Helpers;
using Acornima.Jsx;
using McMaster.Extensions.CommandLineUtils;

namespace JsxTranspiler;

internal enum JavaScriptCodeType
{
    Script,
    Module,
}

[Command("jsxt", Description = "A command line tool for transpiling JSX templates into executable JavaScript code.")]
[HelpOption]
internal sealed class Program
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

    private readonly IConsole _console;

    public Program(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JSX code to parse.")]
    public JavaScriptCodeType CodeType { get; set; }

    [Option("--no-namespaces", Description = "Disallow namespaces in JSX element names.")]
    public bool DisallowNamespaces { get; set; }

    [Option("--namespaced-objects", Description = "Allow namespaces in element names containing member expressions. If `--no-namespaces` is specified, this option has no effect.")]
    public bool AllowNamespacedObjects { get; set; }

    [Argument(0, Description = "The JSX code to transpile. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    public int OnExecute(CommandLineApplication app)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var code = Code ?? _console.ReadString();

        var parser = new JsxParser(new JsxParserOptions
        {
            JsxAllowNamespaces = !DisallowNamespaces,
            JsxAllowNamespacedObjects = AllowNamespacedObjects
        });

        Acornima.Ast.Program root = CodeType switch
        {
            JavaScriptCodeType.Script => parser.ParseScript(code),
            JavaScriptCodeType.Module => parser.ParseModule(code),
            _ => throw new NotImplementedException()
        };

        var transpiler = new JsxToJavaScriptTranspiler();

        var program = transpiler.Transpile(root);

        program.WriteJavaScript(_console.Out, new KnRJavaScriptTextFormatterOptions { KeepSingleStatementBodyInLine = true });

        return 0;
    }
}
