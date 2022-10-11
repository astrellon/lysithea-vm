#nullable enable

namespace SimpleStackVM
{
    public class SymbolValue : IValue
    {
        #region Fields
        public static readonly SymbolValue Empty = new SymbolValue("");
        public readonly string Value;

        public bool IsLabel => this.Value.Length > 0 && this.Value[0] == ':';
        #endregion

        #region Constructor
        public SymbolValue(string value)
        {
            this.Value = string.Intern(value);
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is SymbolValue otherString)
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
            if (other is SymbolValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }
        #endregion
    }
}
