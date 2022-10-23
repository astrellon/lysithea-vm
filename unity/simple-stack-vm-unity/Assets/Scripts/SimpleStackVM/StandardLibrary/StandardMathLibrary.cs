
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
                {"E", new NumberValue(Math.E)},
                {"PI", new NumberValue(Math.PI)},
                {"DegToRad", new NumberValue(DegToRad)},

                {"sin", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Sin(top.Value));
                })},

                {"cos", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Cos(top.Value));
                })},

                {"tan", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Tan(top.Value));
                })},

                {"pow", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var y = vm.PopStack<NumberValue>();
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Pow(x.Value, y.Value));
                })},

                {"exp", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Exp(x.Value));
                })},

                {"floor", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Floor(x.Value));
                })},

                {"ceil", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Ceiling(x.Value));
                })},

                {"round", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Round(x.Value));
                })},

                {"isFinite", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(double.IsFinite(x.Value));
                })},

                {"isNaN", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var x = vm.PopStack<NumberValue>();
                    vm.PushStack(double.IsNaN(x.Value));
                })},

                {"parse", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PeekStack();
                    if (top is NumberValue)
                    {
                        return;
                    }

                    top = vm.PopStack();
                    vm.PushStack(double.Parse(top.ToString()));
                })},

                {"log", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Log(top.Value));
                })},

                {"abs", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<NumberValue>();
                    vm.PushStack(Math.Abs(top.Value));
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

                {"inc", IncNumber},
                {"dec", DecNumber},
            };

            result.Define("math", new ObjectValue(mathFunctions));

            return result;
        }

        public static readonly BuiltinFunctionValue IncNumber = new BuiltinFunctionValue((vm, numArgs) =>
        {
            vm.PushStack(new NumberValue(vm.PopStack<NumberValue>().Value + 1));
        });

        public static readonly BuiltinFunctionValue DecNumber = new BuiltinFunctionValue((vm, numArgs) =>
        {
            vm.PushStack(new NumberValue(vm.PopStack<NumberValue>().Value - 1));
        });

        #endregion
    }
}