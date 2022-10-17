#nullable enable

namespace SimpleStackVM
{
    public struct VariableValue : IValue
    {
        #region Fields
        public readonly string Value;

        public bool IsLabel => this.Value.Length > 1 && this.Value[0] == ':';

        public string TypeName => "variable";
        #endregion

        #region Constructor
        public VariableValue(string value)
        {
            this.Value = string.Intern(value);
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is VariableValue otherString)
            {
                return otherString.Value == this.Value;
            }
            return false;
        }

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is VariableValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }

        public override string ToString() => this.Value;
        public override int GetHashCode() => this.Value.GetHashCode();
        #endregion
    }
}
