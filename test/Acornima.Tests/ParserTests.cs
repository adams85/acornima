using System;
using Acornima.Ast;
using Acornima.Helpers;
using Xunit;

namespace Acornima.Tests;

public partial class ParserTests
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Ensures that we don't regress in stack handling, only test in modern runtime for now
    /// </summary>
    [Fact]
    public void CanHandleDeepRecursionWithoutStackOverflow()
    {
        if (OperatingSystem.IsMacOS())
        {
            // stack limit differ quite a lot
            return;
        }

        var parser = new Parser();
#if DEBUG
        const int depth = 450;
#else
        const int depth = 830;
#endif
        var input = $"if ({new string('(', depth)}true{new string(')', depth)}) {{ }}";
        parser.ParseScript(input);
    }
#endif

    [Theory]
    [InlineData("'use strict'; 0", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'; 0", true, EcmaVersion.ES6, null)]
    [InlineData("'use strict'; 00", false, EcmaVersion.ES5, "Invalid number")]
    [InlineData("'use strict'; 00", true, EcmaVersion.ES6, "Invalid number")]
    [InlineData("'\\00'; 'use strict'; 00", false, EcmaVersion.ES3, null)]
    [InlineData("'\\00'; 'use strict'; 00", false, EcmaVersion.ES5, "Octal literal in strict mode")]
    [InlineData("'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal literal in strict mode")]
    [InlineData("'use strict'; '\\00'; 00", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'; '\\00'; 00", false, EcmaVersion.ES5, "Octal literal in strict mode")]
    [InlineData("'use strict'; '\\00'; 00", true, EcmaVersion.ES6, "Octal literal in strict mode")]

    [InlineData("'x';'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'x';'use strict'; 00", false, EcmaVersion.ES5, "Invalid number")]
    [InlineData("'x' 'use strict'; 0", false, EcmaVersion.ES5, "Unexpected token")]
    [InlineData("'x' 'use strict'; 00", false, EcmaVersion.ES5, "Unexpected token")]
    [InlineData("'x'\n'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'x'\n'use strict'; 00", false, EcmaVersion.ES5, "Invalid number")]

    [InlineData("function f() {'use strict'; 0 }", false, EcmaVersion.ES5, null)]
    [InlineData("() => {'use strict'; 0 }", true, EcmaVersion.ES6, null)]
    [InlineData("function f() {'use strict'; 00 }", false, EcmaVersion.ES5, "Invalid number")]
    [InlineData("() => {'use strict'; 00 }", true, EcmaVersion.ES6, "Invalid number")]
    [InlineData("function f() {'\\00'; 'use strict'; 00", false, EcmaVersion.ES5, "Octal literal in strict mode")]
    [InlineData("() => {'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal literal in strict mode")]
    [InlineData("function f() {'use strict'; '\\00'; 00", false, EcmaVersion.ES5, "Octal literal in strict mode")]
    [InlineData("() => {'use strict'; '\\00'; 00", true, EcmaVersion.ES6, "Octal literal in strict mode")]

    [InlineData("(x = 0) => 00", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => 00", true, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => { 00 }", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => { 00 }", true, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 0 }", true, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => {'use strict'; 00 }", true, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES6, "Octal literal in strict mode")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES6, "Octal literal in strict mode")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal literal in strict mode")]

    [InlineData("(x = 0) => {'use strict'; 0 }; 00", false, EcmaVersion.ES6, null)]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 0 }; 00", false, EcmaVersion.ES6, "Invalid number")]
    [InlineData("(x = 0) => {'use strict'; 0 }; 00", true, EcmaVersion.ES6, "Invalid number")]

    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES5, "Binding arguments in strict mode")]
    [InlineData("'use strict';\r\n(arguments)=>{}", false, EcmaVersion.ES6, "Binding arguments in strict mode")]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES5, "Binding eval in strict mode")]
    [InlineData("'use strict'\r\n(eval)=>{}", false, EcmaVersion.ES6, "Unexpected token")]
    public void ShouldHandleStrictModeDetectionEdgeCases(string input, bool isModule, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });

        if (expectedError is null)
        {
            Program root = isModule ? parser.ParseModule(input) : parser.ParseScript(input);
            Assert.NotNull(root);

            // TODO
            //if (ecmaVersion >= EcmaVersion.ES5)
            //{
            //    Assert.Contains(root.DescendantNodes(), stmt => stmt.GetType() == typeof(Directive));
            //}
            //else
            //{
            //    Assert.DoesNotContain(root.DescendantNodes(), stmt => stmt.GetType() == typeof(Directive));
            //}
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => isModule ? parser.ParseModule(input) : parser.ParseScript(input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("that", EcmaVersion.ES3, false, false)]
    [InlineData("that", EcmaVersion.ES3, true, false)]
    [InlineData("this", EcmaVersion.ES3, false, true)]
    [InlineData("this", EcmaVersion.ES3, true, true)]
    [InlineData("super", EcmaVersion.ES3, false, false)]
    [InlineData("super", EcmaVersion.ES3, true, false)]
    [InlineData("export", EcmaVersion.ES3, false, false)]
    [InlineData("export", EcmaVersion.ES3, true, true)]
    [InlineData("import", EcmaVersion.ES3, false, false)]
    [InlineData("import", EcmaVersion.ES3, true, true)]

    [InlineData("that", EcmaVersion.ES5, false, false)]
    [InlineData("that", EcmaVersion.ES5, true, false)]
    [InlineData("this", EcmaVersion.ES5, false, true)]
    [InlineData("this", EcmaVersion.ES5, true, true)]
    [InlineData("super", EcmaVersion.ES5, false, false)]
    [InlineData("super", EcmaVersion.ES5, true, false)]
    [InlineData("export", EcmaVersion.ES5, false, false)]
    [InlineData("export", EcmaVersion.ES5, true, true)]
    [InlineData("import", EcmaVersion.ES5, false, false)]
    [InlineData("import", EcmaVersion.ES5, true, true)]

    [InlineData("that", EcmaVersion.ES6, false, false)]
    [InlineData("that", EcmaVersion.ES6, true, false)]
    [InlineData("this", EcmaVersion.ES6, false, true)]
    [InlineData("this", EcmaVersion.ES6, true, true)]
    [InlineData("super", EcmaVersion.ES6, false, true)]
    [InlineData("super", EcmaVersion.ES6, true, true)]
    [InlineData("export", EcmaVersion.ES6, false, true)]
    [InlineData("export", EcmaVersion.ES6, true, true)]
    [InlineData("import", EcmaVersion.ES6, false, true)]
    [InlineData("import", EcmaVersion.ES6, true, true)]
    public void IsKeyword_Works(string word, EcmaVersion ecmaVersion, bool isModule, bool isKeyword)
    {
        Assert.Equal(isKeyword, Parser.IsKeyword(word.AsSpan(), ecmaVersion, isModule));
    }

    [Theory]
    [InlineData("word", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("class", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("abstract", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, true, false, false, false)]

    [InlineData("word", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, false, true)]
    [InlineData("class", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("class", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, true, false)]

    [InlineData("word", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("await", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("enum", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("eval", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("eval", EcmaVersion.ES6, true, true, true, false)]
    public void IsReservedWord_Works(string word, EcmaVersion ecmaVersion, bool allowReserved, bool isModule, bool isStrict, bool isReservedWord)
    {
        var parser = new Parser(new ParserOptions
        {
            EcmaVersion = ecmaVersion,
            AllowReserved = allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No
        });
        parser.Reset("", 0, 0, isModule ? SourceType.Module : SourceType.Script, null, strict: false);

        Assert.Equal(isReservedWord, parser._isReservedWord(word.AsSpan(), isStrict));
    }

    [Theory]
    [InlineData("word", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, true, false, false, false)]

    [InlineData("word", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("class", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("eval", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("eval", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, true, true)]

    [InlineData("word", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("await", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("enum", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("eval", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("eval", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("eval", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("eval", EcmaVersion.ES6, true, true, true, true)]
    public void IsReservedWordBind_Works(string word, EcmaVersion ecmaVersion, bool allowReserved, bool isModule, bool isStrict, bool isReservedWord)
    {
        var parser = new Parser(new ParserOptions
        {
            EcmaVersion = ecmaVersion,
            AllowReserved = allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No
        });
        parser.Reset("", 0, 0, isModule ? SourceType.Module : SourceType.Script, null, strict: false);

        Assert.Equal(isReservedWord, parser._isReservedWordBind(word.AsSpan(), isStrict));
    }

    [Theory]
    [InlineData("as")]
    [InlineData("do")]
    [InlineData("if")]
    [InlineData("in")]
    [InlineData("of")]
    [InlineData("for")]
    [InlineData("get")]
    [InlineData("let")]
    [InlineData("new")]
    [InlineData("set")]
    [InlineData("try")]
    [InlineData("var")]
    [InlineData("case")]
    [InlineData("else")]
    [InlineData("enum")]
    [InlineData("from")]
    [InlineData("null")]
    [InlineData("this")]
    [InlineData("true")]
    [InlineData("void")]
    [InlineData("with")]
    [InlineData("async")]
    [InlineData("await")]
    [InlineData("break")]
    [InlineData("catch")]
    [InlineData("class")]
    [InlineData("const")]
    [InlineData("false")]
    [InlineData("super")]
    [InlineData("throw")]
    [InlineData("while")]
    [InlineData("yield")]
    [InlineData("delete")]
    [InlineData("export")]
    [InlineData("import")]
    [InlineData("return")]
    [InlineData("static")]
    [InlineData("switch")]
    [InlineData("typeof")]
    [InlineData("finally")]
    [InlineData("continue")]
    [InlineData("debugger")]
    [InlineData("function")]
    [InlineData("arguments")]
    [InlineData("instanceof")]
    [InlineData("constructor")]
    public void UsesInternedInstancesForWellKnownTokens(string token)
    {
        var stringPool = new StringPool();

        var nonInternedToken = new string(token.ToCharArray());
        var slicedToken = Tokenizer.DeduplicateString(nonInternedToken.AsSpan(), ref stringPool);
        Assert.Equal(token, slicedToken);

        Assert.NotNull(string.IsInterned(slicedToken));
        Assert.Equal(0, stringPool.Count);
    }

    [Fact]
    public void UsesPooledInstancesForNotWellKnownTokens()
    {
        var stringPool = new StringPool();

        var token = "pow2";
        var slicedToken1 = Tokenizer.DeduplicateString("pow2".AsSpan(), ref stringPool);
        Assert.Equal(token, slicedToken1);

        var source = "async function pow2(x) { return x ** 2; }";
        var slicedToken2 = Tokenizer.DeduplicateString(source.AsSpan(15, token.Length), ref stringPool);
        Assert.Equal(token, slicedToken2);

        Assert.Same(slicedToken1, slicedToken2);
        Assert.Equal(1, stringPool.Count);
    }
}
