using System;
using Acornima.Ast;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class AstVisitorTests
{
    [Test]
    public void ThrowsCatchableExceptionOnTooDeepRecursion()
    {
        Expression expression = new Identifier("x");
        for (int i = 0; i < 1_000_000; i++)
        {
            expression = new ParenthesizedExpression(expression);
        }

        var visitor = new AstVisitor();
        Assert.Throws<InsufficientExecutionStackException>(() => visitor.Visit(expression));
    }
}
