using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM
{
    public static class Program
    {
        private static ObjectValue Player = new ObjectValue(new Dictionary<string, IValue>
        {
            { "name", (StringValue)"Priya" },
            { "age", (NumberValue)30 }
        });

        private static ObjectValue ShopKeep = new ObjectValue(new Dictionary<string, IValue>
        {
            { "name", (StringValue)"Shop Keeper" },
            { "age", (NumberValue)32 }
        });

        private static Random Rand = new Random();
        private static int Counter = 0;

        #region Methods
        public static void Main(string[] args)
        {
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("perfTest.json"));
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine(64, OnRunCommandPerfTest);
            vm.AddScopes(scopes);

            // var disassembled = VirtualMachineDisassembler.Disassemble(vm);
            // File.WriteAllText("output.json", disassembled.ToString(4));

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

        private static void OnRunCommand(IValue command, VirtualMachine vm)
        {
            var commandName = command.ToString();
            if (commandName == "add")
            {
                var num1 = vm.PopStack<NumberValue>();
                var num2 = vm.PopStack<NumberValue>();
                vm.PushStack((NumberValue)(num1.Value + num2.Value));
            }
            else if (commandName == "isMorning")
            {
                vm.PushStack((BoolValue)false);
            }
            else if (commandName == "prepend")
            {
                var top = vm.PopStackString();
                var top2 = vm.PopStackString();
                vm.PushStack((StringValue)(top + top2));
            }
            else if (commandName == "append")
            {
                var top = vm.PopStackString();
                var top2 = vm.PopStackString();
                vm.PushStack((StringValue)(top2 + top));
            }
            else if (commandName == "text")
            {
                var text = vm.PopStackString();
                Console.WriteLine($"TEXT [{vm.CurrentScope.ScopeName}:{vm.ProgramCounter}]: {text}");
            }
        }
        #endregion
    }
}