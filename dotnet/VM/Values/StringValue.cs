#nullable enable

using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public struct StringValue : IValue
    {
        #region Fields
        public static readonly StringValue Empty = new StringValue("");
        public readonly string Value;
        #endregion

        #region Constructor
        public StringValue(string value)
        {
            this.Value = string.Intern(value);
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

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is StringValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Length + index;
            }

            return index;
        }

        public static explicit operator StringValue(string input) => new StringValue(input);
        public static implicit operator string (StringValue str) => str.Value;
        #endregion
    }
}