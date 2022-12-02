using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class DebugSymbols
    {
        #region Fields
        public static readonly DebugSymbols Empty = new DebugSymbols("", new int[0]);

        public readonly string FullText;
        public readonly IReadOnlyList<int> CodeLineToText;
        #endregion

        #region Constructor
        public DebugSymbols(string fullText, IReadOnlyList<int> codeLineToText)
        {
            this.FullText = fullText;
            this.CodeLineToText = codeLineToText;
        }
        #endregion
    }
}