using System.Diagnostics;

#nullable enable

namespace LysitheaVM
{
    [DebuggerDisplay("{Description}")]
    public class CodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly IValue? Input;

        public string Description
        {
            get
            {
                if (this.Input == null)
                {
                    return $"{this.Operator}";
                }

                if (this.Input is StringValue stringInput)
                {
                    return $"{this.Operator}: \"{stringInput.Value}\"";
                }
                return $"{this.Operator}: {this.Input.ToString()}";
            }
        }
        #endregion

        #region Constructors
        public CodeLine(Operator op, IValue? input)
        {
            this.Operator = op;
            this.Input = input;
        }
        #endregion
    }
}