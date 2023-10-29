using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace LysitheaVM.Example
{
    public static class SnapshotProgram
    {
        private static readonly Scope CustomScope = CreateScope();
        private static Snapshot CreatedSnapshot;
        #region Methods

        public static void Main(string[] args)
        {
            var assembler = new Assembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(CustomScope);
            var script = assembler.ParseFromFile("../../../examples/testSnapshot.lys");

            var vm = new VirtualMachine(16);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Execute(script);
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

            Console.WriteLine("Stopped after creating snapshot");

            try
            {
                var sw = Stopwatch.StartNew();
                vm.FromSnapshot(script, CreatedSnapshot);
                if (vm.Running && !vm.Paused)
                {
                    vm.Execute();
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

            result.TrySetConstant("make-snapshot", (vm, args) =>
            {
                CreatedSnapshot = vm.CreateSnapshot();
                Console.WriteLine("Made snapshot");
                vm.Running = false;
            });

            return result;
        }
        #endregion
    }
}