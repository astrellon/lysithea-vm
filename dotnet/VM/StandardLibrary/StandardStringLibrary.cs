using System.Linq;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class StandardStringLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();

        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var stringFunctions = new Dictionary<string, IValue>
            {
                {"length", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    vm.PushStack(new NumberValue(top.Value.Length));
                }, "string.length")},

                {"set", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var value = args.GetIndex(2);
                    vm.PushStack(Set(top, index.IntValue, value.ToString()));
                }, "string.set")},

                {"get", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    vm.PushStack(Get(top, index.IntValue));
                }, "string.get")},

                {"insert", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var value = args.GetIndex(2);
                    vm.PushStack(Insert(top, index.IntValue, value.ToString()));
                }, "string.insert")},

                {"substring", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var length = args.GetIndex<NumberValue>(2);
                    vm.PushStack(SubString(top, index.IntValue, length.IntValue));
                }, "string.substring")},

                {"removeAt", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    vm.PushStack(RemoveAt(top, index.IntValue));
                }, "string.removeAt")},

                {"removeAll", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<StringValue>(0);
                    var values = args.GetIndex<StringValue>(1);
                    vm.PushStack(RemoveAll(top, values.Value));
                }, "string.removeAll")},

                {"join", new BuiltinFunctionValue((vm, args) =>
                {
                    var separator = args.Value.First().ToString();
                    var result = string.Join(separator, args.Value.Skip(1));
                    vm.PushStack(result);
                }, "string.join")}
            };

            result.TryConstant("string", new ObjectValue(stringFunctions));

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