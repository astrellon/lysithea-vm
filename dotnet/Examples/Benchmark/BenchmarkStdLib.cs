using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace LysitheaVM
{
    [MemoryDiagnoser]
    public class BenchmarkStdLib
    {
        #region Fields
        private const string PathOffset = "../../../../../../../";
        private static string CodeText = File.ReadAllText(Path.Combine(PathOffset, "testStandardLibraryNoAssert.lisp"));
        private static readonly VirtualMachineAssembler Assembler = CreateAssembler();
        private static readonly Script Code = Assembler.ParseFromText(CodeText);
        private static readonly VirtualMachine SharedVM = new VirtualMachine(8);
        #endregion

        #region Methods
        private static VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            return assembler;
        }

        [Benchmark]
        public void TestStdLibReuseCode()
        {
            SharedVM.Reset();
            SharedVM.Execute(Code);
        }

        [Benchmark]
        public void TestStdLibCreateCode()
        {
            var code = Assembler.ParseFromText(CodeText);
            var vm = new VirtualMachine(8);
            vm.Execute(code);
        }

        [Benchmark]
        public void TestStdLibStreamFile()
        {
            var code = Assembler.ParseFromFile(FilePath);
            var vm = new VirtualMachine(8);
            vm.Execute(code);
        }
        #endregion
    }
}