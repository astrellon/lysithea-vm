#nullable enable

namespace SimpleStackVM
{
    public struct BoolValue : IValue
    {
        #region Fields
        public static BoolValue True = new BoolValue(true);
        public static BoolValue False = new BoolValue(false);

        public readonly bool Value;

        public string TypeName => "bool";
        #endregion

        #region Constructor
        public BoolValue(bool value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override string ToString() => this.Value ? "true" : "false";

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is BoolValue otherBool))
            {
                return 1;
            }

            return this.Value.CompareTo(otherBool.Value);
        }
        #endregion
    }
}