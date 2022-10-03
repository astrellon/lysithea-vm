using System.Diagnostics;

namespace SimpleStackVM
{
    [DebuggerDisplay("{Description}")]
    public struct CodeLine
    {
        #region Fields
        public static readonly CodeLine Empty = new CodeLine(Operator.Unknown, NullValue.Value);

        public readonly Operator Operator;
        public readonly IValue Input;

        public string Description
        {
            get
            {
                if (this.Input is StringValue stringInput)
                {
                    return $"{this.Operator} \"{stringInput.Value}\"";
                }
                return $"{this.Operator} {this.Input.ToString()}";
            }
        }
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