using System;
using System.Linq;

namespace LysitheaVM
{
    public struct CodeLocation
    {
        #region Fields
        public static readonly CodeLocation Empty = new CodeLocation(0, 0);

        public readonly int StartLineNumber;
        public readonly int EndLineNumber;
        public readonly int StartColumnNumber;
        public readonly int EndColumnNumber;
        #endregion

        #region Constructor
        public CodeLocation(int lineNumber, int columnNumber) : this(lineNumber, columnNumber, lineNumber, columnNumber)
        {

        }
        public CodeLocation(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
        {
            this.StartLineNumber = startLineNumber;
            this.StartColumnNumber = startColumnNumber;
            this.EndLineNumber = endLineNumber;
            this.EndColumnNumber = endColumnNumber;
        }
        #endregion

        #region Methods
        public new string ToString()
        {
            return $"{this.StartLineNumber}:{this.StartColumnNumber} -> {this.EndLineNumber}:{this.EndColumnNumber}";
        }
        #endregion
    }
}