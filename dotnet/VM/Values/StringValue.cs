#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public struct StringValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "length" };
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

        public override string ToString() => this.Value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out IValue? value)
        {
            if (key == "length")
            {
                value = new BuiltinFunctionValue(this.GetLength);
                return true;
            }

            value = null;
            return false;
        }

        public void GetLength(VirtualMachine vm, int numArgs)
        {
            vm.PushStack(this.Value.Length);
        }
        #endregion
    }
}