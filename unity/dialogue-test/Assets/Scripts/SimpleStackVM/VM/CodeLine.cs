namespace SimpleStackVM
{
    public struct CodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly IValue Input;
        #endregion

        #region Constructors
        public CodeLine(Operator op, IValue input)
        {
            this.Operator = op;
            this.Input = input;
        }
        #endregion
    }
}