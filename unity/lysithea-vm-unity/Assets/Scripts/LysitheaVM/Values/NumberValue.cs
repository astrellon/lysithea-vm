#nullable enable

namespace LysitheaVM
{
    public struct NumberValue : IValue
    {
        #region Fields
        public const long MaxSafeInteger = 9007199254740991L;
        public const long MinSafeInteger = -9007199254740991L;

        public readonly double Value;

        public int IntValue => (int)this.Value;
        public float FloatValue => (float)this.Value;
        public long LongValue => (long)this.Value;

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
        public string ToStringSerialise() => this.ToString();

        public static bool IsSafeInteger(double input)
        {
            return input < MaxSafeInteger && input > MinSafeInteger;
        }

        public static IValue Create(int input)
        {
            return new NumberValue(input);
        }

        public static IValue Create(float input)
        {
            return new NumberValue(input);
        }

        public static IValue Create(double input)
        {
            return new NumberValue(input);
        }

        public static IValue CreateSafeValue(long input)
        {
            if (input < MaxSafeInteger && input > MinSafeInteger)
            {
                return new NumberValue((double)input);
            }

            return new StringValue(input.ToString());
        }

        public static long GetSafeValue(IValue input)
        {
            if (input is NumberValue numberValue)
            {
                return numberValue.LongValue;
            }
            else if (input is StringValue stringValue)
            {
                return long.Parse(stringValue.Value);
            }

            throw new System.Exception("Invalid number");
        }
        #endregion
    }
}