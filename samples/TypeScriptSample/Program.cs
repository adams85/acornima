using System;
using Acornima;
using Acornima.TypeScript;
using Acornima.Ast;

namespace TypeScriptSample;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("🎉 TypeScript Extension for Acornima 🎉");
        Console.WriteLine("✅ Supports: Parameter types, Return types, Variable types");

        // Test simple JavaScript (baseline)
        var simpleJSCode = @"
const name = 'World';
function greet(name) {
    return 'Hello ' + name;
}
console.log(greet(name));
";

        Console.WriteLine("\n--- ✅ Simple JavaScript (Baseline) ---");
        TestParsing(simpleJSCode);

        // Test parameter type annotations
        var parameterTypesCode = @"
function greet(name: string, age: number) {
    return 'Hello ' + name + ', you are ' + age;
}
const result = greet('Alice', 25);
";

        Console.WriteLine("\n--- ✅ Parameter Type Annotations ---");
        TestParsing(parameterTypesCode);

        // Test return type annotations
        var returnTypesCode = @"
function add(a: number, b: number): number {
    return a + b;
}
function getName(): string {
    return 'TypeScript';
}
";

        Console.WriteLine("\n--- ✅ Return Type Annotations ---");
        TestParsing(returnTypesCode);

        // Test variable type annotations
        var variableTypesCode = @"
const message: string = 'Hello World';
let count: number = 42;
var isActive: boolean = true;
const items: string[] = ['a', 'b', 'c'];
";

        Console.WriteLine("\n--- ✅ Variable Type Annotations ---");
        TestParsing(variableTypesCode);

        // Show the transformation        // Test various inline object types
        var complexInlineObjectCode = @"
let user: { name: string, age: number } = { name: 'Alice', age: 25 };
const config: { host: string; port: number } = { host: 'localhost', port: 3000 };
function process(data: { id: number; value: string }): void {
    console.log(data.id, data.value);
}
const nested: { person: { name: string; age: number } } = { person: { name: 'Bob', age: 30 } };
";

        Console.WriteLine("\n--- 🧪 Testing Complex Inline Object Types ---");
        TestParsing(complexInlineObjectCode);

        Console.WriteLine("\n🔄 TypeScript → JavaScript Transformation Examples:");
        Console.WriteLine("   function greet(name: string): string         →  function greet(name)");
        Console.WriteLine("   const count: number = 42                    →  const count=42");
        Console.WriteLine("   let user: { name: string } = { name: 'A' }  →  let user={name:'A'}");
    }

    private static void TestParsing(string code)
    {
        try
        {
            var parser = new TypeScriptParser();
            var ast = parser.ParseScript(code);

            Console.WriteLine("Successfully parsed!");
            Console.WriteLine($"AST Type: {ast.GetType().Name}");
            Console.WriteLine($"Body contains {ast.Body.Count} statements");

            // Try to convert back to JavaScript
            Console.WriteLine("Generated JavaScript:");
            Console.WriteLine(ast.ToJavaScript());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing: {ex.Message}");
        }

        Console.WriteLine();
    }
}
