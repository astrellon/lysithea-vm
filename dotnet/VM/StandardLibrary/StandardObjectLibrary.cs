using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public static class StandardObjectLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var objectFunctions = new Dictionary<string, IValue>();

            objectFunctions["set"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var value = vm.PopStack();
                var key = vm.PopStack<StringValue>();
                var obj = vm.PopStack<ObjectValue>();
                vm.PushStack(Set(obj, key, value));
            });

            objectFunctions["get"] = new BuiltinFunctionValue((vm, numArgs) =>
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
            });

            objectFunctions["removeKey"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var key = vm.PopStack<StringValue>();
                var obj = vm.PopStack<ObjectValue>();
                vm.PushStack(RemoveKey(obj, key));
            });

            objectFunctions["removeValues"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var values = vm.PopStack();
                var obj = vm.PopStack<ObjectValue>();
                vm.PushStack(RemoveValues(obj, values));
            });

            objectFunctions["keys"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(Keys(top));
            });

            objectFunctions["values"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(Values(top));
            });

            objectFunctions["length"] = new BuiltinFunctionValue((vm, numArgs) =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(new NumberValue(top.Value.Count));
            });

            result.Define("object", new ObjectValue(objectFunctions));

            return result;
        }

        public static ArrayValue Keys(ObjectValue self)
        {
            var keys = self.Value.Keys.Select(k => new StringValue(k) as IValue).ToList();
            return new ArrayValue(keys);
        }

        public static ArrayValue Values(ObjectValue self)
        {
            return new ArrayValue(self.Value.Values.ToList());
        }

        public static ObjectValue Set(ObjectValue self, string key, IValue value)
        {
            var result = self.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            result[key] = value;
            return new ObjectValue(result);
        }

        public static ObjectValue RemoveKey(ObjectValue self, string key)
        {
            if (!self.Value.ContainsKey(key))
            {
                return self;
            }

            var result = self.Value.Where(kvp => kvp.Key != key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new ObjectValue(result);
        }

        public static ObjectValue RemoveValues(ObjectValue self, IValue values)
        {
            var result = self.Value.Where(kvp => kvp.Value.CompareTo(values) != 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return new ObjectValue(result);
        }
        #endregion
    }
}