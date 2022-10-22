using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public static class VirtualMachineExtensions
    {
        #region Methods
        public static void PushStack(this VirtualMachine vm, bool value)
        {
            vm.PushStack(new BoolValue(value));
        }
        public static void PushStack(this VirtualMachine vm, int value)
        {
            vm.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine vm, float value)
        {
            vm.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine vm, double value)
        {
            vm.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine vm, string value)
        {
            vm.PushStack(new StringValue(value));
        }
        public static void PushStack(this VirtualMachine vm, IReadOnlyList<IValue> value)
        {
            vm.PushStack(new ArrayValue(value));
        }
        public static void PushStack(this VirtualMachine vm, IReadOnlyDictionary<string, IValue> value)
        {
            vm.PushStack(new ObjectValue(value));
        }
        public static void PushStack(this VirtualMachine vm, BuiltinFunctionValue.BuiltinFunctionDelegate value)
        {
            vm.PushStack(new BuiltinFunctionValue(value));
        }
        #endregion
    }
}