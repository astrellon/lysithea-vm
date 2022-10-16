using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace SimpleStackVM
{
    public static class TestProgram
    {
        private static readonly Random Rand = new Random();
        private static readonly Scope CustomScope = CreateScope();
        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineAssembler();
            assembler.BuiltinScope.CombineScope(StandardMathLibrary.Scope);
            assembler.BuiltinScope.CombineScope(StandardMiscLibrary.Scope);
            assembler.BuiltinScope.CombineScope(StandardOperators.Scope);
            assembler.BuiltinScope.CombineScope(CustomScope);
            var code = assembler.ParseFromText(File.ReadAllText("../examples/testProgram.lisp"));

            var vm = new VirtualMachine(16);
            vm.CurrentCode = code;

            try
            {
                vm.Running = true;
                var sw = Stopwatch.StartNew();
                while (vm.Running && !vm.Paused)
                {
                    vm.Step();
                }
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.Elapsed.TotalMilliseconds}ms");
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

            result.Define("done", (vm, numArgs) =>
            {
                Console.WriteLine("Done!");
            });

            result.Define("rand", (vm, numArgs) =>
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
            });

            return result;
        }
        #endregion
    }
}