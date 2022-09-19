using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardLibraryProgram
    {
        #region Methods
        public static void Main(string[] args)
        {
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../examples/testStandardLibrary.json"));
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine(64, new[] { StandardLibrary.Standard, OnRunCommand });
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

        private static bool OnRunCommand(IValue input, VirtualMachine vm)
        {
            if (input is StringValue stringValue)
            {
                var command = stringValue.Value;

                switch (command)
                {
                    case "print":
                    {
                        var top = vm.PopStack();
                        Console.WriteLine($"Print: {top.ToString()}");
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}