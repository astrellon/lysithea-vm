using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    using BuiltinFunctionHandler = Action<VirtualMachine>;

    public class BuiltinFunctionValue : IFunctionValue
    {
        private static readonly BuiltinFunctionHandler EmptyHandler = (vm) => { };

        #region Field
        bool IValue.IsNull => false;
        object IValue.RawValue => this.Value;

        public readonly BuiltinFunctionHandler Value;
        #endregion

        #region Constructor
        public BuiltinFunctionValue(BuiltinFunctionHandler value)
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

        public static implicit operator BuiltinFunctionValue(BuiltinFunctionHandler handler)
        {
            return new BuiltinFunctionValue(handler);
        }
        #endregion
    }
}