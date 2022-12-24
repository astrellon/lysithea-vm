using System;

namespace LysitheaVM
{
    public class AssemblerException : Exception
    {
        #region Fields
        public readonly Assembler Assembler;
        public readonly Token Token;
        #endregion

        #region Constructor
        public AssemblerException(Assembler assembler, Token token, string message) : base(token.Location.ToString() + ": " + token.ToString() + ": " + message)
        {
            this.Assembler = assembler;
            this.Token = token;
        }
        #endregion
    }
}