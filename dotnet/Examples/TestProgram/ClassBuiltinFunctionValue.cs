using System;
using System.Linq;
using System.Reflection;

namespace SimpleStackVM.Example
{
    public struct ClassBuiltinFunctionValue : IFunctionValue
    {
        #region Fields
        private readonly object self;
        private readonly MethodInfo method;
        #endregion

        #region Constructor
        public ClassBuiltinFunctionValue(object self, MethodInfo method)
        {
            this.self = self;
            this.method = method;
        }

        public string TypeName => "class-builtin-function";

        public int CompareTo(IValue other)
        {
            return 1;
        }

        public void Invoke(VirtualMachine vm, int numArgs, bool pushToStackTrace)
        {
            var args = new object[] { vm.GetArgs(numArgs) };
            this.method.Invoke(this.self, args);
        }
        #endregion

        #region Methods
        #endregion
    }
}