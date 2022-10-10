using System;

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

            result.Define("assert.true", vm =>
            {
                var top = vm.PopStack<BoolValue>();
                if (!top.Value)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected true");
                }
            });

            result.Define("assert.false", vm =>
            {
                var top = vm.PopStack<BoolValue>();
                if (top.Value)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected false");
                }
            });

            result.Define("assert.equals", vm =>
            {
                var toCompare = vm.PopStack();
                var top = vm.PopStack();
                if (top.CompareTo(toCompare) != 0)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected equals:\nExpected: {toCompare.ToString()}\nActual: {top.ToString()}");
                }
            });

            result.Define("assert.notEquals", vm =>
            {
                var toCompare = vm.PopStack();
                var top = vm.PopStack();
                if (top.CompareTo(toCompare) == 0)
                {
                    vm.Running = false;
                    Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                    Console.WriteLine($"Assert expected not equals:\nExpected: {toCompare.ToString()}\nActual: {top.ToString()}");
                }
            });

            return result;
        }
        #endregion
    }
}