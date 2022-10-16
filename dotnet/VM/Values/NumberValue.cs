#nullable enable

namespace SimpleStackVM
{
    public struct NumberValue : IValue
    {
        #region Fields
        public readonly double Value;

        public int IntValue => (int)this.Value;
        public float FloatValue => (float)this.Value;

        public string TypeName => "number";
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

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is NumberValue otherNum)
            {
                return this.Value.CompareTo(otherNum.Value);
            }

            return 1;
        }

        public static explicit operator NumberValue(double input) => new NumberValue(input);
        #endregion
    }
}