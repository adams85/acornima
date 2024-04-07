using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Acornima.Jsx;

public static class JsxToken
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string UnwrapStringValue(this in TokenValue tokenValue)
    {
        // TokenValue.TemplateCooked is repurposed to store the string value.
        Debug.Assert(tokenValue.Value is ValueProvider);
        return tokenValue.TemplateCooked!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TokenValue IdentifierValue(string value)
    {
        return new TokenValue(ValueProvider.IdentifierValueProvider, templateCooked: value);
    }

    public static Token Identifier(string value, Range range, in SourceLocation location)
    {
        return new Token(TokenKind.Extension, IdentifierValue(value ?? throw new ArgumentNullException(nameof(value))), range, location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TokenValue TextValue(string value)
    {
        return new TokenValue(ValueProvider.TextValueProvider, templateCooked: value);
    }

    public static Token Text(string value, Range range, in SourceLocation location)
    {
        return new Token(TokenKind.Extension, TextValue(value ?? throw new ArgumentNullException(nameof(value))), range, location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsxTokenKind JsxTokenKind(this in Token token)
    {
        return token.Kind == TokenKind.Extension && token._value.Value is ValueProvider valueProvider
            ? valueProvider.Kind
            : Jsx.JsxTokenKind.Unknown;
    }

    private sealed class ValueProvider : Token.ExtensionValueProvider
    {
        public static readonly ValueProvider IdentifierValueProvider = new(Jsx.JsxTokenKind.Identifier);
        public static readonly ValueProvider TextValueProvider = new(Jsx.JsxTokenKind.Text);

        private ValueProvider(JsxTokenKind kind)
        {
            Kind = kind;
        }

        public JsxTokenKind Kind { get; }

        public override string KindText => "JSX" + Kind;

        public override object? GetValue(in TokenValue value) => UnwrapStringValue(value);

        public override string ToString(in TokenValue value) => $"{KindText} ({UnwrapStringValue(value)})";
    }
}
