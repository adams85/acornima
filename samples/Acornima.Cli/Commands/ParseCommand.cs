using System;
using Acornima.Ast;
using Acornima.Cli.Helpers;
using Acornima.Jsx;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Commands;

internal enum JavaScriptCodeType
{
    Script,
    Module,
    Expression,
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

    [Option("--jsx", Description = "Allow JSX expressions.")]
    public bool AllowJsx { get; set; }

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

    private T CreateParserOptions<T>() where T : ParserOptions, new() => new T
    {
        RegExpParseMode = SkipRegExp ? RegExpParseMode.Skip : RegExpParseMode.Validate,
        Tolerant = Tolerant,
    };

    private T CreateAstToJsonOptions<T>() where T : AstToJsonOptions, new() => new T
    {
        IncludeLineColumn = IncludeLineColumn,
        IncludeRange = IncludeRange,
    };

    public int OnExecute()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var code = Code ?? _console.ReadString();

        IParser parser = AllowJsx
            ? new JsxParser(CreateParserOptions<JsxParserOptions>())
            : new Parser(CreateParserOptions<ParserOptions>());

        Node rootNode = CodeType switch
        {
            JavaScriptCodeType.Script => parser.ParseScript(code),
            JavaScriptCodeType.Module => parser.ParseModule(code),
            JavaScriptCodeType.Expression => parser.ParseExpression(code),
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
            var astToJsonOptions = AllowJsx
                ? CreateAstToJsonOptions<JsxAstToJsonOptions>()
                : CreateAstToJsonOptions<AstToJsonOptions>();

            _console.WriteLine(rootNode.ToJson(astToJsonOptions, indent: "  "));
        }

        return 0;
    }
}
