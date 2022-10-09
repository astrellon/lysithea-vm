using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public class ProcedureValue : IProcedureValue
    {
        #region Field
        bool IValue.IsNull => false;
        object IValue.RawValue => this.Value;

        public readonly Procedure Value;
        #endregion

        #region Constructor
        public ProcedureValue(Procedure value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is ProcedureValue otherProc)
            {
                return this.Value == otherProc.Value;
            }

            return false;
        }

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is ProcedureValue otherProcedure))
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
            return $"proc:{this.Value.Name}";
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #endregion
    }
}