using System;
using System.Linq;
using SimpleStackVM.Extensions;

namespace SimpleStackVM
{
    public static class StandardStringLibrary
    {
        #region Fields
        public const string HandleName = "string";
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
                // String Operators
                case "append":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new StringValue(left.ToString() + right.ToString()));
                        break;
                    }
                case "prepend":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new StringValue(right.ToString() + left.ToString()));
                        break;
                    }
                case "length":
                    {
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(new NumberValue(top.Value.Length));
                        break;
                    }
                case "get":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(new StringValue(top.Value[(int)index.Value].ToString()));
                        break;
                    }
                case "set":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(top.Set((int)index, value.ToString()));
                        break;
                    }
                case "insert":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(top.Insert((int)index, value.ToString()));
                        break;
                    }
                case "substring":
                    {
                        var length = vm.PopStack<NumberValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(top.SubString((int)index.Value, (int)length.Value));
                        break;
                    }
            }
        }
        #endregion
    }
}