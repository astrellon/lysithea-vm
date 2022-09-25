using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
                        vm.PushStack(top.Set(GetIndex(top, index), value));
                        break;
                    }
                case "get":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Value[GetIndex(top, index)]);
                        break;
                    }
                case "insert":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.Insert(GetIndex(top, index), value));
                        break;
                    }
                case "insertFlatten":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.InsertFlatten(GetIndex(top, index), value));
                        break;
                    }
                case "removeAt":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.RemoveAt(GetIndex(top, index)));
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
                case "sublist":
                    {
                        var length = vm.PopStack<NumberValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(top.SubList(GetIndex(top, index), length.IntValue));
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndex(ArrayValue value, NumberValue input)
        {
            var index = input.IntValue;
            if (index < 0)
            {
                return value.Value.Count + index + 1;
            }

            return index;
        }
        #endregion
    }
}