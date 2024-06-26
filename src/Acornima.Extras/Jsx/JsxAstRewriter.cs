using Acornima.Helpers;

namespace Acornima.Jsx;

using static ExceptionHelper;

[AutoGeneratedAstVisitor(VisitorType = typeof(IJsxAstVisitor), TargetVisitorFieldName = nameof(_rewriter), BaseVisitorFieldName = nameof(_jsxVisitor))]
public partial class JsxAstRewriter : AstRewriter, IJsxAstVisitor
{
    /// <summary>
    /// Creates an <see cref="IJsxAstVisitor"/> instance which can be used for working around multiple inheritance:
    /// the returned instance re-routes visitations of JSX nodes to the specified <paramref name="rewriter"/>,
    /// thus it can be used for emulating base class method calls.
    /// </summary>
    public static IJsxAstVisitor CreateJsxRewriterFor<TRewriter>(TRewriter rewriter)
        where TRewriter : AstRewriter, IJsxAstVisitor
    {
        return new JsxAstRewriter(rewriter ?? ThrowArgumentNullException<AstRewriter>(nameof(rewriter)));
    }

    private readonly AstRewriter _rewriter;
    private readonly IJsxAstVisitor _jsxVisitor;

    public JsxAstRewriter()
    {
        _rewriter = this;
        _jsxVisitor = JsxAstVisitor.CreateJsxVisitorFor(this);
    }

    private JsxAstRewriter(AstRewriter rewriter)
    {
        _rewriter = rewriter;
        _jsxVisitor = JsxAstVisitor.CreateJsxVisitorFor(this);
    }
}
