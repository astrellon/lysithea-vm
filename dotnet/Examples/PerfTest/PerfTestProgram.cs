using System;
using System.Diagnostics;
using System.Linq;

namespace SimpleStackVM
{
    public static class PerfTestProgram
    {
        private static Random Rand = new Random();

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
                var num1 = args.GetIndexDouble(0);
                var num2 = args.GetIndexDouble(1);
                vm.PushStack((num1 + num2));
            });

            result.Define("lessThan", (vm, args) =>
            {
                var left = args.GetIndexDouble(0);
                var right = args.GetIndexDouble(1);
                vm.PushStack(left < right);
            });

            result.Define("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}