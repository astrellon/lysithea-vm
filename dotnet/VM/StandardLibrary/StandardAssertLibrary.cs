using System;
using System.Collections.Generic;

namespace SimpleStackVM
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

            var assertFunctions = new Dictionary<string, IValue>();
            assertFunctions["true"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<BoolValue>();
                if (!top.Value)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected true");
                }
            });

            assertFunctions["false"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<BoolValue>();
                if (top.Value)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected false");
                }
            });

            assertFunctions["equals"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var actual = vm.PopStack();
                var expected = vm.PopStack();
                if (expected.CompareTo(actual) != 0)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected equals:\nExpected: {expected.ToString()}\nActual: {actual.ToString()}");
                }
            });

            assertFunctions["notEquals"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var actual = vm.PopStack();
                var expected = vm.PopStack();
                if (expected.CompareTo(actual) == 0)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected not equals:\nExpected: {expected.ToString()}\nActual: {actual.ToString()}");
                }
            });

            result.Define("assert", new ObjectValue(assertFunctions));

            return result;
        }
        #endregion
    }
}