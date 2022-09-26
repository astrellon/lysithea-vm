using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

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
                        vm.PushStack(Set(obj, key, value));
                        break;
                    }
                case "get":
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
                        break;
                    }
                case "keys":
                    {
                        var top = vm.PopStack<ObjectValue>();
                        vm.PushStack(Keys(top));
                        break;
                    }
                case "values":
                    {
                        var top = vm.PopStack<ObjectValue>();
                        vm.PushStack(Values(top));
                        break;
                    }
                case "length":
                    {
                        var top = vm.PopStack<ObjectValue>();
                        vm.PushStack(new NumberValue(top.Value.Count));
                        break;
                    }
            }
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