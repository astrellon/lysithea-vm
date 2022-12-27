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
        public AssemblerException(Assembler assembler, Token token, string message) : base(message)
        {
            this.Assembler = assembler;
            this.Token = token;
        }
        #endregion
    }
}