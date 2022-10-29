#nullable enable

using System.Runtime.CompilerServices;

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
        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is FunctionValue otherProcedure))
            {
                return 1;
            }

            if (this.Value == otherProcedure.Value)
            {
                return 0;
            }

            return 1;
        }

        public override string ToString() => $"function:{this.Value.Name}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, ArrayValue args, bool pushToStackTrace)
        {
            vm.ExecuteFunction(this.Value, args, pushToStackTrace);
        }
        #endregion
    }
}