using System;

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
        #endregion
    }
}