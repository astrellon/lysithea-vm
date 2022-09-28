using System;
using System.Linq;

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
                        vm.PushStack(Append(left, right));
                        break;
                    }
                case "prepend":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack<ArrayValue>();
                        vm.PushStack(Prepend(left, right));
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
                        vm.PushStack(Set(top, index.IntValue, value));
                        break;
                    }
                case "get":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(Get(top, index.IntValue));
                        break;
                    }
                case "insert":
                    {
                        var value = vm.PopStack();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(Insert(top, index.IntValue, value));
                        break;
                    }
                case "insertFlatten":
                    {
                        var value = vm.PopStack<ArrayValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(InsertFlatten(top, index.IntValue, value));
                        break;
                    }
                case "remove":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(Remove(top, value));
                        break;
                    }
                case "removeAt":
                    {
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(RemoveAt(top, index.IntValue));
                        break;
                    }
                case "removeAll":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(RemoveAll(top, value));
                        break;
                    }
                case "contains":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(new BoolValue(Contains(top, value)));
                        break;
                    }
                case "indexOf":
                    {
                        var value = vm.PopStack();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(new NumberValue(IndexOf(top, value)));
                        break;
                    }
                case "sublist":
                    {
                        var length = vm.PopStack<NumberValue>();
                        var index = vm.PopStack<NumberValue>();
                        var top = vm.PopStack<ArrayValue>();
                        vm.PushStack(SubList(top, index.IntValue, length.IntValue));
                        break;
                    }
            }
        }

        public static ArrayValue Append(ArrayValue self, IValue input)
        {
            return new ArrayValue(self.Value.Append(input).ToList());
        }

        public static ArrayValue Prepend(ArrayValue self, IValue input)
        {
            return new ArrayValue(self.Value.Prepend(input).ToList());
        }

        public static ArrayValue Set(ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue[self.GetIndex(index)] = input;
            return new ArrayValue(newValue);
        }

        public static IValue Get(ArrayValue self, int index)
        {
            return self.Value[self.GetIndex(index)];
        }

        public static ArrayValue Insert(ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.Insert(self.GetIndex(index), input);
            return new ArrayValue(newValue);
        }

        public static ArrayValue InsertFlatten(ArrayValue self, int index, ArrayValue input)
        {
            var newValue = self.Value.ToList();
            newValue.InsertRange(self.GetIndex(index), input.Value);
            return new ArrayValue(newValue);
        }

        public static ArrayValue RemoveAt(ArrayValue self, int index)
        {
            var newValue = self.Value.ToList();
            newValue.RemoveAt(self.GetIndex(index));
            return new ArrayValue(newValue);
        }

        public static ArrayValue Remove(ArrayValue self, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.Remove(input);
            return new ArrayValue(newValue);
        }

        public static ArrayValue RemoveAll(ArrayValue self, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.RemoveAll(v => v.CompareTo(input) == 0);
            return new ArrayValue(newValue);
        }

        public static bool Contains(ArrayValue self, IValue input)
        {
            return self.Value.Contains(input);
        }

        public static int IndexOf(ArrayValue self, IValue input)
        {
            for (var i = 0; i < self.Value.Count; i++)
            {
                if (input.Equals(self.Value[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static ArrayValue SubList(ArrayValue self, int index, int length)
        {
            return new ArrayValue(self.Value.Skip(self.GetIndex(index)).Take(length).ToList());
        }
        #endregion
    }
}