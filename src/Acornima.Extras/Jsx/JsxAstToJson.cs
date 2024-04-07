namespace Acornima.Jsx;

public record class JsxAstToJsonOptions : AstToJsonOptions
{
    public static new readonly JsxAstToJsonOptions Default = new();

    protected internal override AstToJsonConverter CreateConverter(JsonWriter writer) => new JsxAstToJsonConverter(writer, this);
}
