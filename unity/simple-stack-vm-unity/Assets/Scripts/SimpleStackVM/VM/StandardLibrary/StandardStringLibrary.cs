using System;
using System.Linq;

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
                        vm.PushStack(Get(top, index.IntValue));
                        break;
                    }
                case "set":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(Set(top, index.IntValue, value.ToString()));
                        break;
                    }
                case "insert":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(Insert(top, index.IntValue, value.ToString()));
                        break;
                    }
                case "substring":
                    {
                        var length = vm.PopStack<NumberValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(SubString(top, index.IntValue, length.IntValue));
                        break;
                    }
                case "removeAt":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(RemoveAt(top, index.IntValue));
                        break;
                    }
                case "removeAll":
                    {
                        var values = vm.PopStack<StringValue>();
                        var top = vm.PopStack<StringValue>();
                        vm.PushStack(RemoveAll(top, values.Value));
                        break;
                    }
            }
        }

        public static StringValue Set(StringValue self, int index, string value)
        {
            index = self.GetIndex(index);
            var left = self.Value.Substring(0, index);
            var right = self.Value.Substring(index + 1);
            return new StringValue($"{left}{value}{right}");
        }

        public static StringValue Get(StringValue self, int index)
        {
            return new StringValue(self.Value[self.GetIndex(index)].ToString());
        }

        public static StringValue Insert(StringValue self, int index, string value)
        {
            return new StringValue(self.Value.Insert(self.GetIndex(index), value));
        }

        public static StringValue SubString(StringValue self, int index, int length)
        {
            return new StringValue(self.Value.Substring(self.GetIndex(index), length));
        }

        public static StringValue RemoveAt(StringValue self, int index)
        {
            return new StringValue(self.Value.Remove(self.GetIndex(index), 1));
        }

        public static StringValue RemoveAll(StringValue self, string values)
        {
            return new StringValue(self.Value.Replace(values, ""));
        }
        #endregion
    }
}