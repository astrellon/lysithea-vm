using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace LysitheaVM
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

    public static class IArrayValueExtensions
    {
        public static bool TryGetIndex<T>(this IArrayValue self, int index, [NotNullWhen(true)] out T? result) where T : IValue
        {
            if (self.TryGetIndex(index, out var foundValue))
            {
                if (foundValue is T foundCasted)
                {
                    result = foundCasted;
                    return true;
                }
            }

            result = default(T);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IValue GetIndex(this IArrayValue self, int index)
        {
            if (self.TryGetIndex(index, out var value))
            {
                return value;
            }

            throw new System.IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetIndex<T>(this IArrayValue self, int index) where T : IValue
        {
            if (self.TryGetIndex(index, out var value))
            {
                if (value is T result)
                {
                    return result;
                }
                throw new System.Exception($"Unable to cast argument to: {typeof(T).FullName} for {value.ToString()}");
            }

            throw new System.IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetIndex<T>(this IArrayValue self, int index, VirtualMachine.CastValueDelegate<T> cast) where T : IValue
        {
            if (self.TryGetIndex(index, out var value))
            {
                return cast(value);
            }

            throw new System.IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndexInt(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).IntValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetIndexFloat(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).FloatValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetIndexDouble(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).Value;
        }
    }
}