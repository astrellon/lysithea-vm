using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace LysitheaVM
{
    public partial class VirtualMachine
    {
        public delegate T CastValueDelegate<T>(IValue input);

        #region Fields
        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<ScopeFrame> stackTrace;

        public IReadOnlyScope? BuiltinScope = null;
        public Scope GlobalScope { get; private set; }
        public Scope CurrentScope { get; private set; }
        public Script? CurrentScript { get; private set; }
        private int lineCounter = 0;
        private int returnStackSizeTo = -1;

        public bool Running;
        public bool Paused;
        public Function CurrentCode = Function.Empty;

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
        public int LineCounter => this.lineCounter;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize)
        {
            this.GlobalScope = new Scope();
            this.CurrentScope = this.GlobalScope;
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void Reset()
        {
            this.GlobalScope = new Scope();
            this.CurrentScope = this.GlobalScope;
            this.CurrentScript = null;
            this.lineCounter = 0;
            this.stack.Clear();
            this.stackTrace.Clear();
            this.Running = false;
            this.Paused = false;
        }

        public void ChangeToScript(Script script)
        {
            this.lineCounter = 0;
            this.stack.Clear();
            this.stackTrace.Clear();
            this.CurrentScript = script;

            this.BuiltinScope = script.BuiltinScope;
            this.CurrentCode = script.Code;
        }

        public void SetExecutionContext(Script script, Scope globalScope, Function code, int lineCounter, IReadOnlyList<IValue> stack, IReadOnlyList<ScopeFrame> stackTrace)
        {
            this.BuiltinScope = script.BuiltinScope;
            this.CurrentCode = code;
            this.CurrentScript = script;

            this.GlobalScope = globalScope;
            this.CurrentScope = stackTrace.Any() ? stackTrace.Last().Scope : globalScope;

            this.lineCounter = lineCounter;
            this.stack.From(stack);
            this.stackTrace.From(stackTrace);
        }

        public void Execute(Script script)
        {
            this.ChangeToScript(script);
            this.Execute();
        }

        public void Execute()
        {
            this.Running = true;
            this.Paused = false;
            while (this.Running && !this.Paused)
            {
                this.Step();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Jump(string label)
        {
            if (this.CurrentCode.Labels.TryGetValue(label, out var line))
            {
                this.lineCounter = line;
            }
            else
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to jump to label: {label}");
            }
        }

        #region Function Methods
        public ArgumentsValue GetArgs(int numArgs)
        {
            if (numArgs == 0)
            {
                return ArgumentsValue.Empty;
            }

            var hasArguments = false;
            var temp = new IValue[numArgs];
            for (var i = 0; i < numArgs; i++)
            {
                var value = this.PopStack();
                if (value is ArgumentsValue argValue)
                {
                    hasArguments = true;
                }
                temp[numArgs - i - 1] = value;
            }

            if (hasArguments)
            {
                return new ArgumentsValue(temp.SelectMany(FlattenTempArgs).ToList());
            }
            return new ArgumentsValue(temp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFunction(IFunctionValue value, int numArgs, bool pushToStackTrace)
        {
            var args = this.GetArgs(numArgs);
            value.Invoke(this, args, pushToStackTrace);
        }

        public void SwitchToFunction(Function function, ArgumentsValue args, bool pushToStackTrace = false)
        {
            if (pushToStackTrace)
            {
                this.PushToStackTrace(new ScopeFrame(this.CurrentCode, this.stack.Index, this.CurrentScope, this.lineCounter));
            }

            this.CurrentCode = function;
            this.CurrentScope = new Scope(this.CurrentScope);
            this.lineCounter = 0;

            var numCalledArgs = Math.Min(args.ArrayValues.Count, function.Parameters.Count);
            var i = 0;
            for (; i < numCalledArgs; i++)
            {
                var argName = function.Parameters[i];
                if (argName.StartsWith("..."))
                {
                    this.CurrentScope.TryDefine(argName.Substring(3), args.SubList(i));
                    i++;
                    break;
                }
                this.CurrentScope.TryDefine(argName, args[i]);
            }

            if (i < function.Parameters.Count)
            {
                var argName = function.Parameters[i];
                if (argName.StartsWith("..."))
                {
                    this.CurrentScope.TryDefine(argName.Substring(3), ArgumentsValue.Empty);
                }
                else
                {
                    throw new VirtualMachineException(this, this.CreateStackTrace(), $"Function called without enough arguments: {function.Name}");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushToStackTrace(ScopeFrame scopeFrame)
        {
            if (!this.stackTrace.TryPush(scopeFrame))
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), "Unable to call, call stack full");
            }
        }

        public bool TryReturn()
        {
            if (!this.stackTrace.TryPop(out var scopeFrame))
            {
                return false;
            }

            this.CurrentCode = scopeFrame.Function;
            this.CurrentScope = scopeFrame.Scope;
            this.lineCounter = scopeFrame.LineCounter;
            this.returnStackSizeTo = scopeFrame.StackSize;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return()
        {
            if (!this.TryReturn())
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), "Unable to return, call stack empty");
            }
        }

        private static IEnumerable<IValue> FlattenTempArgs(IValue input)
        {
            if (input is ArgumentsValue argValue)
            {
                foreach (var item in argValue.Value)
                {
                    yield return item;
                }
                yield break;
            }

            yield return input;
        }
        #endregion

        #region Stack Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IValue PopStack()
        {
            if (!this.stack.TryPop(out var obj))
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), "Unable to pop stack, empty");
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IValue PeekStack()
        {
            if (!this.stack.TryPeek(out var obj))
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), "Unable to peek stack, empty");
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushStack(IValue value)
        {
            if (!this.stack.TryPush(value))
            {
                throw new VirtualMachineException(this, this.CreateStackTrace(), "Unable to push stack, stack is full");
            }
        }
        #endregion

        #region Debugging Methods
        public IReadOnlyList<string> CreateStackTrace()
        {
            var result = new List<string>();

            result.Add(DebugScopeLine(this.CurrentCode, this.lineCounter - 1));
            for (var i = this.stackTrace.Index; i >= 0; i--)
            {
                var stackFrame = this.stackTrace.Data[i];
                result.Add(DebugScopeLine(stackFrame.Function, stackFrame.LineCounter - 1));
            }

            return result;
        }

        public IEnumerable<string> PrintStackDebug()
        {
            yield return ($"Stack size: {this.stack.StackSize}");
            for (var i = 0; i < this.stack.StackSize; i++)
            {
                var item = this.stack.Data[i];
                yield return ($"- {item?.ToString()}");
            }
        }

        private static string DebugScopeLine(Function function, int line)
        {
            var text = $"  at [{function.Name}] in {function.DebugSymbols.SourceName}";
            if (line >= function.Code.Count)
            {
                return $"{text} end of code";
            }
            if (line < 0)
            {
                return $"{text} before start of code";
            }

            function.DebugSymbols.TryGetLocation(line, out var codeLocation);

            text += ExceptionsCommon.CreateErrorLogAt(function.DebugSymbols.SourceName, codeLocation, function.DebugSymbols.FullText);
            return text;
        }
        #endregion

        #endregion
    }
}