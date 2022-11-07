using System;

namespace SimpleStackVM
{
    public class PerfTestVM
    {
        #region Fields
        private static readonly IReadOnlyScope PerfTestScope = CreateScope();
        private static readonly Random Rand = new Random();
        public readonly VirtualMachine VM;
        public readonly VirtualMachineAssembler Assembler;
        #endregion

        #region Constructor
        public PerfTestVM()
        {
            this.VM = new VirtualMachine(8);
            this.Assembler = new VirtualMachineAssembler();
            this.Assembler.BuiltinScope.CombineScope(PerfTestScope);
        }
        #endregion

        #region Methods
        private static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("rand", (vm, args) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.Define("add", (vm, args) =>
            {
                var num1 = args.GetIndexDouble(0);
                var num2 = args.GetIndexDouble(1);
                vm.PushStack((num1 + num2));
            });

            result.Define("lessThan", (vm, args) =>
            {
                var left = args.GetIndexDouble(0);
                var right = args.GetIndexDouble(1);
                vm.PushStack(left < right);
            });

            result.Define("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}