using System.Collections.Generic;

#nullable enable

namespace SimpleStackVM
{
    public class AnyValue : IValue
    {
        #region Fields
        public readonly object Value;
        #endregion

        #region Constructor
        public AnyValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }

        public AnyValue(object rawValue)
        {
            this.Value = rawValue;
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
            if (other is AnyValue otherValue)
            {
                return this.Value.Equals(otherValue.Value);
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

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is AnyValue otherAny)
            {
                return this.Value == otherAny.Value ? 0 : 1;
            }

            return 1;
        }
        #endregion
    }
}