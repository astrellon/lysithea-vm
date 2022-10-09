using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public class ScopeFrame
    {
        #region Fields
        public int LineCounter;
        public readonly Function Function;
        public readonly Scope Scope;

        public bool HasMoreCode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return !this.Function.IsEmpty && this.LineCounter < this.Function.Code.Count;
            }
        }
        #endregion

        #region Constructor
        public ScopeFrame()
        {
            this.Function = Function.Empty;
            this.Scope = new Scope();
            this.LineCounter = 0;
        }

        public ScopeFrame(Function function, Scope scope, int lineCounter = 0)
        {
            this.LineCounter = lineCounter;
            this.Function = function;
            this.Scope = scope;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{this.LineCounter}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNextCodeLine(out CodeLine result)
        {
            if (this.HasMoreCode)
            {
                result = this.Function.Code[this.LineCounter++];
                return true;
            }

            result = CodeLine.Empty;
            return false;
        }

        public bool TryGetLabel(string label, out int line)
        {
            return this.Function.Labels.TryGetValue(label, out line);
        }
        #endregion
    }
}