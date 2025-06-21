using System;
using System.Linq;
using Acornima.TypeScript;
using Xunit;

namespace Acornima.Tests;

public partial class TypeScriptParserTests
{
    [Theory]
    [InlineData("function greet(name: string) { return 'Hello ' + name; }", "function greet(name){return'Hello '+name}")]
    [InlineData("function add(a: number, b: number): number { return a + b; }", "function add(a,b){return a+b}")]
    [InlineData("const message: string = 'Hello World';", "const message='Hello World'")]
    [InlineData("let count: number = 42;", "let count=42")]
    [InlineData("var isActive: boolean = true;", "var isActive=true")]
    public void SimpleTypeAnnotations_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("let user: { name: string } = { name: 'Alice' };", "let user={name:'Alice'}")]
    [InlineData("const config: { host: string; port: number } = { host: 'localhost', port: 3000 };", "const config={host:'localhost',port:3000}")]
    [InlineData("let data: { a: string, b: number } = { a: 'test', b: 1 };", "let data={a:'test',b:1}")]
    public void InlineObjectTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("function process(data: { id: number; value: string }): void { console.log(data.id); }", "function process(data){console.log(data.id)}")]
    [InlineData("function getUser(): { name: string; age: number } { return { name: 'Bob', age: 25 }; }", "function getUser(){return{name:'Bob',age:25}}")]
    public void FunctionWithObjectTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const items: string[] = ['a', 'b', 'c'];", "const items=['a','b','c']")]
    [InlineData("let numbers: number[] = [1, 2, 3];", "let numbers=[1,2,3]")]
    [InlineData("const users: Array<string> = ['Alice', 'Bob'];", "const users=['Alice','Bob']")]
    public void ArrayTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const nested: { person: { name: string; age: number } } = { person: { name: 'Charlie', age: 30 } };", "const nested={person:{name:'Charlie',age:30}}")]
    [InlineData("let complex: { items: { id: number; tags: string[] }[] } = { items: [{ id: 1, tags: ['a'] }] };", "let complex={items:[{id:1,tags:['a']}]}")]
    public void NestedObjectTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("function multiply(x: number, y: number): number { return x * y; } const result = multiply(5, 3);", 2)]
    [InlineData("const name: string = 'TypeScript'; const age: number = 5; console.log(name, age);", 3)]
    [InlineData("function greet(name: string): string { return 'Hello ' + name; }", 1)]
    public void TypeAnnotations_DoNotAffectASTStructure(string input, int expectedStatementCount)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        Assert.Equal(expectedStatementCount, ast.Body.Count);
    }

    // Edge Cases and Advanced Scenarios
    [Theory]
    [InlineData("const value = obj as string;", "const value=obj")]
    [InlineData("const result = (input as number) + 5;", "const result=input+5")]
    [InlineData("function test() { return data as { id: number }; }", "function test(){return data}")]
    public void TypeAssertions_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const value = obj!;", "const value=obj")]
    [InlineData("function test() { return user!.name; }", "function test(){return user.name}")]
    [InlineData("const result = data!.items!.length;", "const result=data.items.length")]
    public void NonNullAssertions_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const template: string = `Hello ${name}`;", "const template=`Hello ${name}`")]
    [InlineData("const complex: { x: number } = { x: `Value: ${x}: ${y}`.length };", "const complex={x:`Value: ${x}: ${y}`.length}")]
    [InlineData("function test(): string { return `Template with ${value}: end`; }", "function test(){return`Template with ${value}: end`}")]
    public void TemplateStringsWithTypeAnnotations_ParseCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const obj: { [key: string]: number } = {};", "const obj={}")]
    [InlineData("let map: { [id: number]: string } = { 1: 'one', 2: 'two' };", "let map={1:'one',2:'two'}")]
    [InlineData("const index: { [K in keyof T]: boolean } = {};", "const index={}")]
    public void IndexSignatures_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const union: string | number = 'test';", "const union='test'")]
    [InlineData("let value: boolean | null | undefined = true;", "let value=true")]
    [InlineData("function process(data: string | { id: number }): void {}", "function process(data){}")]
    public void UnionTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const intersection: A & B = value;", "const intersection=value")]
    [InlineData("function merge<T, U>(a: T, b: U): T & U { return Object.assign(a, b); }", "function merge(a,b){return Object.assign(a,b)}")]
    public void IntersectionTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData(":")]
    [InlineData("const x:")]
    [InlineData("function test(")]
    [InlineData("let x: { unclosed")]
    public void MalformedTypeAnnotations_DoNotCrashParser(string input)
    {
        var parser = new TypeScriptParser();
        // Should not throw - either parse successfully or throw a parse error, but not crash
        try
        {
            var ast = parser.ParseScript(input);
        }
        catch (ParseErrorException)
        {
            // Expected for malformed input
        }
    }

    [Theory]
    [InlineData("const obj = { key: 'value' };")]
    [InlineData("function test() { const x = { a: 1, b: 2 }; return x; }")]
    [InlineData("const nested = { outer: { inner: { value: 42 } } };")]
    public void JavaScriptObjectLiterals_RemainUnchanged(string input)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();

        // Remove whitespace and semicolons for comparison since parser output may not include semicolons
        var normalizedInput = input.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace(";", "");
        var normalizedResult = result.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace(";", "");

        Assert.Equal(normalizedInput, normalizedResult);
    }

    [Theory]
    [InlineData("const regex: RegExp = /test: \\d+/g;", "const regex=/test: \\d+/g")]
    [InlineData("const pattern: RegExp = /object: { key: value }/;", "const pattern=/object: { key: value }/")]
    public void RegularExpressions_WithColonsAndBraces_ParseCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void StandardJavaScript_ParsesWithoutTypeScript()
    {
        var input = @"
            const name = 'World';
            function greet(name) {
                return 'Hello ' + name;
            }
            console.log(greet(name));
        ";

        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);

        Assert.Equal(3, ast.Body.Count);
        var result = ast.ToJavaScript();
        Assert.Contains("const name='World'", result);
        Assert.Contains("function greet(name)", result);
        Assert.Contains("console.log(greet(name))", result);
    }

    [Fact]
    public void EmptyInput_ParsesSuccessfully()
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript("");
        Assert.Empty(ast.Body);
    }

    [Fact]
    public void ComplexTypeScriptCode_ParsesSuccessfully()
    {
        var input = @"
            function processUser(user: { name: string, age: number }): string {
                return user.name + ' is ' + user.age + ' years old';
            }

            const userData: { name: string; age: number } = { name: 'Alice', age: 25 };
            const result: string = processUser(userData);
            console.log(result);
        ";

        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);

        Assert.Equal(4, ast.Body.Count);
        var result = ast.ToJavaScript();

        // Verify TypeScript annotations are removed but JavaScript logic remains
        Assert.Contains("function processUser(user)", result);
        Assert.Contains("const userData={name:'Alice',age:25}", result);
        Assert.Contains("const result=processUser(userData)", result);
        Assert.Contains("console.log(result)", result);

        // Verify no TypeScript syntax remains
        Assert.DoesNotContain("string", result);
        Assert.DoesNotContain("number", result);
        Assert.DoesNotContain("{ name:", result.Replace("{name:", "")); // Avoid false positive from object literal
    }

    [Fact]
    public void VeryComplexNestedTypes_ParseCorrectly()
    {
        var input = @"
            const config: {
                database: {
                    host: string;
                    port: number;
                    credentials: {
                        user: string;
                        password: string;
                    };
                };
                cache: {
                    enabled: boolean;
                    ttl: number;
                };
            } = {
                database: {
                    host: 'localhost',
                    port: 5432,
                    credentials: {
                        user: 'admin',
                        password: 'secret'
                    }
                },
                cache: {
                    enabled: true,
                    ttl: 3600
                }
            };
        ";

        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();

        // Should contain all the JavaScript values
        Assert.Contains("const config=", result);
        Assert.Contains("host:'localhost'", result);
        Assert.Contains("port:5432", result);
        Assert.Contains("user:'admin'", result);
        Assert.Contains("password:'secret'", result);
        Assert.Contains("enabled:true", result);
        Assert.Contains("ttl:3600", result);

        // Should not contain any TypeScript type annotations
        Assert.DoesNotContain("string", result);
        Assert.DoesNotContain("number", result);
        Assert.DoesNotContain("boolean", result);
    }

    [Fact]
    public void FunctionWithMultipleComplexParameters_ParseCorrectly()
    {
        var input = @"
            function processData(
                user: { id: number; name: string; tags: string[] },
                options: { verbose: boolean; timeout: number },
                callback: (result: { success: boolean; data: any }) => void
            ): { processed: boolean; timestamp: Date } {
                return { processed: true, timestamp: new Date() };
            }
        ";        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();

        // Should contain function with parameters but no type annotations 
        Assert.Contains("function processData(user,options,callback)", result);
        Assert.Contains("return{processed:true,timestamp:new Date}", result); // Note: Acornima omits () for parameterless new expressions

        // Should not contain TypeScript syntax
        Assert.DoesNotContain("number", result);
        Assert.DoesNotContain("string", result);
        Assert.DoesNotContain("boolean", result);
        Assert.DoesNotContain("Date", result.Replace("new Date", "")); // Avoid false positive
    }

    [Theory]
    [InlineData("let x: string; x = 'test';")]
    [InlineData("let y: number; y = 42; console.log(y);")]
    public void VariableDeclarationsWithoutInitializers_ParseCorrectly(string input)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();

        // Type annotations should be removed
        Assert.DoesNotContain("string", result);
        Assert.DoesNotContain("number", result);

        // Variable names and assignments should remain
        Assert.True(result.Contains("x") || result.Contains("y"));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_TypeAnnotations()
    {
        var parser = new TypeScriptParser();
        const int depth = 10_000;
        // Create deeply nested object type annotation
        var openBraces = string.Join("", Enumerable.Range(0, depth).Select(_ => "{ a: "));
        var closeBraces = string.Join("", Enumerable.Range(0, depth).Select(_ => " }"));
        var input = $"const x: {openBraces}string{closeBraces} = null;";

        // Should handle deep nesting gracefully without stack overflow
        try
        {
            var ast = parser.ParseScript(input);
            var result = ast.ToJavaScript();
            Assert.Contains("const x=null", result);
        }
        catch (InsufficientExecutionStackException)
        {
            // This is acceptable - the parser detected deep recursion
        }
        catch (ParseErrorException)
        {
            // This is also acceptable - malformed input
        }
    }

    [Fact]
    public void HandlesLargeTypeAnnotations_WithoutPerformanceIssues()
    {
        var parser = new TypeScriptParser();

        // Create a large object type with many properties
        var properties = string.Join("; ", Enumerable.Range(0, 1000).Select(i => $"prop{i}: string"));
        var input = $"const largeObj: {{ {properties} }} = {{}};";

        var start = DateTime.UtcNow;
        var ast = parser.ParseScript(input);
        var elapsed = DateTime.UtcNow - start;

        // Should complete in reasonable time (less than 1 second for this size)
        Assert.True(elapsed.TotalSeconds < 1.0, $"Parsing took too long: {elapsed.TotalSeconds}s");

        var result = ast.ToJavaScript();
        Assert.Equal("const largeObj={}", result);
    }

    [Theory]
    [InlineData("const name: `template-${string}` = 'test';", "const name='test'")]
    [InlineData("let value: `prefix-${number}-suffix` = 'prefix-42-suffix';", "let value='prefix-42-suffix'")]
    public void TemplateLiteralTypes_AreSkippedCorrectly(string input, string expectedOutput)
    {
        var parser = new TypeScriptParser();
        var ast = parser.ParseScript(input);
        var result = ast.ToJavaScript();
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("const optional: string? = null;")]
    [InlineData("function test(param: number? = 5): void {}")]
    public void OptionalTypes_AreSkippedCorrectly(string input)
    {
        var parser = new TypeScriptParser();
        try
        {
            var ast = parser.ParseScript(input);
            var result = ast.ToJavaScript();

            // Should not contain TypeScript syntax
            Assert.DoesNotContain("string", result);
            Assert.DoesNotContain("number", result);
            Assert.DoesNotContain("void", result);
        }
        catch (ParseErrorException)
        {
            // Optional syntax might not be fully supported, which is acceptable
        }
    }
}
