namespace SimpleStackVM
{
    public struct NumberValue : IValue
    {
        #region Fields
        public readonly double Value;
        public object RawValue => this.Value;
        #endregion

        #region Constructor
        public NumberValue(double value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is NumberValue otherNum)
            {
                return otherNum.Value == this.Value;
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

        public static explicit operator NumberValue(double input) => new NumberValue(input);
        #endregion
    }
}