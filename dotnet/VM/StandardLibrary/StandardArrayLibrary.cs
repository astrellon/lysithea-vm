using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public static partial class StandardArrayLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var arrayFunctions = new Dictionary<string, IValue>
            {
                {"join", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var args = vm.GetArgs(numArgs);
                    vm.PushStack(args);
                })},

                {"length", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(top.Value.Count);
                })},

                {"set", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(Set(top, index.IntValue, value));
                })},

                {"get", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    top.TryGet(index.IntValue, out var value);
                    vm.PushStack(value);
                })},

                {"insert", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(Insert(top, index.IntValue, value));
                })},

                {"insertFlatten", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack<ArrayValue>();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(InsertFlatten(top, index.IntValue, value));
                })},

                {"remove", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(Remove(top, value));
                })},

                {"removeAt", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(RemoveAt(top, index.IntValue));
                })},

                {"removeAll", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(RemoveAll(top, value));
                })},

                {"contains", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(Contains(top, value));
                })},

                {"indexOf", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(IndexOf(top, value));
                })},

                {"sublist", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var length = vm.PopStack<NumberValue>();
                    var index = vm.PopStack<NumberValue>();
                    var top = vm.PopStack<ArrayValue>();
                    vm.PushStack(SubList(top, index.IntValue, length.IntValue));
                })}
            };

            result.Define("array", new ObjectValue(arrayFunctions));

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
                if (input.CompareTo(self.Value[i]) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static ArrayValue SubList(ArrayValue self, int index, int length)
        {
            index = self.GetIndex(index);
            if (length < 0)
            {
                length = self.Value.Count - index;
            }
            else
            {
                var diff = (index + length) - self.Value.Count;
                if (diff > 0)
                {
                    length -= diff;
                }
            }

            var result = new IValue[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = self.Value[i + index];
            }
            return new ArrayValue(result);
        }

        public static int GeneralCompareTo(IArrayValue left, IValue? rightInput)
        {
            if (left == rightInput)
            {
                return 0;
            }

            if (left == null || !(rightInput is IArrayValue right))
            {
                return 1;
            }

            var compareLength = left.ArrayValues.Count.CompareTo(right.ArrayValues.Count);
            if (compareLength != 0)
            {
                return compareLength;
            }

            for (var i = 0; i < left.ArrayValues.Count; i++)
            {
                var compare = left.ArrayValues[i].CompareTo(right.ArrayValues[i]);
                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }

        public static string GeneralToString(IArrayValue input)
        {
            var result = new StringBuilder();
            result.Append('(');
            var first = true;
            foreach (var value in input.ArrayValues)
            {
                if (!first)
                {
                    result.Append(' ');
                }
                first = false;

                result.Append(value.ToString());
            }

            result.Append(')');
            return result.ToString();

        }
        #endregion
    }
}