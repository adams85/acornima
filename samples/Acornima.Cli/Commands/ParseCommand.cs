using System;
using Acornima.Ast;
using Acornima.Cli.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Commands;

internal enum JavaScriptCodeType
{
    Script,
    Module,
    ExpressionInScript,
    ExpressionInModule,
}

[Command(CommandName, Description = "Parse JS code and print resulting AST in JSON format.")]
internal sealed class ParseCommand
{
    public const string CommandName = "parse";

    private readonly IConsole _console;

    public ParseCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JS code to parse.")]
    public JavaScriptCodeType CodeType { get; set; }

    //[Option("--jsx", Description = "Allow JSX expressions.")]
    //public bool AllowJsx { get; set; }

    [Option("--skip-regexp", Description = "Skip parsing of regular expressions.")]
    public bool SkipRegExp { get; set; }

    [Option("-t|--tolerant", Description = "Tolerate noncritical syntax errors.")]
    public bool Tolerant { get; set; }

    [Option("-s|--simple", Description = "Print a simple overview of the AST.")]
    public bool Simple { get; set; }

    [Option("-l|--linecol", Description = "Include line and column location information.")]
    public bool IncludeLineColumn { get; set; }

    [Option("-r|--range", Description = "Include range location information.")]
    public bool IncludeRange { get; set; }

    // TODO: more options

    [Argument(0, Description = "The JS code to parse. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    public int OnExecute()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var code = Code ?? _console.ReadString();

        var parserOptions = new ParserOptions
        {
            RegExpParseMode = SkipRegExp ? RegExpParseMode.Skip : RegExpParseMode.Validate,
            Tolerant = Tolerant
        };

        var parser = new Parser();

        Node rootNode = CodeType switch
        {
            JavaScriptCodeType.Script => parser.ParseScript(code),
            JavaScriptCodeType.Module => parser.ParseModule(code),
            JavaScriptCodeType.ExpressionInScript => parser.ParseExpression(code, sourceType: SourceType.Script),
            JavaScriptCodeType.ExpressionInModule => parser.ParseExpression(code, sourceType: SourceType.Module),
            _ => throw new InvalidOperationException()
        };

        if (Simple)
        {
            var treePrinter = new TreePrinter(_console);
            treePrinter.Print(new[] { rootNode },
                node => node.ChildNodes,
                node =>
                {
                    var nodeType = node.Type.ToString();
                    var nodeClrType = node.GetType().Name;
                    return nodeType == nodeClrType ? nodeType : $"{nodeType} ({nodeClrType})";
                });
        }
        else
        {
            var astToJsonOptions = new AstToJsonOptions
            {
                IncludeLineColumn = IncludeLineColumn,
                IncludeRange = IncludeRange,
            };

            _console.WriteLine(rootNode.ToJsonString(astToJsonOptions, indent: "  "));
        }

        return 0;
    }
}
