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
        private const string FileName = "testStandardLibraryNoAssert.lys";
        private static string CodeText = File.ReadAllText(Path.Combine(PathOffset, FileName));
        private static readonly Assembler Assembler = CreateAssembler();
        private static readonly Script Code = Assembler.ParseFromText(FileName, CodeText);
        private static readonly VirtualMachine SharedVM = new VirtualMachine(8);
        #endregion

        #region Methods
        private static Assembler CreateAssembler()
        {
            var assembler = new Assembler();
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
            var code = Assembler.ParseFromText(FileName, CodeText);
            var vm = new VirtualMachine(8);
            vm.Execute(code);
        }
        #endregion
    }
}