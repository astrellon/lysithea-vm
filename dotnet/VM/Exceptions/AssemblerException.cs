using System;

namespace LysitheaVM
{
    public class AssemblerException : Exception
    {
        #region Fields
        public readonly Assembler Assembler;
        public readonly Token Token;
        public readonly string Trace;
        #endregion

        #region Constructor
        public AssemblerException(Assembler assembler, Token token, string trace, string message) : base($"{token.Location}: {token}: {message}")
        {
            this.Assembler = assembler;
            this.Token = token;
            this.Trace = trace;
        }
        #endregion
    }
}