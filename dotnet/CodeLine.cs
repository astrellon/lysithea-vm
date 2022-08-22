namespace SimpleStackVM
{
    public struct CodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly IValue Input;
        #endregion

        #region Constructors
        public CodeLine(Operator op, IValue? input = null)
        {
            this.Operator = op;
            this.Input = input ?? NullValue.Value;
        }

        public CodeLine(Operator op, bool input)
        {
            this.Operator = op;
            this.Input = new BoolValue(input);
        }

        public CodeLine(Operator op, string input)
        {
            this.Operator = op;
            this.Input = new StringValue(input);
        }

        public CodeLine(Operator op, double input)
        {
            this.Operator = op;
            this.Input = new NumberValue(input);
        }
        #endregion
    }
}