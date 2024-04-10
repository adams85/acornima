using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Tests.Acorn;
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

    public static IEnumerable<object[]> IsKeywordMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from ecmaVersion in new[] { EcmaVersion.ES3, EcmaVersion.ES5, EcmaVersion.ES6, EcmaVersion.Latest }
        from isModule in new[] { false, true }
        select new object[]
        {
            word,
            ecmaVersion,
            ecmaVersion < EcmaVersion.ES6 && word is "export" or "import" // in these cases we deliberately deviate from the original acornjs implementation
                ? false
                : AcornUtils.WordsRegexp(AcornIdentifier.Keywords[ecmaVersion >= EcmaVersion.ES6 ? "6" : isModule ? "5module" : "5"]).IsMatch(word)
        };

    [Theory]
    [MemberData(nameof(IsKeywordMatchesAcornImplData))]
    public void IsKeywordMatchesAcornImpl(string word, EcmaVersion ecmaVersion, bool expectedIsKeyword)
    {
        Assert.Equal(expectedIsKeyword, Parser.IsKeyword(word.AsSpan(), ecmaVersion, out _));
    }

    public static IEnumerable<object[]> IsKeywordRelationalOperatorMatchesAcornImplData =>
        from word in GetAllReservedWords()
        select new object[] { word, AcornIdentifier.KeywordRelationalOperator.IsMatch(word) };

    [Theory]
    [MemberData(nameof(IsKeywordRelationalOperatorMatchesAcornImplData))]
    public void IsKeywordRelationalOperatorMatchesAcornImpl(string keyword, bool expectedIsKeyword)
    {
        Assert.Equal(expectedIsKeyword, Parser.IsKeywordRelationalOperator(keyword.AsSpan()));
    }

    [Theory]
    [InlineData("that", EcmaVersion.ES3, false)]
    [InlineData("this", EcmaVersion.ES3, true)]
    [InlineData("super", EcmaVersion.ES3, false)]
    [InlineData("export", EcmaVersion.ES3, false)]
    [InlineData("import", EcmaVersion.ES3, false)]

    [InlineData("that", EcmaVersion.ES5, false)]
    [InlineData("this", EcmaVersion.ES5, true)]
    [InlineData("super", EcmaVersion.ES5, false)]
    [InlineData("export", EcmaVersion.ES5, false)]
    [InlineData("import", EcmaVersion.ES5, false)]

    [InlineData("that", EcmaVersion.ES6, false)]
    [InlineData("this", EcmaVersion.ES6, true)]
    [InlineData("super", EcmaVersion.ES6, true)]
    [InlineData("export", EcmaVersion.ES6, true)]
    [InlineData("import", EcmaVersion.ES6, true)]
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

    public static IEnumerable<object[]> IsReservedWordNonStrictMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from ecmaVersion in new[] { EcmaVersion.ES3, EcmaVersion.ES5, EcmaVersion.ES6, EcmaVersion.Latest }
        from allowReserved in new[] { false, true }
        select new object[]
        {
            word,
            allowReserved,
            ecmaVersion,
            AcornUtils.WordsRegexp(GetReservedWordsNonStrict(allowReserved, ecmaVersion, isModule: false)).IsMatch(word),
        };

    [Theory]
    [MemberData(nameof(IsReservedWordNonStrictMatchesAcornImplData))]
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

    public static IEnumerable<object[]> IsReservedWordStrictMatchesAcornImplData =>
        from word in GetAllReservedWords()
        from combination in new (EcmaVersion EcmaVersion, bool IsModule)[] {
            (EcmaVersion.ES5, false),
            (EcmaVersion.ES6, false),
            (EcmaVersion.ES6, true),
            (EcmaVersion.Latest, false),
            (EcmaVersion.Latest, true),
        }
        from allowReserved in new[] { false, true }
        select new object[]
        {
            word,
            allowReserved,
            combination.EcmaVersion,
            combination.IsModule,
            AcornUtils.WordsRegexp(GetReservedWordsStrict(allowReserved, combination.EcmaVersion, combination.IsModule, out var reservedWordsStrictBind)).IsMatch(word),
            AcornUtils.WordsRegexp(reservedWordsStrictBind).IsMatch(word),
        };

    [Theory]
    [MemberData(nameof(IsReservedWordStrictMatchesAcornImplData))]
    public void IsReservedWordStrictMatchesAcornImpl(string word, bool allowReserved, EcmaVersion ecmaVersion, bool isModule, bool expectedIsReservedWord, bool expectedIsReservedWordBind)
    {
        Parser.GetIsReservedWord(isModule, ecmaVersion,
            allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No,
            out var isReservedWord, out var isReservedWordBind);

        Assert.Equal(expectedIsReservedWord, isReservedWord(word.AsSpan(), strict: true));
        Assert.Equal(expectedIsReservedWordBind, isReservedWordBind(word.AsSpan(), strict: true));
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

    #endregion
}
