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
        public int CompareTo(IValue? other)
        {
            if (other == null || other is NullValue) return 0;

            return 1;
        }

        public override string ToString() => RawValueString;
        #endregion
    }
}