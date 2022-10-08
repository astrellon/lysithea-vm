using System;
using System.Diagnostics.CodeAnalysis;
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
            result.Define("object.set", new BuiltinProcedureValue(vm =>
            {
                var value = vm.PopStack();
                var key = vm.PopStack<StringValue>();
                var obj = vm.PopStack<ObjectValue>();
                vm.PushStack(Set(obj, key, value));
            }));

            result.Define("object.get", new BuiltinProcedureValue(vm =>
            {
                var key = vm.PopStack<StringValue>();
                var obj = vm.PopStack<ObjectValue>();
                if (TryGetValue(obj, key, out var value))
                {
                    vm.PushStack(value);
                }
                else
                {
                    vm.PushStack(NullValue.Value);
                }
            }));

            result.Define("object.keys", new BuiltinProcedureValue(vm =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(Keys(top));
            }));

            result.Define("object.values", new BuiltinProcedureValue(vm =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(Values(top));
            }));

            result.Define("object.length", new BuiltinProcedureValue(vm =>
            {
                var top = vm.PopStack<ObjectValue>();
                vm.PushStack(new NumberValue(top.Value.Count));
            }));

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

        public static bool TryGetValue(ObjectValue self, string key, [NotNullWhen(true)] out IValue? value)
        {
            return self.Value.TryGetValue(key, out value);
        }

        public static bool TryGetValue<T>(ObjectValue self, string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (!TryGetValue(self, key, out var result))
            {
                value = default(T);
                return false;
            }

            if (result is T castedValue)
            {
                value = castedValue;
                return true;
            }

            value = default(T);
            return false;
        }

        public static ObjectValue Set(ObjectValue self, string key, IValue value)
        {
            var result = self.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            result[key] = value;
            return new ObjectValue(result);
        }
        #endregion
    }
}