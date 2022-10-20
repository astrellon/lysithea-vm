using System;
using System.Linq;
using System.Reflection;

namespace SimpleStackVM
{
    public struct ClassBuiltinFunctionValue<T> : IFunctionValue
    {
        public delegate void ClassBuiltinFunctionInvoke<TInvoke>(TInvoke self, VirtualMachine vm, int numArgs);

        #region Fields
        private readonly T self;
        private readonly ClassBuiltinFunctionInvoke<T> method;

        public string TypeName => "class-builtin-function";
        #endregion

        #region Constructor
        public ClassBuiltinFunctionValue(T self, ClassBuiltinFunctionInvoke<T> method)
        {
            this.self = self;
            this.method = method;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "class-builtin-function:" + this.method.ToString();
        }
        public int CompareTo(IValue other)
        {
            if (other == null || !(other is ClassBuiltinFunctionValue<T> otherFunction))
            {
                return -1;
            }

            return this.method == otherFunction.method ? 0 : 1;
        }

        public void Invoke(VirtualMachine vm, int numArgs, bool pushToStackTrace)
        {
            this.method.Invoke(this.self, vm, numArgs);
        }
        #endregion
    }
}