using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public readonly partial struct ChildNodes : IEnumerable<Node>
{
    private readonly Node? _parentNode;

    internal ChildNodes(Node parentNode)
    {
        _parentNode = parentNode;
    }

    public bool IsEmpty()
    {
        using var enumerator = GetEnumerator();
        return !enumerator.MoveNext();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public partial struct Enumerator : IEnumerator<Node>
    {
        private readonly Node? _parentNode;
        private readonly IEnumerator<Node>? _enumerator;
        internal int _propertyIndex;
        internal int _listIndex;
        private Node? _current;

        public Enumerator(in ChildNodes childNodes)
        {
            if (childNodes._parentNode is { } parentNode)
            {
                _enumerator = parentNode.GetChildNodes();
                _parentNode = _enumerator is null ? parentNode : null;
            }
            else
            {
                _parentNode = null;
                _enumerator = null;
            }
            _propertyIndex = 0;
            _listIndex = 0;
            _current = null;
        }

        public readonly void Dispose()
        {
            _enumerator?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _parentNode is not null
                ? (_current = _parentNode.NextChildNode(ref this)) is not null
                : MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_enumerator is not null && _enumerator.MoveNext())
            {
                _current = _enumerator.Current;
                return true;
            }

            _current = null;
            return false;
        }

        public readonly Node Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current!; }

        readonly object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            if (_parentNode is not null)
            {
                _propertyIndex = 0;
                _listIndex = 0;
            }
            else if (_enumerator is not null)
            {
                _enumerator.Reset();
            }

            _current = null;
        }
    }
}
