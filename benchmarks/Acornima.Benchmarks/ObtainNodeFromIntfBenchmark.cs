using Acornima.Ast;
using BenchmarkDotNet.Attributes;

namespace Acornima.Benchmark;

[MemoryDiagnoser]
public class ObtainNodeFromIntfBenchmark
{
    private INode _node = null!;

    [GlobalSetup]
    public void Setup()
    {
        _node = new ArrowFunctionExpression(default, new NullLiteral("null"), expression: true, async: false);
    }

    [Params(10, 10000)]
    public int Iterations { get; set; }

    [Benchmark]
    public int Property()
    {
        var counter = 0;
        for (var i = 0; i < Iterations; i++)
        {
            counter += (int)_node.Node.Type;
        }
        return counter;
    }

    [Benchmark]
    public int DirectTypeCasting()
    {
        var counter = 0;
        for (var i = 0; i < Iterations; i++)
        {
            counter += (int)((Node)_node).Type;
        }
        return counter;
    }

    [Benchmark]
    public int HelperTypeCasting()
    {
        var counter = 0;
        for (var i = 0; i < Iterations; i++)
        {
            counter += (int)_node.As<Node>().Type;
        }
        return counter;
    }
}
