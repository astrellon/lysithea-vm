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

        #region Methods
        public static void Main(string[] args)
        {
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("testCode.json"));
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine();
            vm.AddScopes(scopes);
            vm.OnRunCommand += OnRunCommand;

            // var disassembled = VirtualMachineDisassembler.Disassemble(vm);
            // File.WriteAllText("output.json", disassembled.ToString(4));

            try
            {
                vm.Run("Start");
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
            if (command == "add")
            {
                var num1 = vm.PopObject<NumberValue>();
                var num2 = vm.PopObject<NumberValue>();
                vm.PushStack((NumberValue)(num1.Value + num2.Value));
            }
            else if (command == "isMorning")
            {
                vm.PushStack((BoolValue)false);
            }
            else if (command == "prepend")
            {
                var top = vm.PopStackString();
                var top2 = vm.PopStackString();
                vm.PushStack((StringValue)(top + top2));
            }
            else if (command == "append")
            {
                var top = vm.PopStackString();
                var top2 = vm.PopStackString();
                vm.PushStack((StringValue)(top2 + top));
            }
            else if (command == "text")
            {
                var text = vm.PopStackString();
                Console.WriteLine($"TEXT [{vm.CurrentScope.ScopeName}:{vm.ProgramCounter}]: {text}");
            }
        }
        #endregion
    }
}