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

    public static class ScopeExtensions
    {
        #region Methods
        public static void Define(this Scope vm, string key, bool value)
        {
            vm.Define(key, new BoolValue(value));
        }
        public static void Define(this Scope vm, string key, int value)
        {
            vm.Define(key, new NumberValue(value));
        }
        public static void Define(this Scope vm, string key, float value)
        {
            vm.Define(key, new NumberValue(value));
        }
        public static void Define(this Scope vm, string key, double value)
        {
            vm.Define(key, new NumberValue(value));
        }
        public static void Define(this Scope vm, string key, string value)
        {
            vm.Define(key, new StringValue(value));
        }
        public static void Define(this Scope vm, string key, IReadOnlyList<IValue> value)
        {
            vm.Define(key, new ArrayValue(value));
        }
        public static void Define(this Scope vm, string key, IReadOnlyDictionary<string, IValue> value)
        {
            vm.Define(key, new ObjectValue(value));
        }
        public static void Define(this Scope vm, string key, BuiltinFunctionValue.BuiltinFunctionDelegate value)
        {
            vm.Define(key, new BuiltinFunctionValue(value));
        }
        #endregion
    }
}