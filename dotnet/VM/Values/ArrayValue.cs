using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public struct ArrayValue : IArrayValue, IObjectValue
    {
        #region Fields
        // Static
        public static ArrayValue Empty = new ArrayValue(new IValue[0]);

        private static IReadOnlyList<string> Keys = new [] { "length" };

        // IValue
        public string TypeName => "array";

        // IArrayValue
        public IReadOnlyList<IValue> ArrayValues => this.Value;
        public int Length => this.Value.Count;

        // IObjectValue
        public IReadOnlyList<string> ObjectKeys => Keys;

        // Helper
        public IValue this[int index] => this.Value[this.CalcIndex(index)];

        // Internal
        public readonly IReadOnlyList<IValue> Value;
        #endregion

        #region Constructor
        public ArrayValue(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }
        public ArrayValue(IEnumerable<IValue> value)
        {
            this.Value = value.ToList();
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

        public override string ToString() => StandardArrayLibrary.GeneralToString(this, -1, 0);
        public string ToStringFormatted(int indent, int depth) => StandardArrayLibrary.GeneralToString(this, indent, depth);

        public int CompareTo(IValue? other) => StandardArrayLibrary.GeneralCompareTo(this, other);

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value)
        {
            if (key == "length")
            {
                value = new NumberValue(this.Length);
                return true;
            }

            value = NullValue.Value;
            return false;
        }

        #endregion
    }
}