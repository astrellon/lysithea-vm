using System;
using System.Collections.Generic;

#nullable enable

namespace SimpleStackVM
{
    public class Procedure
    {
        #region Fields
        public static IReadOnlyList<CodeLine> EmptyCode = new CodeLine[0];
        public static IReadOnlyDictionary<string, int> EmptyLabels = new Dictionary<string, int>();
        public static IReadOnlyList<string> EmptyParameters = new string[0];
        public static readonly Procedure Empty = new Procedure(EmptyCode, EmptyParameters, EmptyLabels);

        public readonly IReadOnlyList<CodeLine> Code;
        public readonly IReadOnlyDictionary<string, int> Labels;
        public readonly IReadOnlyList<string> Parameters;

        public string Name = "anonymous";

        public bool IsEmpty => this.Code.Count == 0;
        #endregion

        #region Constructor
        public Procedure(IReadOnlyList<CodeLine> code, IReadOnlyList<string> parameters, IReadOnlyDictionary<string, int>? labels = null)
        {
            this.Code = code;
            this.Parameters = parameters ?? EmptyParameters;
            this.Labels = labels ?? EmptyLabels;
        }
        #endregion
    }
}