using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    using RunCommandHandler = Action<string, VirtualMachineMini>;

    public class VirtualMachineMini
    {
        #region Fields
        private static readonly RunCommandHandler EmptyHandler = (i, vm) => { };

        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<int> stackTrace;
        private RunCommandHandler globalRunHandler;

        public Scope CurrentScope { get; private set; } = Scope.Empty;
        public int ProgramCounter { get; private set; }
        public bool Running;

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        #endregion

        #region Constructor
        public VirtualMachineMini(int stackSize, RunCommandHandler? globalRunHandler = null)
        {
            this.globalRunHandler = globalRunHandler ?? EmptyHandler;
            this.ProgramCounter = 0;
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<int>(stackSize);
        }
        #endregion

        #region Methods
        public void SetGlobalRunHandler(RunCommandHandler runCommandHandler)
        {
            this.globalRunHandler = runCommandHandler;
        }

        public void Reset()
        {
            this.ProgramCounter = 0;
            this.stack.Clear();
            this.Running = false;
        }

        public void SetCurrentScope(Scope scope)
        {
            this.CurrentScope = scope;
        }

        public void Step()
        {
            if (this.CurrentScope.IsEmpty || this.ProgramCounter >= this.CurrentScope.Code.Count)
            {
                this.Running = false;
                return;
            }

            // this.PrintStackDebug();

            var codeLine = this.CurrentScope.Code[this.ProgramCounter++];

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
                case Operator.Swap:
                    {
                        var value = codeLine.Input ?? this.PopStack();
                        if (value is NumberValue number)
                        {
                            this.Swap(number.IntValue);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Swap operator needs a number value");
                        }
                        break;
                    }
                case Operator.Copy:
                    {
                        var value = codeLine.Input ?? this.PopStack();
                        if (value is NumberValue number)
                        {
                            this.Copy(number.IntValue);
                        }
                        else
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Swap operator needs a number value");
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
                case Operator.Call:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        this.CallToLabel(label.ToString());
                        break;
                    }
                case Operator.Return:
                    {
                        this.Return();
                        break;
                    }
                case Operator.Run:
                    {
                        var top = codeLine.Input ?? this.PopStack();
                        this.RunCommand(top);
                        break;
                    }
            }
        }

        public void Swap(int topOffset)
        {
            if (!this.stack.TrySwap(topOffset))
            {
                throw new StackException(this.CreateStackTrace(), $"Unable to swap stack, out of range: {topOffset}");
            }
        }

        public void Copy(int topOffset)
        {
            if (!this.stack.TryCopy(topOffset))
            {
                throw new StackException(this.CreateStackTrace(), $"Unable to copy stack, out of range: {topOffset}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunCommand(IValue value)
        {
            this.globalRunHandler.Invoke(value.ToString(), this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallToLabel(string label)
        {
            this.PushToStackTrace(this.ProgramCounter);
            this.Jump(label);
        }

        public void PushToStackTrace(int line)
        {
            if (!this.stackTrace.TryPush(line))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to call, call stack full");
            }
        }

        public void Return()
        {
            if (!this.stackTrace.TryPop(out var scopeLine))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to return, call stack empty");
            }

            this.ProgramCounter = scopeLine;
        }

        public void Jump(string? label)
        {
            if (string.IsNullOrEmpty(label))
            {
                this.ProgramCounter = 0;
                return;
            }

            if (this.CurrentScope.Labels.TryGetValue(label, out var line))
            {
                this.ProgramCounter = line;
            }
            else
            {
                throw new OperatorException(this.CreateStackTrace(), $"Unable to jump to label: {label}");
            }
        }

        public void Jump(int line)
        {
            if (line < 0 || line >= this.CurrentScope.Code.Count)
            {
                throw new OverflowException("Jumping to a line outside the current scope code.");
            }

            this.ProgramCounter = line;
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

            throw new StackException(this.CreateStackTrace(), "Unable to pop stack, type cast error");
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

            result.Add(DebugScopeLine(this.CurrentScope, this.ProgramCounter - 1));

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

        private static string DebugScopeLine(Scope scope, int line)
        {
            var codeLine = scope.Code[line];
            var codeLineInput = codeLine.Input != null ? codeLine.Input.ToString() : "<empty>";
            return $"[{scope.ScopeName}]:{line - 1}:{codeLine.Operator}: [{codeLineInput}]";
        }
        #endregion
    }
}