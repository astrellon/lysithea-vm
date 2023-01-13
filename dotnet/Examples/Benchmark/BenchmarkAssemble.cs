using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using NLua;

namespace LysitheaVM
{
    [MemoryDiagnoser]
    public class BenchmarkAssemble
    {
        #region Fields
        private const string PathOffset = "../../../../../../../";
        private static readonly Random Rand = new Random();
        #endregion

        #region Methods
        [Benchmark]
        public void TestCreateShortScriptNLua()
        {
            using (var lua = new PerfNLua())
            {
                var func = lua.State.LoadFile(Path.Combine(PathOffset, "shortScript.lua"));
                lua.Execute(func);
            }
        }
        [Benchmark]
        public void TestCreateMidScriptNLua()
        {
            using (var lua = new PerfNLua())
            {
                var func = lua.State.LoadFile(Path.Combine(PathOffset, "midScript.lua"));
                lua.Execute(func);
            }
        }

        // [Benchmark]
        // public void TestCreateShortScriptMoonSharp()
        // {
        //     var script = PerfMoonSharp.CreateScript();
        //     script.LoadFile(Path.Combine(PathOffset, "shortScript.lua"));
        // }
        // [Benchmark]
        // public void TestCreateMidScriptMoonSharp()
        // {
        //     var script = PerfMoonSharp.CreateScript();
        //     script.LoadFile(Path.Combine(PathOffset, "midScript.lua"));
        // }

        // [Benchmark]
        // public void TestCreateShortScriptLys()
        // {
        //     var perfVM = new PerfTestVM();
        //     var script = perfVM.Assembler.ParseFromFile(Path.Combine(PathOffset, "shortScript.lys"));
        // }

        // [Benchmark]
        // public void TestCreateMidScriptLys()
        // {
        //     var perfVM = new PerfTestVM();
        //     var script = perfVM.Assembler.ParseFromFile(Path.Combine(PathOffset, "midScript.lys"));
        // }
        #endregion
    }
}