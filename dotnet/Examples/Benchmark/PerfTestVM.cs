using System;

namespace LysitheaVM
{
    public class PerfTestVM
    {
        #region Fields
        private static Random Rand = new Random();
        private static readonly IReadOnlyScope PerfTestScope = CreateScope();
        public readonly VirtualMachine VM;
        public readonly Assembler Assembler;
        #endregion

        #region Constructor
        public PerfTestVM()
        {
            this.VM = new VirtualMachine(8);
            this.Assembler = new Assembler();
            this.Assembler.BuiltinScope.CombineScope(PerfTestScope);
        }
        #endregion

        #region Methods
        private static Scope CreateScope()
        {
            var result = new Scope();

            result.TrySetConstant("rand", (vm, args) =>
            {
                vm.PushStack(Rand.NextDouble());
            });
            result.TrySetConstant("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}