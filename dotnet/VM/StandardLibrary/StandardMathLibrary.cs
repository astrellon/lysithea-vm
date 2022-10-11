
using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public static class StandardMathLibrary
    {
        #region Fields
        private const double DegToRad = System.Math.PI / 180.0;

        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            // result.Define("math.sin", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Sin(top.Value)));
            // });
            // result.Define("math.cos", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Cos(top.Value)));
            // });
            // result.Define("math.tan", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Tan(top.Value)));
            // });

            // result.Define("math.sinDeg", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Sin(DegToRad * top.Value)));
            // });
            // result.Define("math.cosDeg", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Cos(DegToRad * top.Value)));
            // });
            // result.Define("math.tanDeg", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Tan(DegToRad * top.Value)));
            // });

            // result.Define("math.E", new NumberValue(System.Math.E));
            // result.Define("math.PI", new NumberValue(System.Math.PI));

            // result.Define("math.pow", (vm, numArgs) =>
            // {
            //     var y = vm.PopStack<NumberValue>();
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Pow(x.Value, y.Value)));
            // });

            // result.Define("math.exp", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Exp(x.Value)));
            // });

            // result.Define("math.floor", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Floor(x.Value)));
            // });

            // result.Define("math.ceil", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Ceiling(x.Value)));
            // });

            // result.Define("math.round", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Round(x.Value)));
            // });

            // result.Define("math.isFinite", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new BoolValue(double.IsFinite(x.Value)));
            // });

            // result.Define("math.isNaN", (vm, numArgs) =>
            // {
            //     var x = vm.PopStack<NumberValue>();
            //     vm.PushStack(new BoolValue(double.IsNaN(x.Value)));
            // });

            // result.Define("math.parse", (vm, numArgs) =>
            // {
            //     var top = vm.PeekStack();
            //     if (top is NumberValue)
            //     {
            //         return;
            //     }

            //     top = vm.PopStack();
            //     var num = double.Parse(top.ToString());
            //     vm.PushStack(new NumberValue(num));
            // });

            // result.Define("math.log", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Log(top.Value)));
            // });

            // result.Define("math.log2", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Log2(top.Value)));
            // });

            // result.Define("math.log10", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Log10(top.Value)));
            // });

            // result.Define("math.abs", (vm, numArgs) =>
            // {
            //     var top = vm.PopStack<NumberValue>();
            //     vm.PushStack(new NumberValue(System.Math.Abs(top.Value)));
            // });

            // result.Define("math.max", (vm, numArgs) =>
            // {
            //     var right = vm.PopStack();
            //     var left = vm.PopStack();
            //     vm.PushStack(left.CompareTo(right) > 0 ? left : right);
            // });

            // result.Define("math.min", (vm, numArgs) =>
            // {
            //     var right = vm.PopStack();
            //     var left = vm.PopStack();
            //     vm.PushStack(left.CompareTo(right) < 0 ? left : right);
            // });

            var mathObject = new Dictionary<string, IValue>();
            var add = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack<NumberValue>();
                var left = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(left.Value + right.Value));
            });
            result.Define("+", add);
            // result.Define("math.add", add);
            mathObject["add"] = add;

            var sub = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack<NumberValue>();
                var left = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(left.Value - right.Value));
            });
            result.Define("-", sub);
            // result.Define("math.sub", sub);
            mathObject["sub"] = sub;

            var mul = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack<NumberValue>();
                var left = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(left.Value * right.Value));
            });
            result.Define("*", mul);
            // result.Define("math.mul", mul);

            var div = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack<NumberValue>();
                var left = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(left.Value / right.Value));
            });
            result.Define("/", div);
            // result.Define("math.div", div);

            result.Define("math", new ObjectValue(mathObject));

            return result;
        }
        #endregion
    }
}