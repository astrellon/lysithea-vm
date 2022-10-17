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
        public override bool Equals(object? other)
        {
            if (other == null) return this.Value == false;
            if (other is BoolValue otherBoolValue)
            {
                return otherBoolValue.Value == this.Value;
            }
            return false;
        }

        public override string ToString() => this.Value ? "true" : "false";
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is BoolValue otherBoolValue)
            {
                return this.Value.CompareTo(otherBoolValue.Value);
            }

            return 1;
        }
        #endregion
    }
}