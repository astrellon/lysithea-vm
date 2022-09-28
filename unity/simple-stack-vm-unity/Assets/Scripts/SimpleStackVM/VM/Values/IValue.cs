using System;

namespace SimpleStackVM
{
    public interface IValue : IComparable<IValue>
    {
        bool IsNull { get; }
        object RawValue { get; }
        string ToString();

    }
}