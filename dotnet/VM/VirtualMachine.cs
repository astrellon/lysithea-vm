using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    using RunCommandHandler = Action<string, VirtualMachine>;

    public class VirtualMachine
    {
        public struct ScopeFrame
        {
            public readonly int LineCounter;
            public readonly Scope Scope;

            public ScopeFrame(int lineCounter, Scope scope)
            {
                this.LineCounter = lineCounter;
                this.Scope = scope;
            }

            public override string ToString()
            {
                return $"{this.Scope.ScopeName}:{this.LineCounter}";
            }
        }

        #region Fields
        private static readonly RunCommandHandler EmptyHandler = (i, vm) => { };

        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<ScopeFrame> stackTrace;
        private readonly Dictionary<string, Scope> scopes;
        private readonly Dictionary<string, RunCommandHandler> runHandlers;
        private RunCommandHandler globalRunHandler;

        public Scope CurrentScope { get; private set; } = Scope.Empty;
        public int ProgramCounter { get; private set; }
        public bool Running;
        public bool Paused;

        public IReadOnlyDictionary<string, Scope> Scopes => this.scopes;
        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize, RunCommandHandler? globalRunHandler = null)
        {
            this.runHandlers = new Dictionary<string, RunCommandHandler>();
            this.globalRunHandler = globalRunHandler ?? EmptyHandler;
            this.scopes = new Dictionary<string, Scope>();
            this.ProgramCounter = 0;
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void AddRunHandler(string commandNameSpace, RunCommandHandler runCommandHandler)
        {
            this.runHandlers[commandNameSpace] = runCommandHandler;
        }

        public void SetGlobalRunHandler(RunCommandHandler runCommandHandler)
        {
            this.globalRunHandler = runCommandHandler;
        }

        public void AddScope(Scope scope)
        {
            this.scopes[scope.ScopeName] = scope;
        }

        public void AddScopes(IEnumerable<Scope> scopes)
        {
            foreach (var scope in scopes) { this.AddScope(scope); }
        }

        public void ClearScopes()
        {
            this.scopes.Clear();
        }

        public void Reset()
        {
            this.ProgramCounter = 0;
            this.stack.Clear();
            this.stackTrace.Clear();
            this.Running = false;
            this.Paused = false;
        }

        public void SetCurrentScope(string scopeName)
        {
            if (this.scopes.TryGetValue(scopeName, out var scope))
            {
                this.CurrentScope = scope;
            }
            else
            {
                throw new ScopeException(this.CreateStackTrace(), $"Unable to find scope: {scopeName} for jump.");
            }
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
                case Operator.Pop:
                    {
                        this.PopStack();
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
                            this.Jump(label);
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        var top = this.PopStack();
                        if (top.Equals(BoolValue.True))
                        {
                            this.Jump(label);
                        }
                        break;
                    }
                case Operator.Jump:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        this.Jump(label);
                        break;
                    }
                case Operator.Call:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        this.CallToLabel(label);
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
            if (value is ArrayValue arrayValue)
            {
                if (this.runHandlers.TryGetValue(arrayValue.Value[0].ToString(), out var handler))
                {
                    handler.Invoke(arrayValue.Value[1].ToString(), this);
                }
                else
                {
                    throw new OperatorException(this.CreateStackTrace(), $"Unable to find run command namespace: {value.ToString()}");
                }
            }
            else
            {
                this.globalRunHandler.Invoke(value.ToString(), this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallToLabel(IValue label)
        {
            this.PushToStackTrace(this.ProgramCounter, this.CurrentScope);
            this.Jump(label);
        }

        public void PushToStackTrace(int line, Scope scope)
        {
            if (!this.stackTrace.TryPush(new ScopeFrame(line, scope)))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to call, call stack full");
            }
        }

        public void Return()
        {
            if (!this.stackTrace.TryPop(out var scopeFrame))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to return, call stack empty");
            }

            this.CurrentScope = scopeFrame.Scope;
            this.ProgramCounter = scopeFrame.LineCounter;
        }

        public void Jump(IValue jumpTo)
        {
            if (jumpTo is StringValue stringValue)
            {
                this.Jump(stringValue.Value);
            }
            else if (jumpTo is ArrayValue arrayValue)
            {
                if (arrayValue.Value.Count() == 0)
                {
                    throw new OperatorException(this.CreateStackTrace(), "Cannot jump to an empty array");
                }
                string? scopeName = null;
                var label = arrayValue.Value[0].ToString();
                if (arrayValue.Value.Count() > 1)
                {
                    scopeName = arrayValue.Value[1].ToString();
                }

                this.Jump(label, scopeName);
            }
        }

        public void Jump(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input[0] == ':')
                {
                    this.Jump(input, null);
                }
                else
                {
                    this.Jump(null, input);
                }
            }
            else
            {
                this.ProgramCounter = 0;
            }
        }

        public void Jump(string? label, string? scopeName)
        {
            if (!string.IsNullOrEmpty(scopeName))
            {
                this.SetCurrentScope(scopeName);
            }

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

            result.Add(DebugScopeLine(this.CurrentScope, this.ProgramCounter - 1));
            for (var i = this.stackTrace.Index - 1; i >= 0; i--)
            {
                var stackFrame = this.stackTrace.Data[i];
                result.Add(DebugScopeLine(stackFrame.Scope, stackFrame.LineCounter));
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

        private static string DebugScopeLine(Scope scope, int line)
        {
            if (line >= scope.Code.Count)
            {
                return $"[{scope.ScopeName}:{line - 1}: end of code";
            }
            if (line < 0)
            {
                return $"[{scope.ScopeName}:{line - 1}: before start of code";
            }

            var codeLine = scope.Code[line];
            var codeLineInput = codeLine.Input != null ? codeLine.Input.ToString() : "<empty>";
            return $"[{scope.ScopeName}]:{line - 1}:{codeLine.Operator}: [{codeLineInput}]";
        }
        #endregion
    }
}