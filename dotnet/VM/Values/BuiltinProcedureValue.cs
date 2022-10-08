using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    using BuiltinCommandHandler = Action<VirtualMachine>;

    public class BuiltinProcedureValue : IValue
    {
        private static readonly BuiltinCommandHandler EmptyHandler = (vm) => { };

        #region Field
        bool IValue.IsNull => false;
        object IValue.RawValue => this.Value;

        public readonly BuiltinCommandHandler Value;
        #endregion

        #region Constructor
        public BuiltinProcedureValue(BuiltinCommandHandler value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is BuiltinProcedureValue otherProc)
            {
                return this.Value == otherProc.Value;
            }

            return false;
        }

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is BuiltinProcedureValue otherProcedure))
            {
                return -1;
            }

            if (this.Value == otherProcedure.Value)
            {
                return 0;
            }

            return 1;
        }

        public override string ToString()
        {
            return "builtin-proc";
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static implicit operator BuiltinProcedureValue(BuiltinCommandHandler handler)
        {
            return new BuiltinProcedureValue(handler);
        }
        #endregion
    }
}