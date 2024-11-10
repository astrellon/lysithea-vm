#nullable enable

using System.Runtime.CompilerServices;

namespace LysitheaVM
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
        public string ToStringSerialise() => this.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, ArgumentsValue args, bool pushToStackTrace)
        {
            vm.SwitchToFunction(this.Value, args, pushToStackTrace);
        }
        #endregion
    }
}