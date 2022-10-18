using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public interface IValue : IComparable<IValue>
    {
        string TypeName { get; }

        string ToString();
    }

    public interface IObjectValue : IValue
    {
        IEnumerable<KeyValuePair<string, IValue>> ObjectValues { get; }
        int ObjectLength { get; }

        bool TryGetValue(string key, [NotNullWhen(true)] out IValue? value);
    }

    public interface IArrayValue : IValue
    {
        IEnumerable<IValue> ArrayValues { get; }
        int ArrayLength { get; }

        bool TryGet(int index, [NotNullWhen(true)] out IValue? result);
    }
}