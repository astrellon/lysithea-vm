#nullable enable

using System.Runtime.CompilerServices;

namespace SimpleStackVM
{
    public struct BuiltinFunctionValue : IFunctionValue
    {
        #region Field
        public delegate void BuiltinFunctionDelegate(VirtualMachine vm, ArrayValue args);

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
                return 1;
            }

            return this.Value == otherFunction.Value ? 0 : 1;
        }

        public override string ToString() => "builtin-function";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, ArrayValue args, bool pushToStackTrace)
        {
            this.Value.Invoke(vm, args);
        }
        #endregion
    }
}