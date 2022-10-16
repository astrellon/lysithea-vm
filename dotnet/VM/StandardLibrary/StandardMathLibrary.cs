
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

            var mathFunctions = new Dictionary<string, IValue>
            {
                {"E", new NumberValue(System.Math.E)},
                {"PI", new NumberValue(System.Math.PI)},
                {"DegToRad", new NumberValue(DegToRad)},

                {"sin", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Sin(top.Value)));
                })},
                {"cos", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Cos(top.Value)));
                })},
                {"tan", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Tan(top.Value)));
                })},

                {"pow", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var y = vm.PopStack<NumberValue>();
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Pow(x.Value, y.Value)));
                })},

                {"exp", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Exp(x.Value)));
                })},

                {"floor", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Floor(x.Value)));
                })},

                {"ceil", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Ceiling(x.Value)));
                })},

                {"round", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Round(x.Value)));
                })},

                {"isFinite", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new BoolValue(double.IsFinite(x.Value)));
                })},

                {"isNaN", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(new BoolValue(double.IsNaN(x.Value)));
                })},

                {"parse", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PeekStack();
                    if (top is NumberValue)
                    {
                        return;
                    }

                    top = vm.PopStack();
                    var num = double.Parse(top.ToString());
                    vm.PushStack(new NumberValue(num));
                })},

                {"log", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Log(top.Value)));
                })},

                {"abs", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(new NumberValue(System.Math.Abs(top.Value)));
                })},

                {"max", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var right = vm.PopStack();
                    var left = vm.PopStack();
                    vm.PushStack(left.CompareTo(right) > 0 ? left : right);
                })},

                {"min", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var right = vm.PopStack();
                    var left = vm.PopStack();
                    vm.PushStack(left.CompareTo(right) < 0 ? left : right);
                })},
            };

            result.Define("math", new ObjectValue(mathFunctions));

            return result;
        }
        #endregion
    }
}