using System;

namespace LysitheaVM
{
    public class PerfTestVM
    {
        #region Fields
        private static readonly IReadOnlyScope PerfTestScope = CreateScope();
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

            result.Define("rand", PerfTestVMFunctions.FuncRand);
            result.Define("add", PerfTestVMFunctions.FuncAdd);
            result.Define("lessThan", PerfTestVMFunctions.FuncLessThan);
            result.Define("print", PerfTestVMFunctions.FuncPrint);

            return result;
        }
        #endregion
    }
}