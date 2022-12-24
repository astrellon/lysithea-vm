using System;
using System.Linq;

namespace LysitheaVM
{
    public partial class VirtualMachine
    {
        #region Methods
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

            switch (codeLine.Operator)
            {
                default:
                    {
                        throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unknown operator: {codeLine.Operator}");
                    }
                case Operator.Push:
                    {
                        if (codeLine.Input != null)
                        {
                            this.PushStack(codeLine.Input);
                        }
                        else
                        {
                            this.PushStack(this.PeekStack());
                        }

                        break;
                    }
                case Operator.ToArgument:
                    {
                        var top = codeLine.Input ?? this.PopStack();
                        if (!(top is IArrayValue arrayValue))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to convert argument value onto stack: {top.ToString()}");
                        }

                        this.PushStack(new ArgumentsValue(arrayValue.ArrayValues));
                        break;
                    }
                case Operator.Get:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        if (!(key is StringValue))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to get variable, input needs to be a string: {key.ToString()}");
                        }

                        var keyString = key.ToString();
                        if (this.CurrentScope.TryGetKey(keyString, out var foundValue) ||
                            (this.BuiltinScope != null && this.BuiltinScope.TryGetKey(keyString, out foundValue)))
                        {
                            this.PushStack(foundValue);
                        }
                        else
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to get variable: {key.ToString()}");
                        }
                        break;
                    }
                case Operator.GetProperty:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        if (!(key is ArrayValue arrayInput))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to get property, input needs to be an array: {key.ToString()}");
                        }

                        var top = this.PopStack();
                        if (ValuePropertyAccess.TryGetProperty(top, arrayInput, out var found))
                        {
                            this.PushStack(found);
                        }
                        else
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to get property: {key.ToString()}");
                        }
                        break;
                    }
                case Operator.Define:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        if (!this.CurrentScope.TryDefine(key.ToString(), value))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to define {key}, value is defined as constant");
                        }
                        break;
                    }
                case Operator.Set:
                    {
                        var key = codeLine.Input ?? this.PopStack();
                        var value = this.PopStack();
                        if (!this.CurrentScope.TrySet(key.ToString(), value))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Unable to set variable that has not been defined or is constant: {key.ToString()} = {value.ToString()}");
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
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Call needs a num args code line input");
                        }

                        var top = this.PopStack();
                        if (top is IFunctionValue procTop)
                        {
                            this.CallFunction(procTop, numArgs.IntValue, true);
                        }
                        else
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Call needs a function to run: {top.ToString()}");
                        }
                        break;
                    }
                case Operator.CallDirect:
                    {
                        if (codeLine.Input == null || !(codeLine.Input is ArrayValue arrayInput) ||
                           !(arrayInput[0] is IFunctionValue procTop) || !(arrayInput[1] is NumberValue numArgs))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Call direct needs an array of the function and num args code line input");
                        }

                        this.CallFunction(procTop, numArgs.IntValue, true);
                        break;
                    }

                // Misc Operator
                case Operator.StringConcat:
                    {
                        if (codeLine.Input == null || !(codeLine.Input is NumberValue numArgs))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"StringConcat operator needs the number of args to concat");
                        }

                        var args = this.GetArgs(numArgs.IntValue);
                        this.PushStack(string.Join("", args.Value));
                        break;
                    }

                // Math Operators
                case Operator.Add:
                    {
                        this.PushStack(this.GetNumArg(codeLine) + this.PopStackDouble());
                        break;
                    }
                case Operator.Sub:
                    {
                        var right = this.GetNumArg(codeLine);
                        var left = this.PopStackDouble();
                        this.PushStack(left - right);
                        break;
                    }
                case Operator.UnaryNegative:
                    {
                        this.PushStack(-this.PopStackDouble());
                        break;
                    }
                case Operator.Multiply:
                    {
                        this.PushStack(this.GetNumArg(codeLine) * this.PopStackDouble());
                        break;
                    }
                case Operator.Divide:
                    {
                        var right = this.GetNumArg(codeLine);
                        var left = this.PopStackDouble();
                        this.PushStack(left / right);
                        break;
                    }
                case Operator.Inc:
                    {
                        if (codeLine.Input == null)
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Inc operator needs code line variable");
                        }

                        var key = codeLine.Input.ToString();
                        if (!this.CurrentScope.TryGetKey<NumberValue>(key, out var foundValue))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Inc operator could not find variable: {key}");
                        }
                        this.CurrentScope.TrySet(key, new NumberValue((foundValue).Value + 1));
                        break;
                    }
                case Operator.Dec:
                    {
                        if (codeLine.Input == null)
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Dec operator needs code line variable");
                        }

                        var key = codeLine.Input.ToString();
                        if (!this.CurrentScope.TryGetKey<NumberValue>(key, out var foundValue))
                        {
                            throw new VirtualMachineException(this, this.CreateStackTrace(), $"Dec operator could not find variable: {key}");
                        }
                        this.CurrentScope.TrySet(key, new NumberValue((foundValue).Value - 1));
                        break;
                    }

                // Comparison Operators
                case Operator.LessThan:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) < 0);
                        break;
                    }
                case Operator.LessThanEquals:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) <= 0);
                        break;
                    }
                case Operator.Equals:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) == 0);
                        break;
                    }
                case Operator.NotEquals:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) != 0);
                        break;
                    }
                case Operator.GreaterThan:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) > 0);
                        break;
                    }
                case Operator.GreaterThanEquals:
                    {
                        var right = codeLine.Input ?? this.PopStack();
                        var left = this.PopStack();
                        this.PushStack(left.CompareTo(right) >= 0);
                        break;
                    }

                    // Boolean Operators
                case Operator.And:
                    {
                        this.PushStack(this.GetBoolArg(codeLine) && this.PopStackBool());
                        break;
                    }
                case Operator.Or:
                    {
                        this.PushStack(this.GetBoolArg(codeLine) || this.PopStackBool());
                        break;
                    }
                case Operator.Not:
                    {
                        this.PushStack(!this.PopStackBool());
                        break;
                    }
            }
        }
        #endregion
    }
}