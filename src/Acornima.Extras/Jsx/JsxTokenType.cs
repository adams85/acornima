using Acornima.Jsx;

namespace Acornima;

// https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `function getJsxTokens`

internal static class JsxTokenType
{
    public static readonly TokenType Name = new TokenType("jsxName", TokenKind.Extension);
    public static readonly TokenType Text = new TokenType("jsxText", TokenKind.Extension, beforeExpression: true);
    public static readonly TokenType TagStart = new TokenType("jsxTagStart", TokenKind.Punctuator, startsExpression: true, updateContext: JsxTokenizer.UpdateContext_TagStart);
    public static readonly TokenType TagEnd = new TokenType("jsxTagEnd", TokenKind.Punctuator, updateContext: JsxTokenizer.UpdateContext_TagEnd);
}
