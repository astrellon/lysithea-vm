using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LysitheaVM
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

            var objectFunctions = new Dictionary<string, IValue>
            {
                {"join", new BuiltinFunctionValue((vm, args) =>
                {
                    vm.PushStack(Join(args));
                }, "object.join")},

                {"set", new BuiltinFunctionValue((vm, args) =>
                {
                    var obj = args.GetIndex<ObjectValue>(0);
                    var key = args.GetIndex<StringValue>(1);
                    var value = args.GetIndex(2);
                    vm.PushStack(Set(obj, key.Value, value));
                }, "object.set")},

                {"get", new BuiltinFunctionValue((vm, args) =>
                {
                    var obj = args.GetIndex<ObjectValue>(0);
                    var key = args.GetIndex<StringValue>(1);
                    if (obj.TryGetKey(key.Value, out var value))
                    {
                        vm.PushStack(value);
                    }
                    else
                    {
                        vm.PushStack(NullValue.Value);
                    }
                }, "object.get")},

                {"removeKey", new BuiltinFunctionValue((vm, args) =>
                {
                    var obj = args.GetIndex<ObjectValue>(0);
                    var key = args.GetIndex<StringValue>(1);
                    vm.PushStack(RemoveKey(obj, key.Value));
                }, "object.removeKey")},

                {"removeValues", new BuiltinFunctionValue((vm, args) =>
                {
                    var obj = args.GetIndex<ObjectValue>(0);
                    var values = args.GetIndex(1);
                    vm.PushStack(RemoveValues(obj, values));
                }, "object.removeValues")},

                {"keys", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ObjectValue>(0);
                    vm.PushStack(Keys(top));
                }, "object.keys")},

                {"values", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ObjectValue>(0);
                    vm.PushStack(Values(top));
                }, "object.values")},

                {"length", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ObjectValue>(0);
                    vm.PushStack(top.Value.Count);
                }, "object.length")}
            };

            result.TrySetConstant("object", new ObjectValue(objectFunctions));

            return result;
        }

        public static ObjectValue Join(ArgumentsValue args)
        {
            var map = new Dictionary<string, IValue>();
            var argValues = args.ArrayValues;

            for (var i = 0; i < argValues.Count; i++)
            {
                var arg = argValues[i];
                if (arg is StringValue argStr)
                {
                    var key = argStr.Value;
                    var value = argValues[++i];
                    map[key] = value;
                }
                else if (arg is IObjectValue argObj)
                {
                    foreach (var key in argObj.ObjectKeys)
                    {
                        if (argObj.TryGetKey(key, out var value))
                        {
                            map[key] = value;
                        }
                    }
                }
                else
                {
                    var key = arg.ToString();
                    var value = argValues[++i];
                    map[key] = value;
                }
            }

            return new ObjectValue(map);
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
            var result = new Dictionary<string, IValue>(self.Value);
            result[key] = value;
            return new ObjectValue(result);
        }

        public static ObjectValue RemoveKey(ObjectValue self, string key)
        {
            if (!self.Value.ContainsKey(key))
            {
                return self;
            }

            var result = new Dictionary<string, IValue>(self.Value.Where(kvp => kvp.Key != key));
            return new ObjectValue(result);
        }

        public static ObjectValue RemoveValues(ObjectValue self, IValue values)
        {
            var result = new Dictionary<string, IValue>(self.Value.Where(kvp => kvp.Value.CompareTo(values) != 0));
            return new ObjectValue(result);
        }

        public static int GeneralCompareTo(IObjectValue left, IValue? rightInput)
        {
            if (left == rightInput)
            {
                return 0;
            }

            if (left == null || !(rightInput is IObjectValue right))
            {
                return 1;
            }

            var leftKeys = left.ObjectKeys;
            var rightKeys = right.ObjectKeys;
            var compareLength = leftKeys.Count.CompareTo(rightKeys.Count);
            if (compareLength != 0)
            {
                return compareLength;
            }

            foreach (var key in leftKeys)
            {
                if (!left.TryGetKey(key, out var leftValue))
                {
                    throw new Exception($"Object key: {key} not present in the object!: {left.ToString()}");
                }

                if (right.TryGetKey(key, out var rightValue))
                {
                    var compare = leftValue.CompareTo(rightValue);
                    if (compare != 0)
                    {
                        return compare;
                    }
                }
                else
                {
                    return 1;
                }
            }

            return 0;
        }
        #endregion
    }
}