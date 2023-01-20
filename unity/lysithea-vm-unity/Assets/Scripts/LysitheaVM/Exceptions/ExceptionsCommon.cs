using System;
using System.Linq;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class ExceptionsCommon
    {
        #region Methods
        public static string CreateErrorLogAt(string sourceName, CodeLocation location, IReadOnlyList<string> fullText)
        {
            var text = $"{sourceName}:{location.StartLineNumber + 1}:{location.StartColumnNumber + 1}\n";

            var fromLineIndex = Math.Max(0, location.StartLineNumber - 1);
            var toLineIndex = Math.Min(fullText.Count, location.StartLineNumber + 2);
            for (var i = fromLineIndex; i < toLineIndex; i++)
            {
                var lineNum = (i + 1).ToString();
                text += $"{lineNum}: {fullText[i]}\n";
                if (i == location.StartLineNumber)
                {
                    text += new String(' ', location.StartColumnNumber + lineNum.Length + 1) + '^';
                    var diff = location.EndColumnNumber - location.StartColumnNumber;
                    if (location.EndLineNumber > location.StartLineNumber)
                    {
                        text += new String('-', fullText[i].Length - location.StartColumnNumber) + '^';
                    }
                    else if (diff > 0)
                    {
                        text += new String('-', diff - 1) + '^';
                    }
                    text += '\n';
                }
            }

            return text;
        }
        #endregion
    }
}