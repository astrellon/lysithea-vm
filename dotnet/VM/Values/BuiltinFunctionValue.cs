#nullable enable

using System.Runtime.CompilerServices;

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

        public override string ToString() => "builtin-function";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, int numArgs, bool pushToStackTrace)
        {
            this.Value.Invoke(vm, numArgs);
        }
        #endregion
    }
}