using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Tests.Acorn;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public partial class ParserTests
{

    #region Keywords

    private static IEnumerable<string> GetAllReservedWords()
    {
        return AcornIdentifier.Keywords.Values.SelectMany(value => value.Split(' '))
            .Concat(AcornIdentifier.ReservedWords.Values.SelectMany(value => value.Split(' ')))
            .Concat(new[] { "await" })
            .Distinct();
    }

    public static IEnumerable<(string, EcmaVersion, bool)> IsKeywordMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from ecmaVersion in new[] { EcmaVersion.ES3, EcmaVersion.ES5, EcmaVersion.ES6, EcmaVersion.Latest }
        select
        (
            word,
            ecmaVersion,
            ecmaVersion < EcmaVersion.ES6 && word is "export" or "import" // in these cases we deliberately deviate from the original acornjs implementation
                ? false
                : AcornUtils.WordsRegexp(AcornIdentifier.Keywords[ecmaVersion >= EcmaVersion.ES6 ? "6" : "5"]).IsMatch(word)
        );

    [Test]
    [MethodDataSource(nameof(IsKeywordMatchesAcornImplData))]
    public void IsKeywordMatchesAcornImpl(string word, EcmaVersion ecmaVersion, bool expectedIsKeyword)
    {
        Assert.Equal(expectedIsKeyword, Parser.IsKeyword(word.AsSpan(), ecmaVersion, out _));
    }

    public static IEnumerable<(string, bool)> IsKeywordRelationalOperatorMatchesAcornImplData =>
        from word in GetAllReservedWords()
        select (word, AcornIdentifier.KeywordRelationalOperator.IsMatch(word));

    [Test]
    [MethodDataSource(nameof(IsKeywordRelationalOperatorMatchesAcornImplData))]
    public void IsKeywordRelationalOperatorMatchesAcornImpl(string keyword, bool expectedIsKeyword)
    {
        Assert.Equal(expectedIsKeyword, Parser.IsKeywordRelationalOperator(keyword.AsSpan()));
    }

    [Test]
    [Arguments("that", EcmaVersion.ES3, false)]
    [Arguments("this", EcmaVersion.ES3, true)]
    [Arguments("super", EcmaVersion.ES3, false)]
    [Arguments("export", EcmaVersion.ES3, false)]
    [Arguments("import", EcmaVersion.ES3, false)]

    [Arguments("that", EcmaVersion.ES5, false)]
    [Arguments("this", EcmaVersion.ES5, true)]
    [Arguments("super", EcmaVersion.ES5, false)]
    [Arguments("export", EcmaVersion.ES5, false)]
    [Arguments("import", EcmaVersion.ES5, false)]

    [Arguments("that", EcmaVersion.ES6, false)]
    [Arguments("this", EcmaVersion.ES6, true)]
    [Arguments("super", EcmaVersion.ES6, true)]
    [Arguments("export", EcmaVersion.ES6, true)]
    [Arguments("import", EcmaVersion.ES6, true)]
    public void IsKeyword_Works(string word, EcmaVersion ecmaVersion, bool isKeyword)
    {
        Assert.Equal(isKeyword, Parser.IsKeyword(word.AsSpan(), ecmaVersion, out _));
    }

    #endregion

    #region Reserved words

    private static string GetReservedWordsNonStrict(bool allowReserved, EcmaVersion ecmaVersion, bool isModule)
    {
        var reserved = "";
        if (!allowReserved)
        {
            reserved = AcornIdentifier.ReservedWords[ecmaVersion >= EcmaVersion.ES6 ? "6" : ecmaVersion == EcmaVersion.ES5 ? "5" : "3"];
            if (isModule)
            {
                reserved += " await";
            }
        }
        return reserved;
    }

    public static IEnumerable<(string, bool, EcmaVersion, bool)> IsReservedWordNonStrictMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from ecmaVersion in new[] { EcmaVersion.ES3, EcmaVersion.ES5, EcmaVersion.ES6, EcmaVersion.Latest }
        from allowReserved in new[] { false, true }
        select
        (
            word,
            allowReserved,
            ecmaVersion,
            AcornUtils.WordsRegexp(GetReservedWordsNonStrict(allowReserved, ecmaVersion, isModule: false)).IsMatch(word)
        );

    [Test]
    [MethodDataSource(nameof(IsReservedWordNonStrictMatchesAcornImplData))]
    public void IsReservedWordNonStrictMatchesAcornImpl(string word, bool allowReserved, EcmaVersion ecmaVersion, bool expectedIsReservedWord)
    {
        Parser.GetIsReservedWord(inModule: false, ecmaVersion,
            allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No,
            out var isReservedWord, out _);

        Assert.Equal(expectedIsReservedWord, isReservedWord(word.AsSpan(), strict: false));
    }

    private static string GetReservedWordsStrict(bool allowReserved, EcmaVersion ecmaVersion, bool isModule, out string reservedWordsStrictBind)
    {
        var reserved = GetReservedWordsNonStrict(allowReserved, ecmaVersion, isModule);
        var reservedStrict = (reserved.Length > 0 ? reserved + " " : "") + AcornIdentifier.ReservedWords["strict"];
        reservedWordsStrictBind = reservedStrict + " " + AcornIdentifier.ReservedWords["strictBind"];
        return reservedStrict;
    }

    public static IEnumerable<(string, bool, EcmaVersion, bool, bool, bool)> IsReservedWordStrictMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from combination in new (EcmaVersion EcmaVersion, bool IsModule)[] {
            (EcmaVersion.ES5, false),
            (EcmaVersion.ES6, false),
            (EcmaVersion.ES6, true),
            (EcmaVersion.Latest, false),
            (EcmaVersion.Latest, true),
        }
        from allowReserved in new[] { false, true }
        select
        (
            word,
            allowReserved,
            combination.EcmaVersion,
            combination.IsModule,
            AcornUtils.WordsRegexp(GetReservedWordsStrict(allowReserved, combination.EcmaVersion, combination.IsModule, out var reservedWordsStrictBind)).IsMatch(word),
            AcornUtils.WordsRegexp(reservedWordsStrictBind).IsMatch(word)
        );

    [Test]
    [MethodDataSource(nameof(IsReservedWordStrictMatchesAcornImplData))]
    public void IsReservedWordStrictMatchesAcornImpl(string word, bool allowReserved, EcmaVersion ecmaVersion, bool isModule, bool expectedIsReservedWord, bool expectedIsReservedWordBind)
    {
        Parser.GetIsReservedWord(isModule, ecmaVersion,
            allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No,
            out var isReservedWord, out var isReservedWordBind);

        Assert.Equal(expectedIsReservedWord, isReservedWord(word.AsSpan(), strict: true));
        Assert.Equal(expectedIsReservedWordBind, isReservedWordBind(word.AsSpan(), strict: true));
    }

    [Test]
    [Arguments("word", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES3, false, false, false, true)]
    [Arguments("enum", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES3, false, false, false, true)]
    [Arguments("class", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES3, false, false, false, true)]
    [Arguments("abstract", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES3, true, false, false, false)]

    [Arguments("word", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("word", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("word", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("await", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("await", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("enum", EcmaVersion.ES5, false, false, false, true)]
    [Arguments("enum", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("enum", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("class", EcmaVersion.ES5, false, false, false, true)]
    [Arguments("class", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("class", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("let", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("let", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES5, true, false, true, true)]
    [Arguments("arguments", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("arguments", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("eval", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("eval", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES5, true, false, true, false)]

    [Arguments("word", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("word", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("word", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("word", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("word", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("await", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("await", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("await", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("await", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("enum", EcmaVersion.ES6, false, false, false, true)]
    [Arguments("enum", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("enum", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("enum", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("enum", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("class", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("class", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("class", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("class", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("class", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("let", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("let", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("let", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES6, true, false, true, true)]
    [Arguments("let", EcmaVersion.ES6, true, true, true, true)]
    [Arguments("arguments", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("arguments", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("arguments", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("arguments", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("eval", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("eval", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("eval", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("eval", EcmaVersion.ES6, true, true, true, false)]
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

    [Test]
    [Arguments("word", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("enum", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("class", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES3, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES3, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES3, true, false, false, false)]

    [Arguments("word", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("word", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("word", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("await", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("await", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("enum", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("enum", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("enum", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("class", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("class", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("class", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES5, false, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES5, true, false, true, false)]
    [Arguments("let", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("let", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES5, true, false, true, true)]
    [Arguments("arguments", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("arguments", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES5, true, false, true, true)]
    [Arguments("eval", EcmaVersion.ES5, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES5, false, false, true, true)]
    [Arguments("eval", EcmaVersion.ES5, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES5, true, false, true, true)]

    [Arguments("word", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("word", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("word", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("word", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("word", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("word", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("await", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("await", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("await", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("await", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("await", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("await", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("enum", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("enum", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("enum", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("enum", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("enum", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("enum", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("class", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("class", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("class", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("class", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("class", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("class", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, false, true, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, false, true, false)]
    [Arguments("abstract", EcmaVersion.ES6, true, true, true, false)]
    [Arguments("let", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("let", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("let", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("let", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("let", EcmaVersion.ES6, true, false, true, true)]
    [Arguments("let", EcmaVersion.ES6, true, true, true, true)]
    [Arguments("arguments", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("arguments", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("arguments", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("arguments", EcmaVersion.ES6, true, false, true, true)]
    [Arguments("arguments", EcmaVersion.ES6, true, true, true, true)]
    [Arguments("eval", EcmaVersion.ES6, false, false, false, false)]
    [Arguments("eval", EcmaVersion.ES6, false, false, true, true)]
    [Arguments("eval", EcmaVersion.ES6, false, true, true, true)]
    [Arguments("eval", EcmaVersion.ES6, true, false, false, false)]
    [Arguments("eval", EcmaVersion.ES6, true, false, true, true)]
    [Arguments("eval", EcmaVersion.ES6, true, true, true, true)]
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

    #endregion
}
