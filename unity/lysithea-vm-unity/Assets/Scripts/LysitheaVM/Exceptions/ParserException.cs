using System;

namespace LysitheaVM
{
    public class ParserException : Exception
    {
        #region Fields
        public readonly CodeLocation Location;
        public readonly string Token;
        public readonly string Trace;
        #endregion

        #region Constructor
        public ParserException(CodeLocation location, string token, string trace, string message) : base($"{location}: {token}: {message}")
        {
            this.Location = location;
            this.Token = token;
            this.Trace = trace;
        }
        #endregion
    }
}