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

        private static readonly Scope CustomScope = CreateScope();

        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineLispAssembler();
            var code = assembler.ParseFromText(File.ReadAllText("../examples/perfTest.lisp"));

            var vm = new VirtualMachine(64);
            vm.AddBuiltinScope(CustomScope);
            vm.SetGlobalCode(code);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Running = true;
                while (vm.Running && !vm.Paused)
                {
                    vm.Step();
                }
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

            result.Define("rand", vm =>
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
            });

            result.Define("add", vm =>
            {
                var num1 = vm.PopStack<NumberValue>();
                var num2 = vm.PopStack<NumberValue>();
                vm.PushStack((NumberValue)(num1.Value + num2.Value));
            });

            result.Define("isDone", vm =>
            {
                Counter++;
                vm.PushStack((BoolValue)(Counter >= 1_000_000));
            });

            result.Define("done", vm =>
            {
                var total = vm.PopStack<NumberValue>();
                Console.WriteLine($"Done: {total.Value}");
            });

            return result;
        }
        #endregion
    }
}