using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimpleStackVM
{
    public static class PerfTestProgram
    {
        private static Random Rand = new Random();
        private static int Counter = 0;

        private static readonly Scope PerfTestScope = CreateScope();

        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineAssembler();
            assembler.BuiltinScope.CombineScope(PerfTestScope);
            var script = assembler.ParseFromFile("../../../examples/perfTest.lisp");

            var vm = new VirtualMachine(8);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Execute(script);
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms");

                vm.Reset();
                Counter = 0;
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

            result.Define("rand", (vm, args) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.Define("add", (vm, args) =>
            {
                var num1 = (NumberValue)vm.PopStack();
                var num2 = (NumberValue)vm.PopStack();
                vm.PushStack((num1.Value + num2.Value));
            });

            result.Define("isDone", (vm, args) =>
            {
                Counter++;
                vm.PushStack((Counter >= 1_000_000));
            });

            result.Define("done", (vm, args) =>
            {
                var total = (NumberValue)vm.PopStack();
                Console.WriteLine($"Done: {total.Value}");
            });

            return result;
        }
        #endregion
    }
}