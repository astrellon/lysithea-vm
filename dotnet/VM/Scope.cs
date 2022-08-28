using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public struct Scope
    {
        #region Fields
        private static IReadOnlyList<CodeLine> EmptyCode = new CodeLine[0];
        private static IReadOnlyDictionary<string, int> EmptyLabels = new Dictionary<string, int>();
        public static readonly Scope Empty = new Scope("<<empty>>", EmptyCode, EmptyLabels);

        public readonly string ScopeName;
        public readonly IReadOnlyList<CodeLine> Code = EmptyCode;
        public readonly IReadOnlyDictionary<string, int> Labels = EmptyLabels;

        public bool IsEmpty => this.Code.Count == 0;
        #endregion

        #region Constructor
        public Scope(string scopeName, IReadOnlyList<CodeLine> code, IReadOnlyDictionary<string, int>? labels = null)
        {
            this.ScopeName = scopeName;
            this.Code = code;
            this.Labels = labels ?? EmptyLabels;
        }
        #endregion
    }
}