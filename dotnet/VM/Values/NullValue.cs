#nullable enable

namespace SimpleStackVM
{
    public struct NullValue : IValue
    {
        #region Fields
        public static readonly NullValue Value = new NullValue();
        public const string RawValueString = "null";

        public string TypeName => "null";
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

        public int CompareTo(IValue? other)
        {
            if (other == null || other is NullValue) return 0;

            return 1;
        }

        public override string ToString() => RawValueString;
        public override int GetHashCode() => -1;
        #endregion
    }
}