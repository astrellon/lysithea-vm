using System;
using System.Linq;

namespace SimpleStackVM
{
    public class ScopeFrame
    {
        #region Fields
        public int LineCounter;
        public readonly Procedure Procedure;
        public readonly Scope Scope;

        public bool HasMoreCode => !this.Procedure.IsEmpty && this.LineCounter < this.Procedure.Code.Count;
        #endregion

        #region Constructor
        public ScopeFrame()
        {
            this.Procedure = Procedure.Empty;
            this.Scope = new Scope();
            this.LineCounter = 0;
        }

        public ScopeFrame(Procedure procedure, Scope scope, int lineCounter = 0)
        {
            this.LineCounter = lineCounter;
            this.Procedure = procedure;
            this.Scope = scope;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{this.Procedure.Name}:{this.LineCounter}";
        }

        public bool TryGetNextCodeLine(out CodeLine result)
        {
            if (this.HasMoreCode)
            {
                result = this.Procedure.Code[this.LineCounter++];
                return true;
            }

            result = CodeLine.Empty;
            return false;
        }

        public bool TryGetLabel(string label, out int line)
        {
            return this.Procedure.Labels.TryGetValue(label, out line);
        }
        #endregion
    }
}