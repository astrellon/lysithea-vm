using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ArgumentsValue : IArrayValue, IObjectValue
    {
        #region Fields
        // Static
        public static ArgumentsValue Empty = new ArgumentsValue(new IValue[0]);

        private static IReadOnlyList<string> Keys = new [] { "length" };

        // IValue
        public string TypeName => "arguments";

        // IArrayValue
        public IReadOnlyList<IValue> ArrayValues => this.Value;
        public int Length => this.Value.Count;

        // IObjectValue
        public IReadOnlyList<string> ObjectKeys => Keys;

        // Helper
        public IValue this[int index] => this.Value[index];

        // Internal
        public readonly IReadOnlyList<IValue> Value;
        #endregion

        #region Constructor
        public ArgumentsValue(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGetIndex(int index, [NotNullWhen(true)] out IValue result)
        {
            if (index >= 0 && index < this.Value.Count)
            {
                result = this.Value[index];
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public ArrayValue SubList(int index)
        {
            if (index == 0)
            {
                return new ArrayValue(this.Value);
            }

            return new ArrayValue(this.Value.Skip(index).ToList());
        }

        public override string ToString() => StandardArrayLibrary.GeneralToString(this);
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