using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ArgumentsValue : IArrayValue
    {
        #region Fields
        // IValue
        public string TypeName => "arguments";

        // IArrayValue
        public IReadOnlyList<IValue> ArrayValues => this.Value;
        public int Length => this.Value.Count;

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
        public bool TryGet(int index, [NotNullWhen(true)] out IValue result)
        {
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
        #endregion
    }
}