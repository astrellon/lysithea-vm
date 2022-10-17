using System;

namespace SimpleStackVM
{
    public interface IValue : IComparable<IValue>
    {
        string ToString();
        string TypeName { get; }
    }
}