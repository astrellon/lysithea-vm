using System.Diagnostics;

#nullable enable

namespace LysitheaVM
{
    public interface ITempCodeLine
    {
        Token Token { get; }
    }

    [DebuggerDisplay("{Description}")]
    public class LabelCodeLine : ITempCodeLine
    {
        #region Fields
        public readonly string Label;
        public readonly Token Token;

        Token ITempCodeLine.Token => this.Token;

        public string Description => $"Label: {this.Label}";
        #endregion

        #region Constructor
        public LabelCodeLine(string label, Token token)
        {
            this.Label = label;
            this.Token = token;
        }
        #endregion
    }

    [DebuggerDisplay("{Description}")]
    public class TempCodeLine : ITempCodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly Token Token;

        Token ITempCodeLine.Token => this.Token;

        public string Description
        {
            get
            {
                return $"Temp: {this.Operator}: {this.Token.ToString()}";
            }
        }
        #endregion

        #region Constructor
        public TempCodeLine(Operator op, Token token)
        {
            this.Operator = op;
            this.Token = token;
        }
        #endregion
    }
}