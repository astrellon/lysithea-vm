using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        public delegate IValue GetVariableHandler(string name, VirtualMachine vm);
        public delegate void RunCommandHandler(string command, VirtualMachine vm);

        public readonly int StackSize;
        public bool DebugMode = false;

        private readonly List<IValue> stack;
        private readonly List<ScopeFrame> stackTrace;

        private readonly Dictionary<string, Scope> scopes;
        private Scope currentScope;

        private int programCounter;
        private bool running;
        private bool paused;

        public event GetVariableHandler? OnGetVariable;
        public event RunCommandHandler? OnRunCommand;

        public bool IsRunning => this.running;
        public int ProgramCounter => this.programCounter;
        public Scope CurrentScope => this.currentScope;
        #endregion

        #region Constructor
        public VirtualMachine(int stackSize = 64)
        {
            this.StackSize = stackSize;

            this.scopes = new Dictionary<string, Scope>();
            this.programCounter = 0;
            this.stack = new List<IValue>(stackSize);
            this.stackTrace = new List<ScopeFrame>(stackSize);
        }
        #endregion

        #region Methods
        public void AddScope(Scope scope)
        {
            this.scopes[scope.ScopeName] = scope;
        }

        public void Stop()
        {
            this.running = false;
        }

        public void Run(string startScopeName)
        {
            if (this.scopes.TryGetValue(startScopeName, out var startScope))
            {
                this.currentScope = startScope;
            }
            else
            {
                throw new ScopeException(this.CreateStackTrace(), $"Unable to find start scope: {startScopeName}");
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
                        this.stack.Add(this.EvaluateLine(codeLine.Input));
                        break;
                    }
                case Operator.Pop:
                    {
                        this.PopObject();
                        break;
                    }
                case Operator.JumpFalse:
                    {
                        var top = this.PopObject();
                        if (codeLine.Input != null)
                        {
                            if (top.Equals(false))
                            {
                                this.JumpToLabel(this.EvaluateLineString(codeLine.Input));
                            }
                        }
                        else
                        {
                            var label = this.PopStackString();
                            if (top.Equals(false))
                            {
                                this.JumpToLabel(label);
                            }
                        }
                        break;
                    }
                case Operator.JumpTrue:
                    {
                        var top = this.PopObject();
                        if (codeLine.Input != null)
                        {
                            if (top.Equals(true))
                            {
                                this.JumpToLabel(this.EvaluateLineString(codeLine.Input));
                            }
                        }
                        else
                        {
                            var label = this.PopStackString();
                            if (top.Equals(true))
                            {
                                this.JumpToLabel(label);
                            }
                        }
                        break;
                    }
                case Operator.Jump:
                    {
                        if (codeLine.Input != null)
                        {
                            this.JumpToLabel(this.EvaluateLineString(codeLine.Input));
                        }
                        else
                        {
                            var label = this.PopStackString();
                            this.JumpToLabel(label);
                        }
                        break;
                    }
                case Operator.Call:
                    {
                        if (codeLine.Input != null)
                        {
                            this.CallToLabel(this.EvaluateLineString(codeLine.Input));
                        }
                        else
                        {
                            var label = this.PopStackString();
                            this.CallToLabel(label);
                        }
                        break;
                    }
                case Operator.Return:
                    {
                        this.Return();
                        break;
                    }
                case Operator.Run:
                    {
                        if (codeLine.Input != null)
                        {
                            var commandName = this.EvaluateLineString(codeLine.Input);
                            if (this.OnRunCommand == null)
                            {
                                throw new OperatorException(this.CreateStackTrace(), $"Cannot run command {commandName}, no listener set");
                            }

                            this.OnRunCommand.Invoke(commandName, this);
                        }
                        else
                        {
                            var top = this.PopStackString();
                            if (this.OnRunCommand == null)
                            {
                                throw new OperatorException(this.CreateStackTrace(), $"Cannot run command {top}, no listener set");
                            }

                            this.OnRunCommand.Invoke(top, this);
                        }
                        break;
                    }
            }
        }

        public string EvaluateLineString(IValue input)
        {
            var result = this.EvaluateLine(input);
            return result.ToString() ?? "<null>";
        }

        public IValue EvaluateLine(IValue input)
        {
            if (input is StringValue str)
            {
                var strValue = str.Value;
                if (string.IsNullOrEmpty(strValue))
                {
                    return StringValue.Empty;
                }

                if (strValue.First() == '{' && strValue.Last() == '}')
                {
                    IValue result = NullValue.Value;
                    var varName = strValue.Substring(1, strValue.Length - 2);
                    var path = ObjectPath.Create(varName);
                    if (path.Path.First() == "TOP")
                    {
                        result = this.stack.Last();
                    }
                    else
                    {
                        if (this.OnGetVariable == null)
                        {
                            throw new OperatorException(this.CreateStackTrace(), $"Cannot get variable {varName}, no listener set");
                        }

                        result = this.OnGetVariable.Invoke(path.Current, this);
                    }

                    if (path.HasMorePath && result is ObjectValue objectValue)
                    {
                        if (objectValue.TryGetValue(path.NextIndex(), out var objectResult))
                        {
                            return objectResult;
                        }

                        throw new OperatorException(this.CreateStackTrace(), $"Unable to get path from variable {path.Path.ToString()}");
                    }

                    return result;
                }

                return str;
            }

            return input;
        }

        public void CallToLabel(string label)
        {
            this.stackTrace.Add(new ScopeFrame(this.programCounter, this.currentScope));
            this.JumpToLabel(label);
        }

        public void Return()
        {
            if (!this.stackTrace.Any())
            {
                throw new StackException(this.CreateStackTrace(), "Unable to return, call stack empty");
            }

            var scopeFrame = this.stackTrace.Last();
            this.stackTrace.Pop();
            this.currentScope = scopeFrame.Scope;
            this.programCounter = scopeFrame.LineCounter;
        }

        public void JumpToLabel(string label)
        {
            if (!label.Any())
            {
                throw new OperatorException(this.CreateStackTrace(), "Cannot jump with empty label");
            }

            if (label[0] == '[')
            {
                var endScope = label.IndexOf(']');
                if (endScope < 0)
                {
                    throw new ScopeException(this.CreateStackTrace(), $"Invalid scope name jump label: {label}");
                }

                var scopeName = label.Substring(1, endScope - 1);
                if (this.scopes.TryGetValue(scopeName, out var scope))
                {
                    this.currentScope = scope;
                }
                else
                {
                    throw new ScopeException(this.CreateStackTrace(), $"Unable to find scope: {scopeName} for jump: {label}");
                }

                label = label.Substring(endScope + 1).TrimStart();
                if (label.Length == 0)
                {
                    this.programCounter = 0;
                    return;
                }
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

        public IValue PopObject()
        {
            if (!this.stack.TryPop(out var obj))
            {
                throw new StackException(this.CreateStackTrace(), "Unable to pop stack, empty");
            }

            return obj;
        }

        public double PopStackDouble()
        {
            var obj = this.PopObject();

            if (obj is NumberValue num)
            {
                return num.Value;
            }

            throw new StackException(this.CreateStackTrace(), $"Top of stack not a double: {obj}");
        }

        public int PopStackInt()
        {
            var obj = this.PopObject();

            if (obj is NumberValue num)
            {
                return (int)num.Value;
            }

            throw new StackException(this.CreateStackTrace(), $"Top of stack not an int: {obj}");
        }

        public string PopStackString()
        {
            var obj = this.PopObject();
            return obj.ToString() ?? "<null>";
        }

        public void PushStack(IValue value)
        {
            this.stack.Add(value);
        }

        public IReadOnlyList<string> CreateStackTrace()
        {
            var result = new List<string>();

            result.Add(DebugScopeLine(this.currentScope, this.programCounter - 1));
            for (var i = this.stackTrace.Count - 1; i >= 0; i--)
            {
                var stackFrame = this.stackTrace[i];
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