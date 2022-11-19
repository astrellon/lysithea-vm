using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace LysitheaVM
{
    [MemoryDiagnoser]
    public class BenchmarkPerfTest
    {
        #region Fields
        private static readonly Random Rand = new Random();
        private const string PathOffset = "../../../../../../../";
        private static readonly string VMCodeText = File.ReadAllText(Path.Combine(PathOffset, "perfTest.lys"));
        private static readonly string LuaCodeText = File.ReadAllText(Path.Combine(PathOffset, "perfTest.lua"));
        private static readonly PerfTestVM PerfVM = new PerfTestVM();
        private static readonly Script PreAssembledScript = AssembleScript();
        private static readonly PerfNLua PerfLuaVM = new PerfNLua();
        private static readonly NLua.LuaFunction PreCompiledLua = PerfLuaVM.Compile(LuaCodeText);
        private static MoonSharp.Interpreter.DynValue MoonSharpMainFunc;
        private static MoonSharp.Interpreter.Script MoonSharpScript = PerfMoonSharp.Compile(LuaCodeText, out MoonSharpMainFunc);
        #endregion

        #region Methods
        private static Script AssembleScript()
        {
            return PerfVM.Assembler.ParseFromText(VMCodeText);
        }

        [Benchmark]
        public void TestControl()
        {
            var controlPerf = new PerfControl();
            controlPerf.Run();
        }

        [Benchmark]
        public void TestExecuteIdealVM()
        {
            var vm = new VirtualMachine(8);
            vm.Execute(PerfTestIdealVM.IdealScript);
        }

        [Benchmark]
        public void TestExecuteIdealVM2()
        {
            var vm = new VirtualMachine(8);
            vm.Execute(PerfTestIdealVM.IdealScript);
        }

        [Benchmark]
        public void TestCreateAndExecuteVM()
        {
            var vm = new PerfTestVM();
            var script = vm.Assembler.ParseFromText(VMCodeText);
            vm.VM.Execute(script);
        }

        [Benchmark]
        public void TestExecuteVM()
        {
            PerfVM.VM.Reset();
            PerfVM.VM.Execute(PreAssembledScript);
        }

        [Benchmark]
        public void TestExecuteNLua()
        {
            PerfLuaVM.Execute(PreCompiledLua);
        }

        [Benchmark]
        public void TestCreateAndExecuteNLua()
        {
            var lua = new PerfNLua();
            lua.Execute(LuaCodeText);
        }

        [Benchmark]
        public void TestExecuteMoonSharp()
        {
            MoonSharpScript.Call(MoonSharpMainFunc);
        }

        [Benchmark]
        public void TestCreateAndExecuteMoonSharp()
        {
            var script = PerfMoonSharp.Compile(LuaCodeText, out var mainFunc);
            script.Call(mainFunc);
        }

        #endregion
    }
}