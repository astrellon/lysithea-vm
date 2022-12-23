
using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class StandardMathLibrary
    {
        #region Fields
        public const double DegToRad = System.Math.PI / 180.0;

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
                    var top = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Sin(top.Value));
                }, "math.sin")},

                {"cos", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Cos(top.Value));
                }, "math.cos")},

                {"tan", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Tan(top.Value));
                }, "math.tan")},

                {"pow", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    var y = args.GetIndex<NumberValue>(1);
                    vm.PushStack(Math.Pow(x.Value, y.Value));
                }, "math.pow")},

                {"exp", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Exp(x.Value));
                }, "math.exp")},

                {"floor", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Floor(x.Value));
                }, "math.floor")},

                {"ceil", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Ceiling(x.Value));
                }, "math.ceil")},

                {"round", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Round(x.Value));
                }, "math.round")},

                {"isFinite", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(double.IsFinite(x.Value));
                }, "math.isFinite")},

                {"isNaN", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0);
                    vm.PushStack(double.IsNaN(x.Value));
                }, "math.isNaN")},

                {"parse", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex(0);
                    if (top is NumberValue)
                    {
                        vm.PushStack(top);
                    }
                    else
                    {
                        vm.PushStack(double.Parse(top.ToString()));
                    }
                }, "math.parse")},

                {"log", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Log(top.Value));
                }, "math.log")},

                {"abs", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<NumberValue>(0);
                    vm.PushStack(Math.Abs(top.Value));
                }, "math.abs")},

                {"max", new BuiltinFunctionValue((vm, args) =>
                {
                    var max = args.GetIndex(0);
                    for (var i = 1; i < args.Length; i++)
                    {
                        var next = args[i];
                        if (next.CompareTo(max) > 0)
                        {
                            max = next;
                        }
                    }
                    vm.PushStack(max);
                }, "math.max")},

                {"min", new BuiltinFunctionValue((vm, args) =>
                {
                    var min = args.GetIndex(0);
                    for (var i = 1; i < args.Length; i++)
                    {
                        var next = args[i];
                        if (next.CompareTo(min) < 0)
                        {
                            min = next;
                        }
                    }
                    vm.PushStack(min);
                }, "math.min")},

                {"sum", new BuiltinFunctionValue((vm, args) =>
                {
                    var total = 0.0;
                    foreach (NumberValue num in args.Value)
                    {
                        total += num.Value;
                    }
                    vm.PushStack(total);
                }, "math.sum")}
            };

            result.TryDefine("math", new ObjectValue(mathFunctions));

            return result;
        }

        #endregion
    }
}