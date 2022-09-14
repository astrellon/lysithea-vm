using System;
using System.IO;
using System.Linq;

namespace SimpleStackVM
{
    public static class RunCommandProgram
    {
        #region Methods
        public static void Run()
        // public static void Main(string[] args)
        {
#if RELEASE
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../../examples/testRunCommands.json"));
#else
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../examples/testRunCommands.json"));
#endif
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine(64, OnRunCommand);
            vm.AddScopes(scopes);

            try
            {
                vm.SetCurrentScope("Main");
                vm.SetRunning(true);
                while (vm.IsRunning && !vm.IsPaused)
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

        private static void OnRunCommand(IValue command, VirtualMachine vm)
        {
            if (command is StringValue stringValue)
            {
                switch (stringValue.Value)
                {
                    case "add":
                    {
                        var num1 = vm.PopStack<NumberValue>();
                        var num2 = vm.PopStack<NumberValue>();
                        vm.PushStack(new NumberValue(num1.Value + num2.Value));
                        break;
                    }
                    case "print":
                    {
                        var top = vm.PopStack();
                        Console.WriteLine($"Print: {top.ToString()}");
                        break;
                    }
                    default:
                    {
                        Console.WriteLine($"Error! Unknown string run command: {stringValue.ToString()}");
                        break;
                    }
                }
            }
            else if (command is NumberValue numberValue)
            {
                Console.WriteLine($"The number: {numberValue.Value}");
            }
            else if (command is BoolValue booleanValue)
            {
                Console.WriteLine($"The boolean: {booleanValue.Value}");
            }
            else if (command is ArrayValue arrayValue)
            {
                Console.WriteLine($"The array command: {arrayValue.ToString()}");
                Console.WriteLine($"- Top array value: {vm.PopStack().ToString()}");
            }
            else if (command is ObjectValue objectValue)
            {
                Console.WriteLine($"The object command: {objectValue.ToString()}");
            }
        }
        #endregion
    }
}