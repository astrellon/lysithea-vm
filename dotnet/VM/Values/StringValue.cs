#nullable enable

namespace SimpleStackVM
{
    public struct StringValue : IValue
    {
        #region Fields
        public static readonly StringValue Empty = new StringValue("");
        public readonly string Value;
        public object RawValue => this.Value;
        public bool IsNull => false;
        #endregion

        #region Constructor
        public StringValue(string value)
        {
            this.Value = string.Intern(value ?? "<<null>>");
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
            return this.Value;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public StringValue Append(string input)
        {
            return new StringValue(this.Value + input);
        }

        public StringValue Prepend(string input)
        {
            return new StringValue(input + this.Value);
        }

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is StringValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }

        public static explicit operator StringValue(string input) => new StringValue(input);
        public static implicit operator string (StringValue str) => str.Value;
        #endregion
    }
}