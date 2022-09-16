using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
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

        [Flags]
        public enum FlagValues : byte
        {
            None = 0,
            Running = 1 << 1,
            Paused = 1 << 2,
            DebugMode = 1 << 3
        }

        #region Fields
        public delegate void RunCommandHandler(IValue command, VirtualMachine vm);

        public FlagValues Flags;

        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<ScopeFrame> stackTrace;
        private readonly Dictionary<string, Scope> scopes;
        private Scope currentScope = Scope.Empty;
        private int programCounter;
        private RunCommandHandler runHandler;

        public int ProgramCounter => this.programCounter;
        public Scope CurrentScope => this.currentScope;
        public IReadOnlyDictionary<string, Scope> Scopes => this.scopes;

        public bool IsRunning => this.Flags.HasFlag(FlagValues.Running);
        public bool IsPaused => this.Flags.HasFlag(FlagValues.Paused);

        public IReadOnlyFixedStack<IValue> Stack => this.stack;
        public IReadOnlyFixedStack<ScopeFrame> StackTrace => this.stackTrace;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize, RunCommandHandler runHandler)
        {
            this.runHandler = runHandler;

            this.scopes = new Dictionary<string, Scope>();
            this.programCounter = 0;
            this.stack = new FixedStack<IValue>(stackSize);
            this.stackTrace = new FixedStack<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void AddScope(Scope scope)
        {
            this.scopes[scope.ScopeName] = scope;
        }

        public void AddScopes(IEnumerable<Scope> scopes)
        {
            foreach (var scope in scopes) { this.AddScope(scope); }
        }

        public void SetScopes(IEnumerable<Scope> scopes)
        {
            this.scopes.Clear();
            this.AddScopes(scopes);
        }

        public void Restart()
        {
            this.programCounter = 0;
            this.SetRunning(true);
            this.SetPause(false);
        }

        public void SetCurrentScope(string scopeName)
        {
            if (this.scopes.TryGetValue(scopeName, out var scope))
            {
                this.currentScope = scope;
            }
            else
            {
                throw new ScopeException(this.CreateStackTrace(), $"Unable to find scope: {scopeName} for jump.");
            }
        }

        public void SetRunning(bool running)
        {
            if (running)
            {
                this.Flags |= FlagValues.Running;
            }
            else
            {
                this.Flags &= ~FlagValues.Running;
            }
        }

        public void SetPause(bool paused)
        {
            if (paused)
            {
                this.Flags |= FlagValues.Paused;
            }
            else
            {
                this.Flags &= ~FlagValues.Paused;
            }
        }

        public void Step()
        {
            if (this.currentScope.IsEmpty || this.programCounter >= this.currentScope.Code.Count)
            {
                this.SetRunning(false);
                return;
            }

            var codeLine = this.currentScope.Code[this.programCounter++];

            if (this.Flags.HasFlag(FlagValues.DebugMode))
            {
                var debugLine = DebugScopeLine(this.currentScope, this.programCounter - 1);
                Console.WriteLine($"- {debugLine}");
            }

            switch (codeLine.Operator)
            {
                default:
                    {
                        throw new UnknownOperatorException(this.CreateStackTrace(), $"Unknown operator: {codeLine.Operator}");
                    }
                case Operator.Push:
                    {
                        if (codeLine.Input.IsNull)
                        {
                            throw new OperatorException(this.CreateStackTrace(), "Push requires input");
                        }
                        if (!this.stack.TryPush(codeLine.Input))
                        {
                            throw new StackException(this.CreateStackTrace(), "Stack is full!");
                        }
                        break;
                    }
                case Operator.JumpFalse:
                    {
                        var label = codeLine.Input ?? this.PopStack();

                        var top = this.PopStack();
                        if (top.Equals(BoolValue.False))
                        {
                            this.JumpToLabel(label);
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        var top = this.PopStack();
                        if (top.Equals(BoolValue.True))
                        {
                            this.JumpToLabel(label);
                        }
                        break;
                    }
                case Operator.Jump:
                    {
                        var label = codeLine.Input ?? this.PopStack();
                        this.JumpToLabel(label);
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
                        this.runHandler.Invoke(top, this);
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallToLabel(IValue label)
        {
            this.PushToStackTrace(this.programCounter, this.currentScope);
            this.JumpToLabel(label);
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

            this.currentScope = scopeFrame.Scope;
            this.programCounter = scopeFrame.LineCounter;
        }

        public void JumpToLabel(IValue jumpTo)
        {
            if (jumpTo is StringValue stringValue)
            {
                this.JumpToLabel(stringValue.Value, null);
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

                this.JumpToLabel(label, scopeName);
            }
        }

        public void JumpToLabel(string label, string? scopeName)
        {
            if (!string.IsNullOrEmpty(scopeName))
            {
                this.SetCurrentScope(scopeName);
            }

            if (string.IsNullOrEmpty(label))
            {
                this.programCounter = 0;
                return;
            }

            if (this.currentScope.Labels.TryGetValue(label, out var line))
            {
                this.programCounter = line;
            }
            else
            {
                throw new OperatorException(this.CreateStackTrace(), $"Unable to jump to label: {label}");
            }
        }

        public void Jump(int line)
        {
            if (line < 0 || line >= this.currentScope.Code.Count)
            {
                throw new OverflowException("Jumping to a line outside the current scope code.");
            }

            this.programCounter = line;
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

            result.Add(DebugScopeLine(this.currentScope, this.programCounter - 1));
            for (var i = this.stackTrace.Index - 1; i >= 0; i--)
            {
                var stackFrame = this.stackTrace.Data[i];
                result.Add(DebugScopeLine(stackFrame.Scope, stackFrame.LineCounter));
            }

            return result;
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