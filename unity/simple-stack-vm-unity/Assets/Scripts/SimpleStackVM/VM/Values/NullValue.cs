#nullable enable

namespace SimpleStackVM
{
    public struct NullValue : IValue
    {
        #region Fields
        public static readonly NullValue Value = new NullValue();
        public const string RawValueString = "<<null>>";

        public object RawValue => RawValueString;
        public bool IsNull => true;
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return true;
            if (other is string str)
            {
                return str == RawValueString;
            }
            if (other is NullValue otherNull)
            {
                return true;
            }
            if (other is BoolValue boolValue)
            {
                return boolValue.Value == false;
            }
            return false;
        }

        public override string ToString()
        {
            return RawValueString;
        }

        public override int GetHashCode()
        {
            return -1;
        }
        #endregion
    }
}