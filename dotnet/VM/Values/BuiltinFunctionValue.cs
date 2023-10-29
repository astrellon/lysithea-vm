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

        public string TypeName => "builtin-function";
        #endregion

        #region Constructor
        public BuiltinFunctionValue(BuiltinFunctionDelegate value, string name)
        {
            this.Name = name;
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

        public override string ToString() => $"builtin-function:{this.Name}";
        public string ToStringFormatted(int indent, int depth) => this.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(VirtualMachine vm, ArgumentsValue args, bool pushToStackTrace)
        {
            this.Value.Invoke(vm, args);
        }
        #endregion
    }
}