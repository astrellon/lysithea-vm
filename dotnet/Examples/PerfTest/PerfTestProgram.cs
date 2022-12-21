using System;
using System.Diagnostics;
using System.Linq;

namespace LysitheaVM
{
    public static class PerfTestProgram
    {
        private static Random Rand = new Random();

        private static readonly Scope PerfTestScope = CreateScope();
        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new Assembler();
            assembler.BuiltinScope.CombineScope(PerfTestScope);
            var script = assembler.ParseFromFile("../../../examples/perfTest.lys");

            var vm = new VirtualMachine(8);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Execute(script);
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");

                vm.Reset();
                sw = Stopwatch.StartNew();

                vm.Execute(script);
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

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