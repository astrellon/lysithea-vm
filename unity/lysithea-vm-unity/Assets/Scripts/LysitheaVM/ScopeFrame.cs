namespace LysitheaVM
{
    public struct ScopeFrame
    {
        #region Fields
        public readonly int LineCounter;
        public readonly int StackSize;
        public readonly Function Function;
        public readonly Scope Scope;
        #endregion

        #region Constructor
        public ScopeFrame(Function function, int stackSize, Scope scope, int lineCounter = 0)
        {
            this.LineCounter = lineCounter;
            this.StackSize = stackSize;
            this.Function = function;
            this.Scope = scope;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"[{this.Function.Name}]:{this.LineCounter}";
        }
        #endregion
    }
}