
using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardComparisonLibrary
    {
        #region Fields
        public const string HandleName = "comp";
        #endregion

        #region Methods
        public static void AddHandler(VirtualMachine vm)
        {
            vm.AddRunHandler(HandleName, Handler);
        }

        public static void Handler(string command, VirtualMachine vm)
        {
            switch (command)
            {
                // Comparison Operators
                case ">":
                case "greater":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) > 0));
                        break;
                    }
                case ">=":
                case "greaterEqual":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) >= 0));
                        break;
                    }
                case "==":
                case "equals":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) == 0));
                        break;
                    }
                case "!=":
                case "notEquals":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) != 0));
                        break;
                    }
                case "<":
                case "less":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) < 0));
                        break;
                    }
                case "<=":
                case "lessEqual":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new BoolValue(left.CompareTo(right) <= 0));
                        break;
                    }
                case "!":
                case "not":
                    {
                        var top = vm.PopStack<BoolValue>();
                        vm.PushStack(new BoolValue(!top.Value));
                        break;
                    }
            }
        }
        #endregion
    }
}