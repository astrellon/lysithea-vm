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

        #region Methods
        public static void Main(string[] args)
        {
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../examples/perfTest.json"));
            var procedures = VirtualMachineJsonAssembler.ParseProcedures(json.AsArray);

            var vm = new VirtualMachine(64, OnRunCommand);
            vm.AddProcedures(procedures);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.SetCurrentProcedure("Main");
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

        private static void OnRunCommand(string command, VirtualMachine vm)
        {
            if (command == "rand")
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
            }
            else if (command == "add")
            {
                var num1 = vm.PopStack<NumberValue>();
                var num2 = vm.PopStack<NumberValue>();
                vm.PushStack((NumberValue)(num1.Value + num2.Value));
            }
            else if (command == "isDone")
            {
                Counter++;
                vm.PushStack((BoolValue)(Counter >= 1_000_000));
            }
            else if (command == "done")
            {
                var total = vm.PopStack<NumberValue>();
                Console.WriteLine($"Done: {total.Value}");
            }
        }
        #endregion
    }
}