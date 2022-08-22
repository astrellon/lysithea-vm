namespace SimpleStackVM
{
    public struct StringValue : IValue
    {
        #region Fields
        public static readonly StringValue Empty = new StringValue("");
        public readonly string Value;
        public object RawValue => this.Value;
        #endregion

        #region Constructor
        public StringValue(string value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is StringValue otherString)
            {
                return otherString.Value == this.Value;
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

        public static explicit operator StringValue(string input) => new StringValue(input);
        #endregion
    }
}