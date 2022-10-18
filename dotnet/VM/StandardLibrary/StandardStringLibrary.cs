using System.Linq;
using System.Collections.Generic;

namespace SimpleStackVM
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
                {"length", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(new NumberValue(top.Value.Length));
                })},

                {"set", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(Set(top, index.IntValue, value.ToString()));
                })},

                {"get", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(Get(top, index.IntValue));
                })},

                {"insert", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(Insert(top, index.IntValue, value.ToString()));
                })},

                {"substring", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var length = vm.PopStack<NumberValue>();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(SubString(top, index.IntValue, length.IntValue));
                })},

                {"removeAt", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(RemoveAt(top, index.IntValue));
                })},

                {"removeAll", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var values = vm.PopStack<StringValue>();
                    var top = vm.PopStack<StringValue>();
                    vm.PushStack(RemoveAll(top, values.Value));
                })},

                {"join", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var args = vm.GetArgs(numArgs);
                    var separator = args.Value.First().ToString();
                    var result = string.Join(separator, args.Value.Skip(1));
                    vm.PushStack(result);
                })}
            };

            result.Define("string", new ObjectValue(stringFunctions));

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