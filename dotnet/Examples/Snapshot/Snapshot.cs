using System;
using System.Linq;
using System.Collections.Generic;

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
        public ObjectValue ToObject()
        {
            var result = new Dictionary<string, IValue>();
            result["line"] = new NumberValue(this.LineCounter);
            result["func"] = new StringValue(this.FunctionLookupName);
            if (this.Scope != null)
            {
                result["scope"] = LysitheaVM.Scope.ToObject(this.Scope);
            }

            return new ObjectValue(result);
        }

        public static SnapshotScopeFrame FromFrame(ScopeFrame frame, int index)
        {
            var scope = index > 0 ? LysitheaVM.Scope.Copy(frame.Scope) : null;
            return new SnapshotScopeFrame(frame.LineCounter, frame.Function.LookupName, scope);
        }

        public static SnapshotScopeFrame FromObject(IObjectValue input)
        {
            if (!input.TryGetKey<NumberValue>("line", out var lineValue))
            {
                throw new InvalidOperationException("Snapshot scope frame object: Missing line number");
            }
            if (!input.TryGetKey<StringValue>("func", out var funcValue))
            {
                throw new InvalidOperationException("Snapshot scope frame object: Missing function lookup name");
            }

            IReadOnlyScope? scope = null;
            if (input.TryGetKey<IObjectValue>("scope", out var scopeValue))
            {
                scope = LysitheaVM.Scope.FromObject(scopeValue);
            }

            return new SnapshotScopeFrame(lineValue.IntValue, funcValue.Value, scope);
        }
        #endregion
    }

    public class Snapshot
    {
        #region Fields
        public readonly IReadOnlyList<IValue> Stack;
        public readonly IReadOnlyList<SnapshotScopeFrame> StackTrace;
        public readonly IReadOnlyScope GlobalScope;
        public readonly string CurrentCodeLookup;
        public readonly int LineCounter;
        public readonly bool Running;
        public readonly bool Paused;
        #endregion

        #region Constructor
        public Snapshot(IReadOnlyList<IValue> stack, IReadOnlyList<SnapshotScopeFrame> stackTrace,
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

        #region Methods
        public ObjectValue ToObject()
        {
            var stack = ArrayValue.Empty;
            if (this.Stack.Count >= 0)
            {
                stack = new ArrayValue(this.Stack);
            }
            var stackTrace = ArrayValue.Empty;
            if (this.StackTrace.Count >= 0)
            {
                stackTrace = new ArrayValue(this.StackTrace.Select(s => s.ToObject() as IValue).ToList());
            }
            var globalScope = LysitheaVM.Scope.ToObject(this.GlobalScope);

            var result = new Dictionary<string, IValue>();
            result["stack"] = stack;
            result["stackTrace"] = stackTrace;
            result["globalScope"] = globalScope;
            result["line"] = new NumberValue(this.LineCounter);
            result["func"] = new StringValue(this.CurrentCodeLookup);
            result["running"] = new BoolValue(this.Running);
            result["paused"] = new BoolValue(this.Paused);

            return new ObjectValue(result);
        }

        public void ApplyTo(VirtualMachine vm, Script script)
        {
            var currentCode = script.Code;
            var globalScope = Scope.Copy(this.GlobalScope);
            var stackTrace = this.StackTrace.Select(s =>
            {
                if (script.TryFindFunction(s.FunctionLookupName, out var func))
                {
                    var scope = s.Scope == null ? globalScope : LysitheaVM.Scope.Copy(s.Scope);
                    return new ScopeFrame(func, scope, s.LineCounter);
                }
                throw new Exception("Error parsing snapshot scope frame");
            }).ToList();

            var parentScope = globalScope;
            for (var i = 1; i < stackTrace.Count; i++)
            {
                var data = stackTrace[i];
                data.Scope.Parent = parentScope;
                parentScope = data.Scope;
            }

            if (!string.IsNullOrWhiteSpace(this.CurrentCodeLookup))
            {
                if (script.TryFindFunction(this.CurrentCodeLookup, out var code))
                {
                    currentCode = code;
                }
            }

            vm.SetExecutionContext(script, globalScope, currentCode, this.LineCounter, this.Stack, stackTrace);
            vm.Running = this.Running;
            vm.Paused = this.Paused;
        }

        public static Snapshot FromVirtualMachine(VirtualMachine vm)
        {
            var stack = vm.Stack.Copy();
            var stackTrace = vm.StackTrace.Copy(SnapshotScopeFrame.FromFrame);
            var globalScope = LysitheaVM.Scope.Copy(vm.GlobalScope);

            return new Snapshot(stack, stackTrace, globalScope, vm.CurrentCode.LookupName, vm.LineCounter, vm.Running, vm.Paused);
        }

        public static Snapshot FromObject(IObjectValue input)
        {
            IReadOnlyList<IValue> stack = new IValue[0];
            if (input.TryGetKey<ArrayValue>("stack", out var stackValue) && stackValue.ArrayValues.Count > 0)
            {
                stack = stackValue.ArrayValues;
            }

            IReadOnlyList<SnapshotScopeFrame> stackTrace = new SnapshotScopeFrame[0];
            if (input.TryGetKey<ArrayValue>("stackTrace", out var stackTraceValue) && stackTraceValue.ArrayValues.Count > 0)
            {
                stackTrace = stackTraceValue.ArrayValues.Select(s => SnapshotScopeFrame.FromObject(s as IObjectValue)).ToList();
            }

            var globalScope = Scope.Empty;
            if (input.TryGetKey<ObjectValue>("globalScope", out var globalScopeValue))
            {
                globalScope = LysitheaVM.Scope.FromObject(globalScopeValue);
            }

            var line = input.GetInt("line");
            var func = input.GetString("func");
            var running = input.GetBoolean("running");
            var paused = input.GetBoolean("paused");

            if (line == null || func == null || running == null || paused == null)
            {
                throw new InvalidOperationException("Missing data");
            }

            return new Snapshot(stack, stackTrace, globalScope, func, line.Value, running.Value, paused.Value);
        }
        #endregion
    }
}