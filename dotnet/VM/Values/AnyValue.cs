using System.Collections.Generic;
using System.Text;

#nullable enable

namespace SimpleStackVM
{
    public struct AnyValue : IValue
    {
        #region Fields
        public readonly object Value;
        public object RawValue => this.Value;
        public bool IsNull => this.Value != null;
        #endregion

        #region Constructor
        public AnyValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (this.Value == null)
            {
                return (other == null || other.Equals(NullValue.Value));
            }
            if (other == null) return false;
            if (other is IValue otherValue)
            {
                return this.Value.Equals(otherValue.RawValue);
            }
            return false;
        }

        public override string? ToString()
        {
            return this.Value == null ? "<<nullAny>>" : this.Value.ToString();
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #endregion
    }
}