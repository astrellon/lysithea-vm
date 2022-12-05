using System.Diagnostics;

#nullable enable

namespace LysitheaVM
{
    public interface ITempCodeLine { }

    [DebuggerDisplay("{Description}")]
    public class LabelCodeLine : ITempCodeLine
    {
        #region Fields
        public readonly string Label;

        public string Description => $"Label: {this.Label}";
        #endregion

        #region Constructor
        public LabelCodeLine(string label)
        {
            this.Label = label;
        }
        #endregion
    }

    [DebuggerDisplay("{Description}")]
    public class TempCodeLine : ITempCodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly IToken Token;

        public string Description
        {
            get
            {
                return $"Temp: {this.Operator}: {this.Token.ToString()}";
            }
        }
        #endregion

        #region Constructor
        public TempCodeLine(Operator op, IToken token)
        {
            this.Operator = op;
            this.Token = token;
        }
        #endregion
    }
}