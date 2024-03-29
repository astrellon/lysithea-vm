using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace LysitheaVM.Example
{
    public static class SnapshotProgram
    {
        private static readonly Scope CustomScope = CreateScope();
        private static ObjectValue CreatedSnapshot;
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
                var snapshot = Snapshot.FromObject(CreatedSnapshot);
                snapshot.ApplyTo(vm, script);
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
                var snapshot = Snapshot.FromVirtualMachine(vm);
                CreatedSnapshot = snapshot.ToObject();
                var str = ToStringFormatted.Indented(CreatedSnapshot, 2);
                Console.WriteLine("Made snapshot: " + str);
                vm.Running = false;
            });

            return result;
        }
        #endregion
    }
}