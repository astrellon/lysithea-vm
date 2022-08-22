using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public struct Scope
    {
        #region Fields
        private static IReadOnlyDictionary<string, int> EmptyLabels = new Dictionary<string, int>();

        public readonly string ScopeName;
        public readonly IReadOnlyList<CodeLine> Code;
        public readonly IReadOnlyDictionary<string, int> Labels;
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