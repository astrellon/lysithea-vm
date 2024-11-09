#nullable enable

using System.Runtime.CompilerServices;

namespace LysitheaVM
{
    public struct BuiltinFunctionValue : IFunctionValue
    {
        #region Field
        public delegate void BuiltinFunctionDelegate(VirtualMachine vm, ArgumentsValue args);

        public readonly BuiltinFunctionDelegate Value;
        public readonly string Name;
        public readonly bool HasReturn;

        public string TypeName => "builtin-function";
        #endregion

        #region Constructor
        public BuiltinFunctionValue(BuiltinFunctionDelegate value, string name, bool hasReturn = true)
        {
            this.Name = name;
            this.Value = value;
            this.HasReturn = hasReturn;
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

        public override string ToString() => $"builtin-function:{this.Name}";
        public string ToStringSerialise() => this.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, ArgumentsValue args, bool pushToStackTrace)
        {
            try
            {
                this.Value.Invoke(vm, args);
            }
            catch (System.Exception exp)
            {
                throw new VirtualMachineException(vm, vm.CreateStackTrace(), exp.Message);
            }
        }
        #endregion
    }
}