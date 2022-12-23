using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class StandardAssertLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var assertFunctions = new Dictionary<string, IValue>
            {
                {"true", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<BoolValue>(0);
                    if (!top.Value)
                    {
                        vm.Running = false;
                        Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                        Console.WriteLine($"Assert expected true");
                    }
                }, "assert.true")},

                {"false", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<BoolValue>(0);
                    if (top.Value)
                    {
                        vm.Running = false;
                        Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                        Console.WriteLine($"Assert expected false");
                    }
                }, "assert.false")},

                {"equals", new BuiltinFunctionValue((vm, args) =>
                {
                    var expected = args.GetIndex(0);
                    var actual = args.GetIndex(1);
                    if (expected.CompareTo(actual) != 0)
                    {
                        vm.Running = false;
                        Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                        Console.WriteLine($"Assert expected equals:\nExpected: {expected.ToString()}\nActual: {actual.ToString()}");
                    }
                }, "assert.equals")},

                {"notEquals", new BuiltinFunctionValue((vm, args) =>
                {
                    var expected = args.GetIndex(0);
                    var actual = args.GetIndex(1);
                    if (expected.CompareTo(actual) == 0)
                    {
                        vm.Running = false;
                        Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                        Console.WriteLine($"Assert expected not equals:\nActual: {actual.ToString()}");
                    }
                }, "assert.notEquals")}
            };

            result.TryDefine("assert", new ObjectValue(assertFunctions));

            return result;
        }
        #endregion
    }
}