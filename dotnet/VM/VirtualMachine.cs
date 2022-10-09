using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public class VirtualMachine
    {

        #region Fields
        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<ScopeFrame> stackTrace;
        private readonly List<IReadOnlyScope> builtinScopes;
        private Scope globalScope;

        public ScopeFrame CurrentFrame { get; private set; }
        public bool Running;
        public bool Paused;

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize)
        {
            this.builtinScopes = new List<IReadOnlyScope>();
            this.globalScope = new Scope();
            this.CurrentFrame = new ScopeFrame(Procedure.Empty, this.globalScope);
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void AddBuiltinScope(IReadOnlyScope scope)
        {
            this.builtinScopes.Add(scope);
        }

        public void SetGlobalCode(Procedure code)
        {
            this.CurrentFrame = new ScopeFrame(code, this.globalScope);
        }

        public void Reset()
        {
            this.globalScope = new Scope();
            this.CurrentFrame = new ScopeFrame(Procedure.Empty, this.globalScope);
            this.stack.Clear();
            this.stackTrace.Clear();
            this.Running = false;
            this.Paused = false;
        }

        public void Step()
        {
            if (!this.CurrentFrame.TryGetNextCodeLine(out var codeLine))
            {
                this.Running = false;
                return;
            }

            // this.PrintStackDebug();

            switch (codeLine.Operator)
            {
                default:
                    {
                        throw new UnknownOperatorException(this.CreateStackTrace(), $"Unknown operator: {codeLine.Operator}");
                    }
                case Operator.Push:
                    {
                        if (codeLine.Input != null)
                        {
                            if (!this.stack.TryPush(codeLine.Input))
                            {
                                throw new StackException(this.CreateStackTrace(), "Stack is full!");
                            }
                        }
                        else if (this.Stack.TryPeek(out var topOfStack))
                        {
                            if (!this.stack.TryPush(topOfStack))
                            {
                                throw new StackException(this.CreateStackTrace(), "Stack is full!");
                            }
                        }
                        else
                        {
                            throw new StackException(this.CreateStackTrace(), "Unable to copy top of an empty stack");
                        }

                        break;
                    }
                case Operator.Get:
                    {
                        var value = codeLine.Input ?? this.PopStack();
                        if (this.TryGetValue(value.ToString(), out var foundValue))
                        {
                            this.PushStack(foundValue);
                        }
                        else
                        {
                            throw new Exception($"Unable to get variable: {value.ToString()}");
                        }
                        break;
                    }
                case Operator.Define:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        this.CurrentFrame.Scope.Define(key.ToString(), value);
                        break;
                    }
                case Operator.Set:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        if (!this.CurrentFrame.Scope.TrySet(key.ToString(), value))
                        {
                            throw new Exception($"Unable to set variable that has not been defined: {key.ToString()} = {value.ToString()}");
                        }
                        break;
                    }
                case Operator.JumpFalse:
                    {
                        var label = codeLine.Input ?? this.PopStack();

                        var top = this.PopStack();
                        if (top.Equals(BoolValue.False))
                        {
                            this.Jump(label.ToString());
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        var top = this.PopStack();
                        if (top.Equals(BoolValue.True))
                        {
                            this.Jump(label.ToString());
                        }
                        break;
                    }
                case Operator.Jump:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        this.Jump(label.ToString());
                        break;
                    }
                case Operator.Return:
                    {
                        this.Return();
                        break;
                    }
                case Operator.Call:
                    {
                        var top = codeLine.Input ?? this.PopStack();
                        if (top is IProcedureValue procTop)
                        {
                            this.CallProcedure(procTop, true);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call needs a procedure to run: {top.ToString()}");
                        }
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string key, out IValue value)
        {
            if (this.CurrentFrame.Scope.TryGet(key, out value))
            {
                return true;
            }

            foreach (var scope in this.builtinScopes)
            {
                if (scope.TryGet(key, out value))
                {
                    return true;
                }
            }

            value = NullValue.Value;
            return false;
        }

        public ArrayValue GetArgs(int numArgs)
        {
            var args = ArrayValue.Empty;
            if (numArgs > 0)
            {
                var temp = new IValue[numArgs];
                for (var i = 0; i < numArgs; i++)
                {
                    temp[numArgs - i - 1] = this.PopStack();
                }
                args = new ArrayValue(temp);
            }
            return args;
        }

        public void CallProcedure(IProcedureValue value, bool pushToStackTrace)
        {
            if (value is ProcedureValue foundProc)
            {
                if (pushToStackTrace)
                {
                    this.PushToStackTrace(this.CurrentFrame);
                }
                this.ExecuteProcedure(foundProc.Value);
            }
            else if (value is BuiltinProcedureValue foundBuiltin)
            {
                this.ExecuteProcedure(foundBuiltin);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteProcedure(BuiltinProcedureValue handler)
        {
            handler.Value.Invoke(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteProcedure(Procedure procedure)
        {
            this.CurrentFrame = new ScopeFrame(procedure, new Scope(this.CurrentFrame.Scope));

            var args = this.GetArgs(procedure.Parameters.Count);
            for (var i = 0; i < args.Count; i++)
            {
                var argName = procedure.Parameters[i];
                this.CurrentFrame.Scope.Define(argName, args[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushToStackTrace(ScopeFrame scopeFrame)
        {
            if (!this.stackTrace.TryPush(scopeFrame))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to call, call stack full");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return()
        {
            if (!this.stackTrace.TryPop(out var scopeFrame))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to return, call stack empty");
            }

            this.CurrentFrame = scopeFrame;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Jump(string label)
        {
            if (this.CurrentFrame.TryGetLabel(label, out var line))
            {
                this.CurrentFrame.LineCounter = line;
            }
            else
            {
                throw new OperatorException(this.CreateStackTrace(), $"Unable to jump to label: {label}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IValue PopStack()
        {
            if (!this.stack.TryPop(out var obj))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to pop stack, empty");
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopStack<T>() where T : IValue
        {
            var obj = this.PopStack();
            if (obj.GetType() == typeof(T))
            {
                return (T)obj;
            }

            throw new StackException(this.CreateStackTrace(), $"Unable to pop stack, type cast error: wanted {typeof(T).FullName} and got {obj.GetType().FullName}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IValue PeekStack()
        {
            if (!this.stack.TryPeek(out var obj))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to peek stack, empty");
            }

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PeekStack<T>() where T : IValue
        {
            var obj = this.PeekStack();
            if (obj.GetType() == typeof(T))
            {
                return (T)obj;
            }

            throw new StackException(this.CreateStackTrace(), "Unable to peek stack, type cast error");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushStack(IValue value)
        {
            if (!this.stack.TryPush(value))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to push stack, stack is full");
            }
        }

        public IReadOnlyList<string> CreateStackTrace()
        {
            var result = new List<string>();

            result.Add(DebugScopeLine(this.CurrentFrame.Procedure, this.CurrentFrame.LineCounter - 1));
            for (var i = this.stackTrace.Index - 1; i >= 0; i--)
            {
                var stackFrame = this.stackTrace.Data[i];
                result.Add(DebugScopeLine(stackFrame.Procedure, stackFrame.LineCounter));
            }

            return result;
        }

        public IEnumerable<string> PrintStackDebug()
        {
            yield return ($"Stack size: {this.stack.StackSize}");
            for (var i = 0; i < this.stack.StackSize; i++)
            {
                var item = this.stack.Data[i];
                yield return ($"- {item.ToString()}");
            }
        }

        private static string DebugScopeLine(Procedure procedure, int line)
        {
            if (line >= procedure.Code.Count)
            {
                return $"[{procedure.Name}:{line - 1}: end of code";
            }
            if (line < 0)
            {
                return $"[{procedure.Name}:{line - 1}: before start of code";
            }

            var codeLine = procedure.Code[line];
            var codeLineInput = codeLine.Input != null ? codeLine.Input.ToString() : "<empty>";
            return $"[{procedure.Name}]:{line - 1}:{codeLine.Operator}: [{codeLineInput}]";
        }
        #endregion
    }
}