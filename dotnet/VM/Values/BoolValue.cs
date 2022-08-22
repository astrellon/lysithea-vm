namespace SimpleStackVM
{
    public struct BoolValue : IValue
    {
        #region Fields
        public readonly bool Value;
        public object RawValue => this.Value;
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
            if (other == null) return false;
            if (other is BoolValue otherBool)
            {
                return otherBool.Value == this.Value;
            }
            return false;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static explicit operator BoolValue(bool input) => new BoolValue(input);
        #endregion
    }
}