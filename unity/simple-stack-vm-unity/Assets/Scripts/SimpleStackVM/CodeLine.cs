using System.Diagnostics;

#nullable enable

namespace SimpleStackVM
{
    public interface ITempCodeLine { }

    [DebuggerDisplay("{Description}")]
    public class LabelCodeLine : ITempCodeLine
    {
        public readonly string Label;

        public string Description => $"Label: {this.Label}";

        public LabelCodeLine(string label) { this.Label = label; }
    }

    [DebuggerDisplay("{Description}")]
    public class CodeLine : ITempCodeLine
    {
        #region Fields
        public static readonly CodeLine Empty = new CodeLine(Operator.Unknown, NullValue.Value);

        public readonly Operator Operator;
        public readonly IValue? Input;

        public string Description
        {
            get
            {
                if (this.Input == null)
                {
                    return this.Operator.ToString();
                }

                if (this.Input is StringValue stringInput)
                {
                    return $"{this.Operator} \"{stringInput.Value}\"";
                }
                return $"{this.Operator} {this.Input.ToString()}";
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

        #region Methods
        public override string ToString()
        {
            return $"{this.Operator}: " + (this.Input != null ? this.Input.ToString() : "<no arg>");
        }
        #endregion
    }
}