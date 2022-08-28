using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        }

        #region Fields
        public delegate void RunCommandHandler(IValue command, VirtualMachine vm);

        public bool DebugMode = false;

        private readonly FixedStack<IValue> stack;
        private readonly FixedStack<ScopeFrame> stackTrace;
        private readonly Dictionary<string, Scope> scopes;
        private Scope currentScope;
        private int programCounter;
        private bool running;
        private bool paused;
        private RunCommandHandler runHandler;

        public bool IsRunning => this.running;
        public int ProgramCounter => this.programCounter;
        public Scope CurrentScope => this.currentScope;
        public IReadOnlyDictionary<string, Scope> Scopes => this.scopes;
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

        public void Stop()
        {
            this.running = false;
        }

        public void Run(string? startScopeName = null)
        {
            if (startScopeName != null)
            {
                if (this.scopes.TryGetValue(startScopeName, out var startScope))
                {
                    this.currentScope = startScope;
                }
                else
                {
                    throw new ScopeException(this.CreateStackTrace(), $"Unable to find start scope: {startScopeName}");
                }
            }
            else if (this.currentScope.IsEmpty)
            {
                throw new Exception("Cannot run virtual machine with an empty scope");
            }

            this.running = true;
            this.paused = false;
            while (this.running && !this.paused)
            {
                this.Step();
            }
        }

        public void SetPause(bool paused)
        {
            this.paused = paused;
        }

        public void Step()
        {
            if (this.programCounter >= this.currentScope.Code.Count)
            {
                this.Stop();
                return;
            }

            var codeLine = this.currentScope.Code[this.programCounter++];

            if (this.DebugMode)
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
                        if (codeLine.Input == null)
                        {
                            throw new OperatorException(this.CreateStackTrace(), "Push requires input");
                        }
                        if (!this.stack.TryPush(codeLine.Input))
                        {
                            throw new StackException(this.CreateStackTrace(), "Stack is full!");
                        }
                        break;
                    }
                case Operator.Pop:
                    {
                        this.PopStack();
                        break;
                    }
                case Operator.JumpFalse:
                    {
                        var label = this.PopStack();
                        var top = this.PopStack();
                        if (top.Equals(BoolValue.False))
                        {
                            this.JumpToLabel(label);
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var label = this.PopStack();
                        var top = this.PopStack();
                        if (top.Equals(BoolValue.True))
                        {
                            this.JumpToLabel(label);
                        }
                        break;
                    }
                case Operator.Jump:
                    {
                        var label = this.PopStack();
                        this.JumpToLabel(label);
                        break;
                    }
                case Operator.Call:
                    {
                        var label = this.PopStack();
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
                        var top = this.PopStack();
                        this.runHandler.Invoke(top, this);
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallToLabel(IValue label)
        {
            if (!this.stackTrace.TryPush(new ScopeFrame(this.programCounter, this.currentScope)))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to call, call stack full");
            }
            this.JumpToLabel(label);
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
                if (this.scopes.TryGetValue(scopeName, out var scope))
                {
                    this.currentScope = scope;
                }
                else
                {
                    throw new ScopeException(this.CreateStackTrace(), $"Unable to find scope: {scopeName} for jump.");
                }
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
            for (var i = this.stackTrace.Data.Count() - 1; i >= 0; i--)
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