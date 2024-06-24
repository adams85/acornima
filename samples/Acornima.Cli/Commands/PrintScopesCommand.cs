using System;
using System.Linq;
using System.Xml.Linq;
using Acornima.Ast;
using Acornima.Cli.Helpers;
using Acornima.Jsx;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Commands;


[Command(CommandName, Description = "Parse JS code and print tree of variable scopes.")]
internal sealed class PrintScopesCommand
{
    public const string CommandName = "scopes";

    private readonly IConsole _console;

    public PrintScopesCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JS code to parse.")]
    public JavaScriptCodeType CodeType { get; set; }

    [Option("--jsx", Description = "Allow JSX expressions.")]
    public bool AllowJsx { get; set; }

    [Argument(0, Description = "The JS code to parse. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    private T CreateParserOptions<T>() where T : ParserOptions, new() => new T().RecordScopeInfoInUserData();

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

        var treePrinter = new TreePrinter(_console);
        treePrinter.Print(new[] { rootNode },
            node => node
                .DescendantNodes(descendIntoChildren: descendantNode => ReferenceEquals(node, descendantNode) || descendantNode.UserData is not ScopeInfo)
                .Where(node => node.UserData is ScopeInfo),
            node =>
            {
                var scopeInfo = (ScopeInfo)node.UserData!;
                var names = scopeInfo.VarVariables.Select(id => id.Name)
                    .Concat(scopeInfo.LexicalVariables.Select(id => id.Name))
                    .Concat(scopeInfo.Functions.Select(id => id.Name))
                    .Distinct()
                    .OrderBy(name => name);
                return $"{node.TypeText} ({string.Join(", ", names)})";
            });

        return 0;
    }
}
