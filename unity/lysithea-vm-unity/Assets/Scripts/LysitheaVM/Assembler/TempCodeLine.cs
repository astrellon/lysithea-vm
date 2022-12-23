using System.Diagnostics;

#nullable enable

namespace LysitheaVM
{
    [DebuggerDisplay("{Description}")]
    public class TempCodeLine
    {
        #region Fields
        public readonly Operator Operator;
        public readonly string LabelLine;
        public readonly Token Token;

        public string Description
        {
            get
            {
                if (this.IsLabel)
                {
                    return $"Label: {this.Token.Location}: {this.LabelLine}";
                }
                return $"Code: {this.Token.Location}: {this.Operator}: {this.Token}";
            }
        }
        public bool IsLabel => !string.IsNullOrEmpty(this.LabelLine);
        #endregion

        #region Constructor
        public TempCodeLine(Operator op, string labelLine, Token token)
        {
            this.Operator = op;
            this.LabelLine = labelLine;
            this.Token = token;
        }
        #endregion

        #region Methods
        public static TempCodeLine Code(Operator op, Token token)
        {
            return new TempCodeLine(op, "", token);
        }

        public static TempCodeLine Label(string labelLine, Token token)
        {
            return new TempCodeLine(LysitheaVM.Operator.Unknown, labelLine, token);
        }
        #endregion
    }
}