using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace LysitheaVM
{
    [MemoryDiagnoser]
    public class BenchmarkStrings
    {
        #region Fields
        private static readonly VirtualMachineAssembler Assembler = CreateAssembler();
        private static readonly VirtualMachine SharedVM = new VirtualMachine(8);

        private static string CodeText1 = File.ReadAllText("/home/alan/git/simple-stack-vm/examples/benchmark1.lisp");
        private static string CodeText2 = File.ReadAllText("/home/alan/git/simple-stack-vm/examples/benchmark2.lisp");
        private static readonly Script Code1 = Assembler.ParseFromText(CodeText1);
        private static readonly Script Code2 = Assembler.ParseFromText(CodeText2);
        #endregion

        #region Methods
        private static VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            return assembler;
        }

        [Benchmark]
        public void TestStdLib()
        {
            SharedVM.Reset();
            SharedVM.Execute(Code1);
        }

        [Benchmark]
        public void TestObjectString()
        {
            SharedVM.Reset();
            SharedVM.Execute(Code2);
        }
        #endregion
    }
}