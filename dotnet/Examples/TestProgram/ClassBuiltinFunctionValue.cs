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

        public string TypeName => "class-builtin-function";
        #endregion

        #region Constructor
        public ClassBuiltinFunctionValue(object self, MethodInfo method)
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
            return 1;
        }

        public void Invoke(VirtualMachine vm, int numArgs, bool pushToStackTrace)
        {
            var args = new object[] { vm, numArgs };
            this.method.Invoke(this.self, args);
        }
        #endregion
    }
}