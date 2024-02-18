using System;
using Acornima.Ast;
using Xunit;

namespace Acornima.Tests;

public class AstVisitorTests
{
    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion()
    {
        Expression expression = new Identifier("x");
        for (int i = 0; i < 100_000; i++)
        {
            expression = new ParenthesizedExpression(expression);
        }

        var visitor = new AstVisitor();
        Assert.Throws<InsufficientExecutionStackException>(() => visitor.Visit(expression));
    }
}
