
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

            var mathFunctions = new Dictionary<string, IValue>();

            mathFunctions["E"] = new NumberValue(System.Math.E);
            mathFunctions["PI"] = new NumberValue(System.Math.PI);

            mathFunctions["sin"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Sin(top.Value)));
            });
            mathFunctions["cos"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Cos(top.Value)));
            });
            mathFunctions["tan"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Tan(top.Value)));
            });

            mathFunctions["sinDeg"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Sin(DegToRad * top.Value)));
            });
            mathFunctions["cosDeg"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Cos(DegToRad * top.Value)));
            });
            mathFunctions["tanDeg"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Tan(DegToRad * top.Value)));
            });

            mathFunctions["pow"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var y = vm.PopStack<NumberValue>();
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Pow(x.Value, y.Value)));
            });

            mathFunctions["exp"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Exp(x.Value)));
            });

            mathFunctions["floor"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Floor(x.Value)));
            });

            mathFunctions["ceil"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Ceiling(x.Value)));
            });

            mathFunctions["round"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Round(x.Value)));
            });

            mathFunctions["isFinite"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new BoolValue(double.IsFinite(x.Value)));
            });

            mathFunctions["isNaN"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new BoolValue(double.IsNaN(x.Value)));
            });

            mathFunctions["parse"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PeekStack();
                if (top is NumberValue)
                {
                    return;
                }

                top = vm.PopStack();
                var num = double.Parse(top.ToString());
                vm.PushStack(new NumberValue(num));
            });

            mathFunctions["log"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Log(top.Value)));
            });

            mathFunctions["log2"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Log2(top.Value)));
            });

            mathFunctions["log10"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Log10(top.Value)));
            });

            mathFunctions["abs"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(System.Math.Abs(top.Value)));
            });

            mathFunctions["max"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(left.CompareTo(right) > 0 ? left : right);
            });

            mathFunctions["min"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(left.CompareTo(right) < 0 ? left : right);
            });

            result.Define("math", new ObjectValue(mathFunctions));

            return result;
        }
        #endregion
    }
}