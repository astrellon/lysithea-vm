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
        private readonly Scope builtinScope;

        private Scope globalScope;
        private Function currentCode;
        private Scope currentScope;
        private int lineCounter;

        public bool Running;
        public bool Paused;

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize)
        {
            // this.builtinScopes = new List<IReadOnlyScope>();
            this.builtinScope = new Scope();
            this.globalScope = new Scope(this.builtinScope);
            this.currentCode = Function.Empty;
            this.currentScope = this.globalScope;
            this.lineCounter = 0;
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void AddBuiltinScope(IReadOnlyScope scope)
        {
            this.builtinScope.CombineScope(scope);
        }

        public void RemoveBuiltinScope(IReadOnlyScope scope)
        {
            // this.builtinScopes.Remove(scope);
        }

        public void SetGlobalCode(Function code)
        {
            this.currentCode = code;
        }

        public void Reset()
        {
            this.globalScope = new Scope(this.builtinScope);
            this.currentCode = Function.Empty;
            this.currentScope = this.globalScope;
            this.lineCounter = 0;
            this.stack.Clear();
            this.stackTrace.Clear();
            this.Running = false;
            this.Paused = false;
        }

        public void Step()
        {
            if (this.lineCounter >= this.currentCode.Code.Count)
            {
                if (this.stackTrace.TryPop(out var scopeFrame))
                {
                    this.currentCode = scopeFrame.Function;
                    this.currentScope = scopeFrame.Scope;
                    this.lineCounter = scopeFrame.LineCounter;
                }
                else
                {
                    this.Running = false;
                }
                return;
            }

            var codeLine = this.currentCode.Code[this.lineCounter++];

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
                            this.PushStack(codeLine.Input);
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
                        if (this.TryGetValue(value, out var foundValue))
                        {
                            this.PushStack(foundValue);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to get variable: {value.ToString()}");
                        }
                        break;
                    }
                case Operator.Define:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        this.currentScope.Define(key.ToString(), value);
                        break;
                    }
                case Operator.Set:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        if (!this.currentScope.TrySet(key.ToString(), value))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to set variable that has not been defined: {key.ToString()} = {value.ToString()}");
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
                        if (codeLine.Input == null)
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call needs a num args code line input");
                        }

                        var numArgs = ((NumberValue)codeLine.Input).IntValue;
                        var top = this.PopStack();
                        if (top is IFunctionValue procTop)
                        {
                            this.CallFunction(procTop, numArgs, true);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call needs a function to run: {top.ToString()}");
                        }
                        break;
                    }
            }
        }

        public bool TryGetValue(IValue key, out IValue value)
        {
            if (key is ArrayValue list)
            {
                this.TryGetValue(list[0], out var current);
                for (var i = 1; i < list.Count; i++)
                {
                    if (current is ObjectValue currentObject)
                    {
                        if (!currentObject.TryGetValue(list[i].ToString(), out current))
                        {
                            value = NullValue.Value;
                            return false;
                        }
                    }
                    else if (current is ArrayValue currentArray)
                    {
                        if (!currentArray.TryGet(list[i], out current))
                        {
                            value = NullValue.Value;
                            return false;
                        }
                    }
                    else
                    {
                        throw new OperatorException(this.CreateStackTrace(), $"Unable to get property from non object or array: [{key.ToString()}]: {current.ToString()}");
                    }
                }

                value = current;
                return true;
            }

            var strKey = key.ToString();

            if (this.currentScope.TryGet(strKey, out value))
            {
                return true;
            }

            value = NullValue.Value;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Jump(string label)
        {
            if (this.currentCode.Labels.TryGetValue(label, out var line))
            {
                this.lineCounter = line;
            }
            else
            {
                throw new OperatorException(this.CreateStackTrace(), $"Unable to jump to label: {label}");
            }
        }

        #region Function Methods
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

        public void CallFunction(IFunctionValue value, int numArgs, bool pushToStackTrace)
        {
            if (value is FunctionValue foundProc)
            {
                if (pushToStackTrace)
                {
                    this.PushToStackTrace(new ScopeFrame(this.currentCode, this.currentScope, this.lineCounter));
                }
                this.ExecuteFunction(foundProc.Value, numArgs);
            }
            else if (value is BuiltinFunctionValue foundBuiltin)
            {
                this.ExecuteFunction(foundBuiltin, numArgs);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteFunction(BuiltinFunctionValue handler, int numArgs)
        {
            handler.Value(this, numArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteFunction(Function function, int numArgs = -1)
        {
            this.currentCode = function;
            this.currentScope = new Scope(this.currentScope);
            this.lineCounter = 0;

            var args = this.GetArgs(numArgs >= 0 ? Math.Min(numArgs, function.Parameters.Count) : function.Parameters.Count);
            for (var i = 0; i < args.Count; i++)
            {
                var argName = function.Parameters[i];
                this.currentScope.Define(argName, args[i]);
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

            this.currentCode = scopeFrame.Function;
            this.currentScope = scopeFrame.Scope;
            this.lineCounter = scopeFrame.LineCounter;
        }

        #endregion

        #region Stack Methods
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
        #endregion

        #region Debugging Methods
        public IReadOnlyList<string> CreateStackTrace()
        {
            var result = new List<string>();

            result.Add(DebugScopeLine(this.currentCode, this.lineCounter - 1));
            for (var i = this.stackTrace.Index - 1; i >= 0; i--)
            {
                var stackFrame = this.stackTrace.Data[i];
                result.Add(DebugScopeLine(stackFrame.Function, stackFrame.LineCounter));
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

        private static string DebugScopeLine(Function function, int line)
        {
            if (line >= function.Code.Count)
            {
                return $"[{function.Name}:{line - 1}: end of code";
            }
            if (line < 0)
            {
                return $"[{function.Name}:{line - 1}: before start of code";
            }

            var codeLine = function.Code[line];
            var codeLineInput = codeLine.Input != null ? codeLine.Input.ToString() : "<empty>";
            return $"[{function.Name}]:{line - 1}:{codeLine.Operator}: [{codeLineInput}]";
        }
        #endregion

        #endregion
    }
}