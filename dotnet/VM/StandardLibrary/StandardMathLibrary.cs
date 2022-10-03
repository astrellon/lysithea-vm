using System;
using System.Linq;

namespace SimpleStackVM
{
    public class StandardMathLibrary : BuiltinProcedureCollection
    {
        #region Fields
        public override string NameSpace => "math";
        public static readonly StandardMathLibrary Instance = new StandardMathLibrary();
        #endregion

        // #region Constructor
        // public StandardMathLibrary() : base()
        // {

        // }
        // #endregion

        #region Methods
        [BuiltinProcedure("add")]
        [BuiltinProcedure("+")]
        public static void Add(VirtualMachine vm, NumberValue num1, NumberValue num2)
        {
            vm.PushStack((NumberValue)(num1.Value + num2.Value));
        }
        [BuiltinProcedure("sub")]
        [BuiltinProcedure("-")]
        public static void Sub(VirtualMachine vm, NumberValue num1, NumberValue num2)
        {
            vm.PushStack((NumberValue)(num1.Value - num2.Value));
        }
        #endregion
    }
}