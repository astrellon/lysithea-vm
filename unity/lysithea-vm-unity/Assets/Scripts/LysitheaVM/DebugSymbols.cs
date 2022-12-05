using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class DebugSymbols
    {
        #region Fields
        public static readonly DebugSymbols Empty = new DebugSymbols(new string[1] {""}, new CodeLocation[0]);

        public readonly IReadOnlyList<string> FullText;
        public readonly IReadOnlyList<CodeLocation> CodeLineToText;
        #endregion

        #region Constructor
        public DebugSymbols(IReadOnlyList<string> fullText, IReadOnlyList<CodeLocation> codeLineToText)
        {
            this.FullText = fullText;
            this.CodeLineToText = codeLineToText;
        }
        #endregion
    }
}