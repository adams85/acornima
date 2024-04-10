//HintName: ChildNodes.Helpers.g.cs
#nullable enable

namespace Acornima.Ast;

partial struct ChildNodes
{
    partial struct Enumerator
    {
        internal Acornima.Ast.Node? MoveNext(Acornima.Ast.Node arg0)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext<T0>(in Acornima.Ast.NodeList<T0> arg0)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullable(Acornima.Ast.Node? arg0)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto default;
                    }
                    
                    return arg0;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullable<T0>(in Acornima.Ast.NodeList<T0?> arg0)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    if (item is null)
                    {
                        goto case 0;
                    }
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext(Acornima.Ast.Node arg0, Acornima.Ast.Node arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext<T0>(in Acornima.Ast.NodeList<T0> arg0, Acornima.Ast.Node arg1)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext<T1>(Acornima.Ast.Node arg0, in Acornima.Ast.NodeList<T1> arg1)
            where T1 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Acornima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0(Acornima.Ast.Node? arg0, Acornima.Ast.Node arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0<T1>(Acornima.Ast.Node? arg0, in Acornima.Ast.NodeList<T1> arg1)
            where T1 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Acornima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt1(Acornima.Ast.Node arg0, Acornima.Ast.Node? arg1)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    if (arg1 is null)
                    {
                        goto default;
                    }
                    
                    return arg1;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext(Acornima.Ast.Node arg0, Acornima.Ast.Node arg1, Acornima.Ast.Node arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext<T0>(in Acornima.Ast.NodeList<T0> arg0, Acornima.Ast.Node arg1, Acornima.Ast.Node arg2)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNext<T0, T2>(in Acornima.Ast.NodeList<T0> arg0, Acornima.Ast.Node arg1, in Acornima.Ast.NodeList<T2> arg2)
            where T0 : Acornima.Ast.Node
            where T2 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    if (_listIndex >= arg2.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    item = arg2[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0<T1>(Acornima.Ast.Node? arg0, in Acornima.Ast.NodeList<T1> arg1, Acornima.Ast.Node arg2)
            where T1 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 2;
                    }
                    
                    Acornima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                case 2:
                    _propertyIndex++;
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0<T2>(Acornima.Ast.Node? arg0, Acornima.Ast.Node arg1, in Acornima.Ast.NodeList<T2> arg2)
            where T2 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    if (_listIndex >= arg2.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    Acornima.Ast.Node? item = arg2[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt2(Acornima.Ast.Node arg0, Acornima.Ast.Node arg1, Acornima.Ast.Node? arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto default;
                    }
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt2<T0>(in Acornima.Ast.NodeList<T0> arg0, Acornima.Ast.Node arg1, Acornima.Ast.Node? arg2)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto default;
                    }
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt1_2(Acornima.Ast.Node arg0, Acornima.Ast.Node? arg1, Acornima.Ast.Node? arg2)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    if (arg1 is null)
                    {
                        goto case 2;
                    }
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto default;
                    }
                    
                    return arg2;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0_2<T1, T3>(Acornima.Ast.Node? arg0, in Acornima.Ast.NodeList<T1> arg1, Acornima.Ast.Node? arg2, in Acornima.Ast.NodeList<T3> arg3)
            where T1 : Acornima.Ast.Node
            where T3 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    if (_listIndex >= arg1.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 2;
                    }
                    
                    Acornima.Ast.Node? item = arg1[_listIndex++];
                    
                    return item;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto case 3;
                    }
                    
                    return arg2;
                case 3:
                    if (_listIndex >= arg3.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto default;
                    }
                    
                    item = arg3[_listIndex++];
                    
                    return item;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt1_2<T0>(in Acornima.Ast.NodeList<T0> arg0, Acornima.Ast.Node? arg1, Acornima.Ast.Node? arg2, Acornima.Ast.Node arg3)
            where T0 : Acornima.Ast.Node
        {
            switch (_propertyIndex)
            {
                case 0:
                    if (_listIndex >= arg0.Count)
                    {
                        _listIndex = 0;
                        _propertyIndex++;
                        goto case 1;
                    }
                    
                    Acornima.Ast.Node? item = arg0[_listIndex++];
                    
                    return item;
                case 1:
                    _propertyIndex++;
                    
                    if (arg1 is null)
                    {
                        goto case 2;
                    }
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto case 3;
                    }
                    
                    return arg2;
                case 3:
                    _propertyIndex++;
                    
                    return arg3;
                default:
                    return null;
            }
        }
        
        internal Acornima.Ast.Node? MoveNextNullableAt0_1_2(Acornima.Ast.Node? arg0, Acornima.Ast.Node? arg1, Acornima.Ast.Node? arg2, Acornima.Ast.Node arg3)
        {
            switch (_propertyIndex)
            {
                case 0:
                    _propertyIndex++;
                    
                    if (arg0 is null)
                    {
                        goto case 1;
                    }
                    
                    return arg0;
                case 1:
                    _propertyIndex++;
                    
                    if (arg1 is null)
                    {
                        goto case 2;
                    }
                    
                    return arg1;
                case 2:
                    _propertyIndex++;
                    
                    if (arg2 is null)
                    {
                        goto case 3;
                    }
                    
                    return arg2;
                case 3:
                    _propertyIndex++;
                    
                    return arg3;
                default:
                    return null;
            }
        }
    }
}
