using System;
using System.Linq;

namespace SimpleStackVM
{
    public static partial class StandardArrayLibrary
    {
        #region Fields
        public static readonly Scope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("array.append", vm =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack<ArrayValue>();
                vm.PushStack(Append(left, right));
            });

            result.Define("array.prepend", vm =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack<ArrayValue>();
                vm.PushStack(Prepend(left, right));
            });

            result.Define("array.length", vm =>
            {
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(new NumberValue(top.Value.Count));
            });

            result.Define("array.set", vm =>
            {
                var value = vm.PopStack();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(Set(top, index.IntValue, value));
            });

            result.Define("array.get", vm =>
            {
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(Get(top, index.IntValue));
            });

            result.Define("array.insert", vm =>
            {
                var value = vm.PopStack();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(Insert(top, index.IntValue, value));
            });

            result.Define("array.insertFlatten", vm =>
            {
                var value = vm.PopStack<ArrayValue>();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(InsertFlatten(top, index.IntValue, value));
            });

            result.Define("array.remove", vm =>
            {
                var value = vm.PopStack();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(Remove(top, value));
            });

            result.Define("array.removeAt", vm =>
            {
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(RemoveAt(top, index.IntValue));
            });

            result.Define("array.removeAll", vm =>
            {
                var value = vm.PopStack();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(RemoveAll(top, value));
            });

            result.Define("array.contains", vm =>
            {
                var value = vm.PopStack();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(new BoolValue(Contains(top, value)));
            });

            result.Define("array.indexOf", vm =>
            {
                var value = vm.PopStack();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(new NumberValue(IndexOf(top, value)));
            });

            result.Define("array.sublist", vm =>
            {
                var length = vm.PopStack<NumberValue>();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<ArrayValue>();
                vm.PushStack(SubList(top, index.IntValue, length.IntValue));
            });

            return result;
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
            return self.Sublist(index, length);
        }
        #endregion
    }
}