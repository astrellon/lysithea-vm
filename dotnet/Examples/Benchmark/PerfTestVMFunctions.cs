using System;
using System.Linq;

namespace LysitheaVM
{
    public static class PerfTestVMFunctions
    {
        private static readonly Random Rand = new Random();

        #region Methods
        public static BuiltinFunctionValue FuncRand = new BuiltinFunctionValue((vm, args) =>
        {
            vm.PushStack(Rand.NextDouble());
        });

        public static BuiltinFunctionValue FuncAdd = new BuiltinFunctionValue((vm, args) =>
        {
            var num1 = args.GetIndexDouble(0);
            var num2 = args.GetIndexDouble(1);
            vm.PushStack((num1 + num2));
        });

        public static BuiltinFunctionValue FuncLessThan = new BuiltinFunctionValue((vm, args) =>
        {
            var left = args.GetIndexDouble(0);
            var right = args.GetIndexDouble(1);
            vm.PushStack(left < right);
        });

        public static BuiltinFunctionValue FuncPrint = new BuiltinFunctionValue((vm, args) =>
        {
            Console.WriteLine(string.Join("", args.Value));
        });
        #endregion
    }
}