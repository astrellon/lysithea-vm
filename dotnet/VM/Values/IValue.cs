using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IValue : IComparable<IValue>
    {
        string TypeName { get; }

        string ToString();
    }

    public interface IObjectValue : IValue
    {
        IReadOnlyList<string> ObjectKeys { get; }

        bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value);
    }

    public interface IArrayValue : IValue
    {
        IReadOnlyList<IValue> ArrayValues { get; }

        bool TryGetIndex(int index, [NotNullWhen(true)] out IValue? result);
    }

    public interface IFunctionValue : IValue
    {
        void Invoke(VirtualMachine vm, ArgumentsValue args, bool pushToStackTrace);
    }
}