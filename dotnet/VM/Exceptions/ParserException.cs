using System;

namespace LysitheaVM
{
    public class ParserException : Exception
    {
        #region Fields
        public readonly CodeLocation Location;
        public readonly string Token;
        #endregion

        #region Constructor
        public ParserException(CodeLocation location, string token, string message) : base(location.ToString() + ": " + message)
        {
            this.Location = location;
            this.Token = token;
        }
        #endregion
    }
}