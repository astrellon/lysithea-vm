using System.Collections.Generic;

#nullable enable

namespace LysitheaVM
{
    public class Function
    {
        #region Fields
        public static IReadOnlyList<CodeLine> EmptyCode = new CodeLine[0];
        public static IReadOnlyDictionary<string, int> EmptyLabels = new Dictionary<string, int>();
        public static IReadOnlyList<string> EmptyParameters = new string[0];
        public static readonly Function Empty = new Function(EmptyCode, EmptyParameters, EmptyLabels, "");

        public readonly IReadOnlyList<CodeLine> Code;
        public readonly IReadOnlyDictionary<string, int> Labels;
        public readonly IReadOnlyList<string> Parameters;
        public readonly string Name;
        public readonly bool HasName;

        public bool IsEmpty => this.Code.Count == 0;
        #endregion

        #region Constructor
        public Function(IReadOnlyList<CodeLine> code, IReadOnlyList<string> parameters, IReadOnlyDictionary<string, int>? labels, string name)
        {
            this.Code = code;
            this.Parameters = parameters ?? EmptyParameters;
            this.Labels = labels ?? EmptyLabels;
            this.Name = name.Length > 0 ? name : "anonymous";
            this.HasName = name.Length > 0;
        }
        #endregion
    }
}