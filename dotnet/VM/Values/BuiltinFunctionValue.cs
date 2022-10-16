using System;

#nullable enable

namespace SimpleStackVM
{
    public struct BuiltinFunctionValue : IFunctionValue
    {
        #region Field
        public delegate void BuiltinFunctionDelegate(VirtualMachine vm, int numArgs);

        public readonly BuiltinFunctionDelegate Value;

        public string TypeName => "builtin-function";
        #endregion

        #region Constructor
        public BuiltinFunctionValue(BuiltinFunctionDelegate value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is BuiltinFunctionValue otherProc)
            {
                return this.Value == otherProc.Value;
            }

            return false;
        }

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is BuiltinFunctionValue otherFunction))
            {
                return -1;
            }

            if (this.Value == otherFunction.Value)
            {
                return 0;
            }

            return 1;
        }

        public override string ToString()
        {
            return "builtin-function";
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static implicit operator BuiltinFunctionValue(BuiltinFunctionDelegate handler)
        {
            return new BuiltinFunctionValue(handler);
        }
        #endregion
    }
}