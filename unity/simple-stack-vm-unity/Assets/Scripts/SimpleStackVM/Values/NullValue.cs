#nullable enable

namespace SimpleStackVM
{
    public struct NullValue : IValue
    {
        #region Fields
        public static readonly NullValue Value = new NullValue();
        public string TypeName => "null";
        #endregion

        #region Methods
        public int CompareTo(IValue? other) => (other == null || other is NullValue) ? 0 : 1;
        public override string ToString() => this.TypeName;
        #endregion
    }
}