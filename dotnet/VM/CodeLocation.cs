using System;
using System.Linq;

namespace LysitheaVM
{
    public struct CodeLocation
    {
        #region Fields
        public static readonly CodeLocation Empty = new CodeLocation(0, 0);

        public readonly int LineNumber;
        public readonly int ColumnNumber;
        #endregion

        #region Constructor
        public CodeLocation(int lineNumber, int columnNumber)
        {
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
        }
        #endregion

        #region Methods
        public new string ToString()
        {
            return $"{this.LineNumber}:{this.ColumnNumber}";
        }
        #endregion
    }
}