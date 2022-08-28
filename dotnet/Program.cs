using System;
using System.IO;
using System.Linq;

namespace SimpleStackVM
{
    public static class Program
    {
        private static Random Rand = new Random();
        private static int Counter = 0;

        #region Methods
        public static void Main(string[] args)
        {
#if RELEASE
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../../examples/perfTest.json"));
#else
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../examples/perfTest.json"));
#endif
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine(64, OnRunCommandPerfTest);
            vm.AddScopes(scopes);

            try
            {
                vm.Run("Main");
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

        private static void OnRunCommandPerfTest(IValue command, VirtualMachine vm)
        {
            var commandName = command.ToString();
            if (commandName == "rand")
            {
                vm.PushStack((NumberValue)Rand.NextDouble());
                return;
            }
            if (commandName == "add")
            {
                var num1 = vm.PopStack<NumberValue>();
                var num2 = vm.PopStack<NumberValue>();
                vm.PushStack((NumberValue)(num1.Value + num2.Value));
                return;
            }
            if (commandName == "isDone")
            {
                Counter++;
                vm.PushStack((BoolValue)(Counter >= 1_000_000));
                return;
            }
            if (commandName == "done")
            {
                var total = vm.PopStack<NumberValue>();
                Console.WriteLine($"Done: {total.Value}");
            }
        }
        #endregion
    }
}