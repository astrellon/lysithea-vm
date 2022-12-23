using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class DebugSymbols
    {
        #region Fields
        public static readonly DebugSymbols Empty = new DebugSymbols("unknown", new string[1] {""}, new CodeLocation[0]);

        public readonly string SourceName;
        public readonly IReadOnlyList<string> FullText;
        public readonly IReadOnlyList<CodeLocation> CodeLineToText;
        #endregion

        #region Constructor
        public DebugSymbols(string sourceName, IReadOnlyList<string> fullText, IReadOnlyList<CodeLocation> codeLineToText)
        {
            this.SourceName = string.IsNullOrWhiteSpace(sourceName) ? "unknown" : sourceName;
            this.FullText = fullText;
            this.CodeLineToText = codeLineToText;
        }
        #endregion

        #region Methods
        public bool TryGetLocation(int line, out CodeLocation result)
        {
            if (line >= 0 && line < this.CodeLineToText.Count)
            {
                result = this.CodeLineToText[line];
                return true;
            }

            result = CodeLocation.Empty;
            return false;
        }
        #endregion
    }
}