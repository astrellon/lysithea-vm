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
            var assembler = new VirtualMachineLispAssembler();
            var code = assembler.ParseFromText(File.ReadAllText("../examples/testProgram.lisp"));

            var vm = new VirtualMachine(64);
            StandardLibrary.AddToVirtualMachine(vm);
            vm.AddBuiltinScope(CustomScope);

            vm.SetGlobalCode(code);
            try
            {
                vm.Running = true;
                var sw = Stopwatch.StartNew();
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

            result.Define("print", new BuiltinProcedureValue(vm =>
            {
                var top = vm.PopStack();
                Console.WriteLine($"Print: {top.ToString()}");
            }));

            result.Define("done", new BuiltinProcedureValue(vm =>
            {
                Console.WriteLine("Done!");
            }));

            result.Define("rand", new BuiltinProcedureValue(vm =>
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
            }));

            return result;
        }
        #endregion
    }
}