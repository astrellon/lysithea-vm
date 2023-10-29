using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LysitheaVM
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
                {"join", new BuiltinFunctionValue((vm, args) =>
                {
                    vm.PushStack(new ArrayValue(args.Value));
                }, "array.join")},

                {"length", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<IArrayValue>(0);
                    vm.PushStack(top.ArrayValues.Count);
                }, "array.length")},

                {"set", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var value = args.GetIndex(2);
                    vm.PushStack(Set(top, index.IntValue, value));
                }, "array.set")},

                {"get", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<IArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    if (top.TryGetIndex(index.IntValue, out var value))
                    {
                        vm.PushStack(value);
                    }
                    else
                    {
                        vm.PushStack(NullValue.Value);
                    }
                }, "array.get")},

                {"insert", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var value = args.GetIndex(2);
                    vm.PushStack(Insert(top, index.IntValue, value));
                }, "array.insert")},

                {"insertFlatten", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var value = args.GetIndex<IArrayValue>(2);
                    vm.PushStack(InsertFlatten(top, index.IntValue, value));
                }, "array.insertFlatten")},

                {"remove", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var value = args.GetIndex(1);
                    vm.PushStack(Remove(top, value));
                }, "array.remove")},

                {"removeAt", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    vm.PushStack(RemoveAt(top, index.IntValue));
                }, "array.removeAt")},

                {"removeAll", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var value = args.GetIndex(1);
                    vm.PushStack(RemoveAll(top, value));
                }, "array.removeAll")},

                {"contains", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<IArrayValue>(0);
                    var value = args.GetIndex(1);
                    vm.PushStack(Contains(top, value));
                }, "array.contains")},

                {"indexOf", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<IArrayValue>(0);
                    var value = args.GetIndex(1);
                    vm.PushStack(IndexOf(top, value));
                }, "array.indexOf")},

                {"sublist", new BuiltinFunctionValue((vm, args) =>
                {
                    var top = args.GetIndex<ArrayValue>(0);
                    var index = args.GetIndex<NumberValue>(1);
                    var length = args.GetIndex<NumberValue>(2);
                    vm.PushStack(SubList(top, index.IntValue, length.IntValue));
                }, "array.sublist")}
            };

            result.TryDefine("array", new ObjectValue(arrayFunctions));

            return result;
        }

        public static ArrayValue Set(ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue[self.CalcIndex(index)] = input;
            return new ArrayValue(newValue);
        }

        public static ArrayValue Insert(ArrayValue self, int index, IValue input)
        {
            var newValue = self.Value.ToList();
            newValue.Insert(self.CalcIndex(index), input);
            return new ArrayValue(newValue);
        }

        public static ArrayValue InsertFlatten(ArrayValue self, int index, IArrayValue input)
        {
            var newValue = self.Value.ToList();
            newValue.InsertRange(self.CalcIndex(index), input.ArrayValues);
            return new ArrayValue(newValue);
        }

        public static ArrayValue RemoveAt(ArrayValue self, int index)
        {
            var newValue = self.Value.ToList();
            newValue.RemoveAt(self.CalcIndex(index));
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

        public static bool Contains(IArrayValue self, IValue input)
        {
            return self.ArrayValues.Contains(input);
        }

        public static int IndexOf(IArrayValue self, IValue input)
        {
            var list = self.ArrayValues;
            for (var i = 0; i < list.Count; i++)
            {
                if (input.CompareTo(list[i]) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static ArrayValue SubList(ArrayValue self, int index, int length)
        {
            index = self.CalcIndex(index);
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

            if (index == 0 && length >= self.Value.Count)
            {
                return self;
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

            if (left == null || !(rightInput is IArrayValue right) || left.GetType() != rightInput.GetType())
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

        private static bool IsAllPrimitive(IArrayValue input)
        {
            foreach (var item in input.ArrayValues)
            {
                if (!(item is BoolValue ||
                    item is NumberValue ||
                    item is StringValue))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GeneralToStringSingleLine(IArrayValue input, int indent, int depth)
        {
            var result = new StringBuilder();
            result.Append('[');
            var first = true;
            foreach (var value in input.ArrayValues)
            {
                if (!first)
                {
                    result.Append(' ');
                }
                result.Append(value.ToStringFormatted(0, 0));
                first = false;
            }
            result.Append(']');
            return result.ToString();
        }

        public static string GeneralToString(IArrayValue input, int indent, int depth)
        {
            if (input.Count() < 32 && IsAllPrimitive(input))
            {
                return GeneralToStringSingleLine(input, indent, depth + 1);
            }

            var result = new StringBuilder();

            var indent1 = "";
            var indent2 = "";

            if (indent >= 0)
            {
                indent1 = " ".Repeat(indent * depth);
                indent2 = indent1 + " ".Repeat(indent);
            }

            result.Append('[');
            var first = true;
            foreach (var value in input.ArrayValues)
            {
                if (indent >= 0)
                {
                    result.Append('\n');
                    result.Append(indent2);
                }
                else if (!first)
                {
                    result.Append(' ');
                }
                first = false;

                result.Append(value.ToStringFormatted(indent, depth + 1));
            }

            if (!first && indent >= 0)
            {
                result.Append('\n');
                result.Append(indent1);
            }
            result.Append(']');
            return result.ToString();

        }
        #endregion
    }
}