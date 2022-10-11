
using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardComparisonLibrary
    {
        #region Fields
        public static readonly Scope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define(">", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) > 0));
            });

            result.Define(">=", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) >= 0));
            });

            result.Define("==", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) == 0));
            });

            result.Define("!=", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) != 0));
            });

            result.Define("<", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) < 0));
            });

            result.Define("<=", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new BoolValue(left.CompareTo(right) <= 0));
            });

            return result;
        }
        #endregion
    }
}