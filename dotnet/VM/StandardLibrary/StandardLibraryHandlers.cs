using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardLibrary
    {
        #region Methods
        private const double DegToRad = Math.PI / 180.0;

        public static bool Standard(IValue input, VirtualMachine vm)
        {
            if (input is StringValue stringValue)
            {
                var command = stringValue.Value;
                switch (command)
                {
                    // Comparison Operators
                    case ">":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) > 0));
                            return true;
                        }
                    case ">=":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) >= 0));
                            return true;
                        }
                    case "==":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) == 0));
                            return true;
                        }
                    case "!=":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) != 0));
                            return true;
                        }
                    case "<":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) < 0));
                            return true;
                        }
                    case "<=":
                        {
                            var right = vm.PopStack();
                            var left = vm.PopStack();
                            vm.PushStack(new BoolValue(left.CompareTo(right) <= 0));
                            return true;
                        }
                    case "!":
                        {
                            var top = vm.PopStack<BoolValue>();
                            vm.PushStack(new BoolValue(!top.Value));
                            return true;
                        }

                    // Math Operators
                    case "math:sin":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Sin(top.Value)));
                        return true;
                    }
                    case "math:cos":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Cos(top.Value)));
                        return true;
                    }
                    case "math:tan":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Tan(top.Value)));
                        return true;
                    }
                    case "math:sinDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Sin(DegToRad * top.Value)));
                        return true;
                    }
                    case "math:cosDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Cos(DegToRad * top.Value)));
                        return true;
                    }
                    case "math:tanDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Tan(DegToRad * top.Value)));
                        return true;
                    }
                    case "math:E":
                    {
                        vm.PushStack(new NumberValue(Math.E));
                        return true;
                    }
                    case "math:PI":
                    {
                        vm.PushStack(new NumberValue(Math.PI));
                        return true;
                    }
                    case "math:pow":
                    {
                        var y = vm.PopStack<NumberValue>();
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Pow(x.Value, y.Value)));
                        return true;
                    }
                    case "math:log":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Log(top.Value)));
                        return true;
                    }
                    case "math:log2":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Log2(top.Value)));
                        return true;
                    }
                    case "math:log10":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Log10(top.Value)));
                        return true;
                    }
                    case "math:abs":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Abs(top.Value)));
                        return true;
                    }
                    case "math:max":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Max(left.Value, right.Value)));
                        return true;
                    }
                    case "math:min":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(Math.Min(left.Value, right.Value)));
                        return true;
                    }
                    case "+":
                    case "math:add":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value + right.Value));
                        return true;
                    }
                    case "-":
                    case "math:sub":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value - right.Value));
                        return true;
                    }
                    case "*":
                    case "math:mul":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value * right.Value));
                        return true;
                    }
                    case "/":
                    case "math:div":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value / right.Value));
                        return true;
                    }

                    // Misc
                    case "toString":
                    {
                        var top = vm.PopStack();
                        vm.PushStack(new StringValue(top.ToString()));
                        return true;
                    }

                    // String Operators
                    case "string:append":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<StringValue>();
                        vm.PushStack(left.Append(right.ToString()));
                        return true;
                    }
                    case "string:prepend":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<StringValue>();
                        vm.PushStack(left.Prepend(right.ToString()));
                        return true;
                    }
                    case "string:length":
                    {
                        var top = vm.PeekStack<StringValue>();
                        vm.PushStack(new NumberValue(top.Value.Length));
                        return true;
                    }
                    case "string:at":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PeekStack<StringValue>();
                        vm.PushStack(new StringValue(top.Value[(int)index.Value].ToString()));
                        return true;
                    }

                    // List Operators
                    case "list:append":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<StringValue>();
                        vm.PushStack(left.Append(right.ToString()));
                        return true;
                    }
                    case "list:prepend":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<StringValue>();
                        vm.PushStack(left.Prepend(right.ToString()));
                        return true;
                    }
                    case "list:length":
                    {
                        var top = vm.PeekStack<ArrayValue>();
                        vm.PushStack(new NumberValue(top.Value.Count));
                        return true;
                    }
                    case "list:at":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PeekStack<ArrayValue>();
                        vm.PushStack(top.Value[(int)index]);
                        return true;
                    }

                    // Object Operators
                    case "object:set":
                    {
                        var value = vm.PopStack();
                        var key = vm.PopStack<StringValue>();
                        var obj = vm.PopStack<ObjectValue>();
                        vm.PushStack(obj.Set(key, value));
                        return true;
                    }
                    case "object:get":
                    {
                        var key = vm.PopStack<StringValue>();
                        var obj = vm.PopStack<ObjectValue>();
                        if (obj.TryGetValue(key, out var value))
                        {
                            vm.PushStack(value);
                        }
                        else
                        {
                            vm.PushStack(NullValue.Value);
                        }
                        return true;
                    }
                    case "object:keys":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        var keys = top.Value.Keys.Select(k => new StringValue(k)).Cast<IValue>().ToList();
                        var list = new ArrayValue(keys);
                        vm.PushStack(list);
                        return true;
                    }
                    case "object:values":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        var list = new ArrayValue(top.Value.Values.ToList());
                        vm.PushStack(list);
                        return true;
                    }
                    case "object:length":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        vm.PushStack(new NumberValue(top.Value.Count));
                        return true;
                    }
                    default: return false;
                }
            }

            return false;
        }
        #endregion
    }
}