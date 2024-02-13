using System.Collections.Generic;

namespace Acornima;

public sealed class ParseErrorCollector : ParseErrorHandler
{
    private readonly List<ParseError> _errors = new();

    public IReadOnlyCollection<ParseError> Errors => _errors;

    protected internal override void Reset()
    {
        _errors.Clear();
        if (_errors.Capacity > 16)
        {
            _errors.Capacity = 16;
        }
    }

    protected override void RecordError(ParseError error)
    {
        _errors.Add(error);
    }
}
