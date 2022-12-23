using System;

namespace LysitheaVM
{
    public class AssemblerException : Exception
    {
        #region Fields
        public readonly Token Token;
        #endregion

        #region Constructor
        public AssemblerException(Token token, string message) : base(token.Location.ToString() + ": " + token.ToString() + ": " + message)
        {
            this.Token = token;
        }
        #endregion
    }
}