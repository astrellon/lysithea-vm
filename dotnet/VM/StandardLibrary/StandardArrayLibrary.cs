using System;
using System.Linq;
using SimpleStackVM.Extensions;

namespace SimpleStackVM
{
    public static partial class StandardArrayLibrary
    {
        #region Fields
        public const string HandleName = "array";
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
                case "append":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<ArrayValue>();
                        vm.PushStack(left.Append(right));
                        break;
                    }
                case "prepend":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<ArrayValue>();
                        vm.PushStack(left.Prepend(right));
                        break;
                    }
                case "concat":
                    {
                        var right = vm.PopStack<ArrayValue>();
                        var left = vm.PopStack<ArrayValue>();
                        vm.PushStack(left.Concat(right));
                        break;
                    }
                case "length":
                    {
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(new NumberValue(top.Value.Count));
                        break;
                    }
                case "set":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Set((int)index, value));
                        break;
                    }
                case "get":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Value[(int)index]);
                        break;
                    }
                case "insert":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Insert((int)index, value));
                        break;
                    }
                case "removeAt":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.RemoveAt((int)index));
                        break;
                    }
                case "remove":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Remove(value));
                        break;
                    }
                case "contains":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(new BoolValue(top.Contains(value)));
                        break;
                    }
                case "indexOf":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(new NumberValue(top.IndexOf(value)));
                        break;
                    }
                case "subList":
                    {
                        var length = vm.PopStack<NumberValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.SubList((int)index.Value, (int)length.Value));
                        break;
                    }
            }
        }
        #endregion
    }
}