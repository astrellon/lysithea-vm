using System;

namespace SimpleStackVM
{
    public static class StandardAssertLibrary
    {
        #region Fields
        public const string HandleName = "assert";
        #endregion

        #region Methods
        public static void AddHandler(VirtualMachine vm)
        {
            vm.AddBuiltinHandler(HandleName, Handler);
        }

        public static void Handler(string command, VirtualMachine vm)
        {
            switch (command)
            {
                case "true":
                    {
                        var top = vm.PopStack<BoolValue>();
                        if (!top.Value)
                        {
                            vm.Running = false;
                            Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                            Console.WriteLine($"Assert expected true");
                        }
                        break;
                    }
                case "false":
                    {
                        var top = vm.PopStack<BoolValue>();
                        if (top.Value)
                        {
                            vm.Running = false;
                            Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                            Console.WriteLine($"Assert expected false");
                        }
                        break;
                    }
                case "equals":
                    {
                        var toCompare = vm.PopStack();
                        var top = vm.PeekStack();
                        if (top.CompareTo(toCompare) != 0)
                        {
                            vm.Running = false;
                            Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                            Console.WriteLine($"Assert expected equals:\nExpected: {toCompare.ToString()}\nActual: {top.ToString()}");
                        }
                        break;
                    }
                case "notEquals":
                    {
                        var toCompare = vm.PopStack();
                        var top = vm.PeekStack();
                        if (top.CompareTo(toCompare) == 0)
                        {
                            vm.Running = false;
                            Console.WriteLine(string.Join("\n", vm.CreateStackTrace()));
                            Console.WriteLine($"Assert expected not equals:\nExpected: {toCompare.ToString()}\nActual: {top.ToString()}");
                        }
                        break;
                    }
            }
        }
        #endregion
    }
}