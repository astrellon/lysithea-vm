using System;
using System.Linq;
using SimpleStackVM.Extensions;

namespace SimpleStackVM
{
    public static class StandardObjectLibrary
    {
        #region Fields
        public const string HandleName = "object";
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
                // Object Operators
                case "set":
                    {
                        var value = vm.PopStack();
                        var key = vm.PopStack<StringValue>();
                        var obj = vm.PopStack<ObjectValue>();
                        vm.PushStack(obj.Set(key, value));
                        break;
                    }
                case "get":
                    {
                        var key = vm.PopStack<StringValue>();
                        var obj = vm.PopStack<ObjectValue>();
                        if (obj.TryGetValue(key, out var value))
                        {
                            vm.PushStack(value);
                        }
                        else
                        {
                            vm.PushStack(NullValue.Value);
                        }
                        break;
                    }
                case "keys":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        var keys = top.Value.Keys.Select(k => new StringValue(k)).Cast<IValue>().ToList();
                        var list = new ArrayValue(keys);
                        vm.PushStack(list);
                        break;
                    }
                case "values":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        var list = new ArrayValue(top.Value.Values.ToList());
                        vm.PushStack(list);
                        break;
                    }
                case "length":
                    {
                        var top = vm.PeekStack<ObjectValue>();
                        vm.PushStack(new NumberValue(top.Value.Count));
                        break;
                    }
            }
        }
        #endregion
    }
}