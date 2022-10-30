using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ArrayValue : IArrayValue, IObjectValue
    {
        #region Fields
        // Static
        public static ArrayValue Empty = new ArrayValue(new IValue[0]);
        public static ArrayValue EmptyArgs = new ArrayValue(new IValue[0], true);

        private static IReadOnlyList<string> Keys = new [] { "length" };

        // IValue
        public string TypeName => this.IsArgumentArray ? "arguments" : "array";

        // IArrayValue
        public IReadOnlyList<IValue> ArrayValues => this.Value;
        public int Length => this.Value.Count;

        // IObjectValue
        public IReadOnlyList<string> ObjectKeys => Keys;

        // Helper
        public IValue this[int index] => this.Value[this.CalcIndex(index)];

        // Internal
        public readonly IReadOnlyList<IValue> Value;
        public readonly bool IsArgumentArray;
        #endregion

        #region Constructor
        public ArrayValue(IReadOnlyList<IValue> value, bool isArgumentArray = false)
        {
            this.Value = value;
            this.IsArgumentArray = isArgumentArray;
        }
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalcIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Count + index;
            }

            return index;
        }

        public bool TryGetIndex(int index, [NotNullWhen(true)] out IValue result)
        {
            index = this.CalcIndex(index);
            if (index >= 0 && index < this.Value.Count)
            {
                result = this.Value[index];
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public override string ToString() => StandardArrayLibrary.GeneralToString(this);
        public int CompareTo(IValue? other) => StandardArrayLibrary.GeneralCompareTo(this, other);
        public void GetLength(VirtualMachine vm, ArrayValue args) => vm.PushStack(this.Value.Count);

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value)
        {
            if (key == "length")
            {
                value = new BuiltinFunctionValue(this.GetLength);
                return true;
            }

            value = NullValue.Value;
            return false;
        }

        #endregion
    }
}