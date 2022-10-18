#if BENCHMARK
using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace SimpleStackVM
{
    public static class Benchmarks
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkClass>();
        }
    }

    [MemoryDiagnoser]
    public class BenchmarkClass
    {
        #region Fields
        private static Random Rand = new Random();
        private static int Counter = 0;
        private static double Total = 0.0;
        private static string CodeText = File.ReadAllText("/home/alan/git/simple-stack-vm/examples/perfTest.lisp");

        private static readonly Scope CustomScope = CreateScope();
        private static readonly VirtualMachineAssembler Assembler = CreateAssembler();
        private static readonly Script Code = Assembler.ParseFromText(CodeText);
        private static readonly VirtualMachine SharedVM = new VirtualMachine(8);
        #endregion

        #region Methods
        private static VirtualMachineAssembler CreateAssembler()
        {
            var assembler = new VirtualMachineAssembler();
            assembler.BuiltinScope.CombineScope(CustomScope);
            return assembler;
        }

        [Benchmark]
        public void TestControl()
        {
            Counter = 0;
            Total = 0.0;

            do
            {
                DoStep();
            }
            while (!DoIsDone());
            DoDone();
        }

        private static void DoStep()
        {
            var num1 = Rand.NextDouble();
            var num2 = Rand.NextDouble();
            Total += num1 + num2;
        }

        private static bool DoIsDone()
        {
            Counter++;
            return Counter >= 1_000_000;
        }

        private static void DoDone()
        {
            Console.WriteLine($"Done: {Total}");
        }

        [Benchmark]
        public void TestCreateAndExecute()
        {
            Counter = 0;

            var vm = new VirtualMachine(8);
            try
            {
                vm.Execute(Code);
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

        [Benchmark]
        public void TestExecute()
        {
            Counter = 0;

            SharedVM.Reset();

            try
            {
                SharedVM.Execute(Code);
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

            result.Define("rand", (vm, numArgs) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.Define("add", (vm, numArgs) =>
            {
                var num1 = vm.PopStack<NumberValue>();
                var num2 = vm.PopStack<NumberValue>();
                vm.PushStack((num1.Value + num2.Value));
            });

            result.Define("isDone", (vm, numArgs) =>
            {
                Counter++;
                vm.PushStack((Counter >= 1_000_000));
            });

            result.Define("done", (vm, numArgs) =>
            {
                var total = vm.PopStack<NumberValue>();
                Console.WriteLine($"Done: {total.Value}");
            });

            return result;
        }
        #endregion
    }
}
#endif