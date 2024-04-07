namespace Acornima.Jsx;

public record class JsxParserOptions : ParserOptions
{
    public static new readonly JsxParserOptions Default = new();

    public JsxParserOptions() : base(new JsxTokenizerOptions()) { }

    public new JsxTokenizerOptions GetTokenizerOptions() => (JsxTokenizerOptions)base.GetTokenizerOptions();

    internal readonly bool _jsxAllowNamespaces = true;
    /// <summary>
    /// Gets or sets whether to allow namespaces in element and attribute names.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool JsxAllowNamespaces { get => _jsxAllowNamespaces; init => _jsxAllowNamespaces = value; }

    internal readonly bool _jsxAllowNamespacedObjects;
    /// <summary>
    /// Gets or sets whether to allow namespaces in element names containing member expressions (e.g. <c>&lt;ns:a.b/&gt;</c>).
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool JsxAllowNamespacedObjects { get => _jsxAllowNamespacedObjects; init => _jsxAllowNamespacedObjects = value; }
}
