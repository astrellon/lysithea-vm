using System;

#nullable enable

namespace LysitheaVM
{
    public class SnapshotScopeFrame
    {
        #region Fields
        public readonly int LineCounter;
        public readonly string FunctionLookupName;
        public readonly IReadOnlyScope? Scope;
        #endregion

        #region Constructor
        public SnapshotScopeFrame(int lineCounter, string functionLookupName, IReadOnlyScope? scope)
        {
            this.LineCounter = lineCounter;
            this.FunctionLookupName = functionLookupName;
            this.Scope = scope;
        }
        #endregion

        #region Methods
        public static SnapshotScopeFrame From(ScopeFrame frame, int index)
        {
            var scope = index > 0 ? LysitheaVM.Scope.Copy(frame.Scope) : null;
            return new SnapshotScopeFrame(frame.LineCounter, frame.Function.LookupName, scope);
        }
        #endregion
    }

    public class Snapshot
    {
        #region Fields
        public readonly IReadOnlyFixedStack<IValue> Stack;
        public readonly IReadOnlyFixedStack<SnapshotScopeFrame> StackTrace;
        public readonly IReadOnlyScope GlobalScope;
        public readonly string CurrentCodeLookup;
        public readonly int LineCounter;
        public readonly bool Running;
        public readonly bool Paused;
        #endregion

        #region Constructor
        public Snapshot(IReadOnlyFixedStack<IValue> stack, IReadOnlyFixedStack<SnapshotScopeFrame> stackTrace,
            IReadOnlyScope globalScope, string currentCodeLookup, int lineCounter, bool running, bool paused)
        {
            this.Stack = stack;
            this.StackTrace = stackTrace;
            this.GlobalScope = globalScope;
            this.LineCounter = lineCounter;
            this.CurrentCodeLookup = currentCodeLookup;
            this.Running = running;
            this.Paused = paused;
        }
        #endregion
    }
}