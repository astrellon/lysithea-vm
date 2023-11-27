using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace LysitheaVM
{
    public class Function
    {
        #region Fields
        public static readonly IReadOnlyList<CodeLine> EmptyCode = new CodeLine[0];
        public static readonly IReadOnlyDictionary<string, int> EmptyLabels = new Dictionary<string, int>();
        public static readonly IReadOnlyList<string> EmptyParameters = new string[0];
        public static readonly Function Empty = new Function(EmptyCode, EmptyParameters, EmptyLabels, "", DebugSymbols.Empty, "");

        public readonly IReadOnlyList<CodeLine> Code;
        public readonly IReadOnlyDictionary<string, int> Labels;
        public readonly IReadOnlyList<string> Parameters;
        public readonly string Name;
        public readonly DebugSymbols DebugSymbols;
        public readonly bool HasName;
        public readonly string LookupName;

        public bool IsEmpty => this.Code.Count == 0;

        public bool HasReturn => this.Code.Any(c => c.Operator == Operator.Return);
        #endregion

        #region Constructor
        public Function(IReadOnlyList<CodeLine> code, IReadOnlyList<string> parameters, IReadOnlyDictionary<string, int>? labels, string name, DebugSymbols debugSymbols, string lookupName)
        {
            this.Code = code;
            this.Parameters = parameters;
            this.Labels = labels ?? EmptyLabels;
            this.Name = name.Length > 0 ? name : "anonymous";
            this.DebugSymbols = debugSymbols;
            this.LookupName = lookupName;

            this.HasName = name.Length > 0;
        }
        #endregion
    }
}