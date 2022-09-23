
using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardMathLibrary
    {
        #region Fields
        public const string HandleName = "math";
        private const double DegToRad = System.Math.PI / 180.0;
        #endregion

        #region Methods
        public static void AddHandler(VirtualMachine vm)
        {
            vm.AddRunHandler(HandleName, Handler);
        }

        public static void Handler(string command, VirtualMachine vm)
        {
            switch (command)
            {
                // Math Operators
                case "sin":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Sin(top.Value)));
                        break;
                    }
                case "cos":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Cos(top.Value)));
                        break;
                    }
                case "tan":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Tan(top.Value)));
                        break;
                    }
                case "sinDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Sin(DegToRad * top.Value)));
                        break;
                    }
                case "cosDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Cos(DegToRad * top.Value)));
                        break;
                    }
                case "tanDeg":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Tan(DegToRad * top.Value)));
                        break;
                    }
                case "E":
                    {
                        vm.PushStack(new NumberValue(System.Math.E));
                        break;
                    }
                case "PI":
                    {
                        vm.PushStack(new NumberValue(System.Math.PI));
                        break;
                    }
                case "pow":
                    {
                        var y = vm.PopStack<NumberValue>();
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Pow(x.Value, y.Value)));
                        break;
                    }
                case "exp":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Exp(x.Value)));
                        break;
                    }
                case "floor":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Floor(x.Value)));
                        break;
                    }
                case "ceil":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Ceiling(x.Value)));
                        break;
                    }
                case "round":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Round(x.Value)));
                        break;
                    }
                case "isFinite":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new BoolValue(double.IsFinite(x.Value)));
                        break;
                    }
                case "isNaN":
                    {
                        var x = vm.PopStack<NumberValue>();
                        vm.PushStack(new BoolValue(double.IsNaN(x.Value)));
                        break;
                    }
                case "parse":
                    {
                        var top = vm.PeekStack();
                        if (top is NumberValue)
                        {
                            break;
                        }

                        top = vm.PopStack();
                        var num = double.Parse(top.ToString());
                        vm.PushStack(new NumberValue(num));
                        break;
                    }
                case "log":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Log(top.Value)));
                        break;
                    }
                case "log2":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Log2(top.Value)));
                        break;
                    }
                case "log10":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Log10(top.Value)));
                        break;
                    }
                case "abs":
                    {
                        var top = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(System.Math.Abs(top.Value)));
                        break;
                    }
                case "max":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        if (left.CompareTo(right) > 0)
                        {
                            vm.PushStack(left);
                        }
                        break;
                    }
                case "min":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        if (left.CompareTo(right) > 0)
                        {
                            vm.PushStack(right);
                        }
                        break;
                    }
                case "+":
                case "add":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value + right.Value));
                        break;
                    }
                case "-":
                case "sub":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value - right.Value));
                        break;
                    }
                case "*":
                case "mul":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value * right.Value));
                        break;
                    }
                case "/":
                case "div":
                    {
                        var right = vm.PopStack<NumberValue>();
                        var left = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(left.Value / right.Value));
                        break;
                    }
            }
        }
        #endregion
    }
}