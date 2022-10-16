using System;

#nullable enable

namespace SimpleStackVM
{
    public struct FunctionValue : IFunctionValue
    {
        #region Field
        public readonly Function Value;

        public string TypeName => "function";
        #endregion

        #region Constructor
        public FunctionValue(Function value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is FunctionValue otherProc)
            {
                return this.Value == otherProc.Value;
            }

            return false;
        }

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is FunctionValue otherProcedure))
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
            return $"function:{this.Value.Name}";
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #endregion
    }
}