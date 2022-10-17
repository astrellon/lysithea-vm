#nullable enable

using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public struct StringValue : IValue
    {
        #region Fields
        public readonly string Value;

        public string TypeName => "string";
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

        public override string ToString() => this.Value;
        public override int GetHashCode() => this.Value.GetHashCode();
        #endregion
    }
}