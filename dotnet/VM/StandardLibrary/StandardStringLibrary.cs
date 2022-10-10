using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class StandardStringLibrary
    {
        #region Fields
        public const string HandleName = "string";
        public static readonly IReadOnlyScope Scope = CreateScope();

        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("string.append", vm =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new StringValue(left.ToString() + right.ToString()));
            });

            result.Define("string.prepend", vm =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new StringValue(right.ToString() + left.ToString()));
            });

            result.Define("string.length", vm =>
            {
                var top = vm.PopStack<StringValue>();
                vm.PushStack(new NumberValue(top.Value.Length));
            });

            result.Define("string.set", vm =>
            {
                var value = vm.PopStack();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(Set(top, index.IntValue, value.ToString()));
            });

            result.Define("string.get", vm =>
            {
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(Get(top, index.IntValue));
            });

            result.Define("string.insert", vm =>
            {
                var value = vm.PopStack();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(Insert(top, index.IntValue, value.ToString()));
            });

            result.Define("string.substring", vm =>
            {
                var length = vm.PopStack<NumberValue>();
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(SubString(top, index.IntValue, length.IntValue));
            });

            result.Define("string.removeAt", vm =>
            {
                var index = vm.PopStack<NumberValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(RemoveAt(top, index.IntValue));
            });

            result.Define("string.removeAll", vm =>
            {
                var values = vm.PopStack<StringValue>();
                var top = vm.PopStack<StringValue>();
                vm.PushStack(RemoveAll(top, values.Value));
            });

            return result;
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