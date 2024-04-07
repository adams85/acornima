namespace Acornima;

// https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `function getJsxTokens`

internal static class JsxTokenContext
{
    public static readonly TokenContext InOpeningTag = new((TokenContextKind)JsxTokenContextKind.OpeningTag, isExpression: false);
    public static readonly TokenContext InClosingTag = new((TokenContextKind)JsxTokenContextKind.ClosingTag, isExpression: false);
    public static readonly TokenContext InExpression = new((TokenContextKind)JsxTokenContextKind.Expression, isExpression: true, preserveSpace: true);
}
