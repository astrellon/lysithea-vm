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
            vm.OnGetVariable += OnGetVariable;
            vm.OnRunCommand += OnRunCommand;

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

        public static void OldMain(string[] args)
        {
            var code = new List<CodeLine>();
            code.Add(new CodeLine(Operator.Push, 5));
            code.Add(new CodeLine(Operator.Push, 12));
            code.Add(new CodeLine(Operator.Run, "add"));
            code.Add(new CodeLine(Operator.Push, "The result: "));
            code.Add(new CodeLine(Operator.Run, "append"));
            code.Add(new CodeLine(Operator.Run, "text"));
            code.Add(new CodeLine(Operator.Call, "[Questions]"));

            code.Add(new CodeLine(Operator.Push, "SELF: Hello there, how are you "));
            code.Add(new CodeLine(Operator.Run, "text"));
            code.Add(new CodeLine(Operator.Push, "{PLAYER.name}"));
            code.Add(new CodeLine(Operator.Run, "text"));
            code.Add(new CodeLine(Operator.Push, "?"));
            code.Add(new CodeLine(Operator.Run, "text"));
            // code.Add(new CodeLine(Operator.Text, "SELF: Hello there, how are you "));
            // code.Add(new CodeLine(Operator.Text, "{PLAYER.name}"));
            // code.Add(new CodeLine(Operator.Text, "?"));
            code.Add(new CodeLine(Operator.Push, Player));
            code.Add(new CodeLine(Operator.Run, "text"));

            // code.Add(new CodeLine(Operator.Run, "isMorning"));

            // code.Add(new CodeLine(Operator.JumpFalse, "NotMorning"));
            // code.Add(new CodeLine(Operator.Text, "PLAYER: Good morning "));
            // code.Add(new CodeLine(Operator.Text, "{SELF.name}"));
            // code.Add(new CodeLine(Operator.Jump, "AfterGreeting"));

            // code.Add(new CodeLine(Operator.Text, "PLAYER: Good evening "));
            // code.Add(new CodeLine(Operator.Text, "{SELF.name}"));

            code.Add(new CodeLine(Operator.Push, "Done!"));
            code.Add(new CodeLine(Operator.Run, "text"));

            var startScope = new Scope("Start", code);

            var vm = new VirtualMachine();
            vm.AddScope(startScope);
            vm.OnRunCommand += OnRunCommand;
            vm.OnGetVariable += OnGetVariable;

            code = new List<CodeLine>();
            code.Add(new CodeLine(Operator.Push, "Questions!"));
            code.Add(new CodeLine(Operator.Run, "text"));
            code.Add(new CodeLine(Operator.Push, "More Questions!"));
            code.Add(new CodeLine(Operator.Run, "text"));
            code.Add(new CodeLine(Operator.Return));
            var questionScope = new Scope("Questions", code);
            vm.AddScope(questionScope);

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

        private static IValue OnGetVariable(string varName, VirtualMachine vm)
        {
            if (varName == "SELF")
            {
                return ShopKeep;
            }
            else if (varName == "PLAYER")
            {
                return Player;
            }
            return (StringValue)"Unknown";
        }

        private static void OnRunCommand(string command, VirtualMachine vm)
        {
            if (command == "add")
            {
                var num1 = vm.PopStackInt();
                var num2 = vm.PopStackInt();
                vm.PushStack((NumberValue)(num1 + num2));
            }
            else if (command == "isMorning")
            {
                vm.PushStack((BoolValue)true);
            }
            else if (command == "append")
            {
                var top = vm.PopStackString();
                var top2 = vm.PopStackString();
                vm.PushStack((StringValue)(top + top2));
            }
            else if (command == "prepend")
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