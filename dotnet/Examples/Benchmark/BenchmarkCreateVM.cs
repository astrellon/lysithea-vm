using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using NLua;

namespace LysitheaVM
{
    [MemoryDiagnoser]
    public class BenchmarkCreateVM
    {
        #region Fields
        private const string PathOffset = "../../../../../../../";
        private static readonly Random Rand = new Random();
        #endregion

        #region Methods
        [Benchmark]
        public void TestCreateNLua()
        {
            using (var lua = new Lua())
            {
            }
        }

        [Benchmark]
        public void TestCreateMoonSharp()
        {
            var script = new MoonSharp.Interpreter.Script();
        }

        [Benchmark]
        public void TestCreateLys()
        {
            var vm = new VirtualMachine(8);
        }
        #endregion
    }
}