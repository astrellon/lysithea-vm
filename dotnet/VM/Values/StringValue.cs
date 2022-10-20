#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public struct StringValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "get", "set", "length", "insert", "substring", "removeAt", "removeAll" };
        public IReadOnlyList<string> ObjectKeys => Keys;
        public string TypeName => "string";

        public readonly string Value;
        #endregion

        #region Constructor
        public StringValue(string value)
        {
            this.Value = string.Intern(value);
        }
        #endregion

        #region Methods
        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is StringValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Length + index;
            }

            return index;
        }

        private ClassBuiltinFunctionValue<StringValue> CreateFunc(ClassBuiltinFunctionValue<StringValue>.ClassBuiltinFunctionInvoke<StringValue> method)
        {
            return new ClassBuiltinFunctionValue<StringValue>(this, method);
        }

        public override string ToString() => this.Value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out IValue? value)
        {
            switch (key)
            {
                case "get":
                {
                    value = CreateFunc(Get);
                    return true;
                }
                case "set":
                {
                    value = CreateFunc(Set);
                    return true;
                }
                case "length":
                {
                    value = CreateFunc(GetLength);
                    return true;
                }
                case "insert":
                {
                    value = CreateFunc(Insert);
                    return true;
                }
                case "substring":
                {
                    value = CreateFunc(SubString);
                    return true;
                }
                case "removeAt":
                {
                    value = CreateFunc(RemoveAt);
                    return true;
                }
                case "removeAll":
                {
                    value = CreateFunc(RemoveAll);
                    return true;
                }
            }

            value = null;
            return false;
        }

        public static void Get(StringValue self, VirtualMachine vm, int numArgs)
        {
            var index = vm.PopStack<NumberValue>();
            vm.PushStack(StandardStringLibrary.Get(self, index.IntValue));
        }

        public static void Set(StringValue self, VirtualMachine vm, int numArgs)
        {
            var value = vm.PopStack();
            var index = vm.PopStack<NumberValue>();
            vm.PushStack(StandardStringLibrary.Set(self, index.IntValue, value.ToString()));
        }

        public static void GetLength(StringValue self, VirtualMachine vm, int numArgs)
        {
            vm.PushStack(self.Value.Length);
        }

        public static void Insert(StringValue self, VirtualMachine vm, int numArgs)
        {
            var value = vm.PopStack();
            var index = vm.PopStack<NumberValue>();
            vm.PushStack(StandardStringLibrary.Insert(self, index.IntValue, value.ToString()));
        }

        public static void SubString(StringValue self, VirtualMachine vm, int numArgs)
        {
            var length = vm.PopStack<NumberValue>();
            var index = vm.PopStack<NumberValue>();
            vm.PushStack(StandardStringLibrary.SubString(self, index.IntValue, length.IntValue));
        }

        public static void RemoveAt(StringValue self, VirtualMachine vm, int numArgs)
        {
            var index = vm.PopStack<NumberValue>();
            vm.PushStack(StandardStringLibrary.RemoveAt(self, index.IntValue));
        }

        public static void RemoveAll(StringValue self, VirtualMachine vm, int numArgs)
        {
            var values = vm.PopStack<StringValue>();
            vm.PushStack(StandardStringLibrary.RemoveAll(self, values.Value));
        }
        #endregion
    }
}