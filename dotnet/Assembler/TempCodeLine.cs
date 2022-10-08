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
    public class TempCodeLine : ITempCodeLine
    {
        public readonly Operator Operator;
        public readonly IValue? Argument;

        public string Description
        {
            get
            {
                if (this.Argument is StringValue stringInput)
                {
                    return $"{this.Operator} \"{stringInput.Value}\"";
                }
                if (this.Argument != null)
                {
                    return $"{this.Operator} {this.Argument.ToString()}";
                }

                return $"{this.Operator} <no arg>";
            }
        }

        public TempCodeLine(Operator op, IValue? argument)
        {
            this.Operator = op;
            if (argument != null && argument is SymbolValue symbol)
            {
                this.Argument = new StringValue(symbol.Value);
            }
            else
            {
                this.Argument = argument;
            }
        }
    }
}