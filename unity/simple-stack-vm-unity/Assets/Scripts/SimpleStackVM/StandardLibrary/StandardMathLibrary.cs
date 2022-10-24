
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

                {"sin", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Sin(top.Value));
                })},

                {"cos", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Cos(top.Value));
                })},

                {"tan", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Tan(top.Value));
                })},

                {"pow", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    var y = args.Get<NumberValue>(1);
                    vm.PushStack(Math.Pow(x.Value, y.Value));
                })},

                {"exp", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Exp(x.Value));
                })},

                {"floor", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Floor(x.Value));
                })},

                {"ceil", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Ceiling(x.Value));
                })},

                {"round", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Round(x.Value));
                })},

                {"isFinite", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(double.IsFinite(x.Value));
                })},

                {"isNaN", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.Get<NumberValue>(0);
                    vm.PushStack(double.IsNaN(x.Value));
                })},

                {"parse", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get(0);
                    vm.PushStack(double.Parse(top.ToString()));
                })},

                {"log", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Log(top.Value));
                })},

                {"abs", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.Get<NumberValue>(0);
                    vm.PushStack(Math.Abs(top.Value));
                })},

                {"max", new BuiltinFunctionValue((vm, args) =>
                {
                    var max = args.Get(0);
                    for (var i = 1; i < args.Length; i++)
                    {
                        var next = args[i];
                        if (next.CompareTo(max) > 0)
                        {
                            max = next;
                        }
                    }
                    vm.PushStack(max);
                })},

                {"min", new BuiltinFunctionValue((vm, args) =>
                {
                    var max = args.Get(0);
                    for (var i = 1; i < args.Length; i++)
                    {
                        var next = args[i];
                        if (next.CompareTo(max) < 0)
                        {
                            max = next;
                        }
                    }
                    vm.PushStack(max);
                })},

                {"inc", IncNumber},
                {"dec", DecNumber},
            };

            result.Define("math", new ObjectValue(mathFunctions));

            return result;
        }

        public static readonly BuiltinFunctionValue IncNumber = new BuiltinFunctionValue((vm, args) =>
        {
            vm.PushStack(new NumberValue(args.Get<NumberValue>(0).Value + 1));
        });

        public static readonly BuiltinFunctionValue DecNumber = new BuiltinFunctionValue((vm, args) =>
        {
            vm.PushStack(new NumberValue(args.Get<NumberValue>(0).Value - 1));
        });

        #endregion
    }
}