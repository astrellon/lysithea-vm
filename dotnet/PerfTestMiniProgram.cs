using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM
{
    public static class PerfTestMiniProgram
    {
        private static Random Rand = new Random();
        private static int Counter = 0;

        #region Methods
        public static void Main(string[] args)
        {
            var scope = new Scope("Main", new []
            {
                new CodeLine(Operator.Push, new NumberValue(0)),
                new CodeLine(Operator.Run, new StringValue("rand")),
                new CodeLine(Operator.Run, new StringValue("rand")),
                new CodeLine(Operator.Run, new StringValue("add")),
                new CodeLine(Operator.Run, new StringValue("add")),
                new CodeLine(Operator.Run, new StringValue("isDone")),
                new CodeLine(Operator.JumpFalse, new StringValue(":Start")),
                new CodeLine(Operator.Run, new StringValue("done")),
            },
            new Dictionary<string, int>
            {
                { ":Start", 1 }
            });

            var vm = new VirtualMachineMini(32, OnRunCommand);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.SetCurrentScope(scope);
                vm.Running = true;
                while (vm.Running)
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

        private static void OnRunCommand(string command, VirtualMachineMini vm)
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