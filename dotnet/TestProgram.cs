using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM
{
    public static class TestProgram
    {
        private static readonly Random Rand = new Random();
        #region Methods
        public static void Main(string[] args)
        {
            var file = File.ReadAllText("../examples/perfTest.lisp");
            // var file = @"(procedure main() (set x 5) (print x))";
            var tokens = VirtualMachineLispParser.Tokenize(file);
            var parsed = VirtualMachineLispParser.ReadAllTokens(tokens);

            var assembler = new VirtualMachineLispAssembler();
            var code = assembler.ParseProcedures(parsed).ToList();

            var vm = new VirtualMachine(64, OnRunCommand);
            StandardLibrary.AddToVirtualMachine(vm);
            vm.AddProcedures(code);
            try
            {
                vm.SetCurrentProcedure("main");
                vm.Running = true;
                while (vm.Running && !vm.Paused)
                {
                    vm.Step();
                }
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

        private static void OnRunCommand(string command, VirtualMachine vm)
        {
            if (command == "print")
            {
                var top = vm.PopStack();
                Console.WriteLine($"Print: {top.ToString()}");
            }
            else if (command == "done")
            {
                Console.WriteLine("Done!");
            }
            else if (command == "rand")
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
            }
            else
            {
                Console.WriteLine($"Unknown command: {command}");
            }
        }
        #endregion
    }
}