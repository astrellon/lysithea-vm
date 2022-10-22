using System;
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

        public IReadOnlyScope? BuiltinScope = null;
        public Scope GlobalScope { get; private set; }
        public Scope CurrentScope { get; private set; }
        private int lineCounter = 0;

        public bool Running;
        public bool Paused;
        public Function CurrentCode = Function.Empty;

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
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

            this.BuiltinScope = script.BuiltinScope;
            this.CurrentCode = script.Code;
        }

        public void Execute(Script script)
        {
            this.ChangeToScript(script);

            this.Running = true;
            this.Paused = false;
            while (this.Running && !this.Paused)
            {
                this.Step();
            }
        }

        public void Step()
        {
            if (this.lineCounter >= this.CurrentCode.Code.Count)
            {
                if (!this.TryReturn())
                {
                    this.Running = false;
                }
                return;
            }

            var codeLine = this.CurrentCode.Code[this.lineCounter++];

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
                            throw new StackException(this.CreateStackTrace(), "Push needs an input");
                        }

                        break;
                    }
                case Operator.Get:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        if (!(key is StringValue stringInput))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to get variable, input needs to be a string: {key.ToString()}");
                        }

                        var keyString = key.ToString();
                        if (this.CurrentScope.TryGetKey(keyString, out var foundValue) ||
                            (this.BuiltinScope != null && this.BuiltinScope.TryGetKey(keyString, out foundValue)))
                        {
                            this.PushStack(foundValue);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to get variable: {key.ToString()}");
                        }
                        break;
                    }
                case Operator.GetProperty:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        if (!(key is ArrayValue arrayInput))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to get property, input needs to be an array: {key.ToString()}");
                        }

                        var top = this.PopStack();
                        if (ValuePropertyAccess.TryGetProperty(top, arrayInput, out var found))
                        {
                            this.PushStack(found);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to get property: {key.ToString()}");
                        }
                        break;
                    }
                case Operator.Define:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        this.CurrentScope.Define(key.ToString(), value);
                        break;
                    }
                case Operator.Set:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        if (!this.CurrentScope.TrySet(key.ToString(), value))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Unable to set variable that has not been defined: {key.ToString()} = {value.ToString()}");
                        }
                        break;
                    }
                case Operator.JumpFalse:
                    {
                        var label = codeLine.Input ?? this.PopStack();

                        var top = this.PopStack();
                        if (top.CompareTo(BoolValue.False) == 0)
                        {
                            this.Jump(label.ToString());
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        var top = this.PopStack();
                        if (top.CompareTo(BoolValue.True) == 0)
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
                        if (codeLine.Input == null || !(codeLine.Input is NumberValue numArgs))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call needs a num args code line input");
                        }

                        var top = this.PopStack();
                        if (top is IFunctionValue procTop)
                        {
                            this.CallFunction(procTop, numArgs.IntValue, true);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call needs a function to run: {top.ToString()}");
                        }
                        break;
                    }
                case Operator.CallDirect:
                    {
                        if (codeLine.Input == null || !(codeLine.Input is ArrayValue arrayInput) ||
                           !(arrayInput[0] is IFunctionValue procTop) || !(arrayInput[1] is NumberValue numArgs))
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Call direct needs an array of the function and num args code line input");
                        }

                        this.CallFunction(procTop, numArgs.IntValue, true);
                        break;
                    }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFunction(IFunctionValue value, int numArgs, bool pushToStackTrace)
        {
            value.Invoke(this, numArgs, pushToStackTrace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteFunction(Function function, int numArgs = -1, bool pushToStackTrace = false)
        {
            if (pushToStackTrace)
            {
                this.PushToStackTrace(new ScopeFrame(this.CurrentCode, this.CurrentScope, this.lineCounter));
            }

            this.CurrentCode = function;
            this.CurrentScope = new Scope(this.CurrentScope);
            this.lineCounter = 0;

            var args = this.GetArgs(numArgs >= 0 ? Math.Min(numArgs, function.Parameters.Count) : function.Parameters.Count);
            for (var i = 0; i < args.Length; i++)
            {
                var argName = function.Parameters[i];
                this.CurrentScope.Define(argName, args[i]);
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

        public bool TryReturn()
        {
            if (!this.stackTrace.TryPop(out var scopeFrame))
            {
                return false;
            }

            this.CurrentCode = scopeFrame.Function;
            this.CurrentScope = scopeFrame.Scope;
            this.lineCounter = scopeFrame.LineCounter;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return()
        {
            if (!this.TryReturn())
            {
                throw new StackException(this.CreateStackTrace(), "Unable to return, call stack empty");
            }
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

        public delegate T CastValueDelegate<T>(IValue input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopStack<T>(CastValueDelegate<T> castFunc) where T : IValue
        {
            var obj = this.PopStack();
            try
            {
                return castFunc(obj);
            }
            catch (Exception exp)
            {
                throw new StackException(this.CreateStackTrace(), $"Unable to pop stack, type cast error: {exp.Message}");
            }
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

            result.Add(DebugScopeLine(this.CurrentCode, this.lineCounter - 1));
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