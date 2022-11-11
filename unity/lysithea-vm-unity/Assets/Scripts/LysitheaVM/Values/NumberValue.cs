#nullable enable

namespace LysitheaVM
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
        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is NumberValue otherNum)
            {
                return this.Value.CompareTo(otherNum.Value);
            }

            return 1;
        }

        public override string ToString() => this.Value.ToString();
        #endregion
    }
}