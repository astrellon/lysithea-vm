using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace LysitheaVM
{
    public static class VirtualMachineExtensions
    {
        #region Methods
        public static void PushStack(this VirtualMachine self, bool value)
        {
            self.PushStack(new BoolValue(value));
        }
        public static void PushStack(this VirtualMachine self, int value)
        {
            self.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine self, float value)
        {
            self.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine self, double value)
        {
            self.PushStack(new NumberValue(value));
        }
        public static void PushStack(this VirtualMachine self, string value)
        {
            self.PushStack(new StringValue(value));
        }

        public static T PopStack<T>(this VirtualMachine self) where T : IValue
        {
            var obj = self.PopStack();
            if (obj.GetType() == typeof(T))
            {
                return (T)obj;
            }

            throw new VirtualMachineException(self, self.CreateStackTrace(), $"Unable to pop stack, type cast error: wanted {typeof(T).FullName} and got {obj.GetType().FullName}");
        }

        public static bool PopStackBool(this VirtualMachine self)
        {
            return self.PopStack<BoolValue>().Value;
        }

        public static double PopStackDouble(this VirtualMachine self)
        {
            return self.PopStack<NumberValue>().Value;
        }

        public static double GetNumArg(this VirtualMachine self, CodeLine input)
        {
            if (input.Input == null)
            {
                return self.PopStackDouble();
            }
            if (input.Input is NumberValue numValue)
            {
                return numValue.Value;
            }
            throw new VirtualMachineException(self, self.CreateStackTrace(), "Error attempting to get number argument");
        }

        public static bool GetBoolArg(this VirtualMachine self, CodeLine input)
        {
            if (input.Input == null)
            {
                return self.PopStackBool();
            }
            if (input.Input is BoolValue boolValue)
            {
                return boolValue.Value;
            }
            throw new VirtualMachineException(self, self.CreateStackTrace(), "Error attempting to get boolean argument");
        }
        #endregion
    }

    public static class ListExtensions
    {
        public static T PopBack<T>(this IList<T> self)
        {
            var result = self.Last();
            self.RemoveAt(self.Count - 1);
            return result;
        }
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

        public static IValue GetIndex(this IArrayValue self, int index)
        {
            if (self.TryGetIndex(index, out var value))
            {
                return value;
            }

            throw new System.IndexOutOfRangeException();
        }

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

        public static T GetIndex<T>(this IArrayValue self, int index, VirtualMachine.CastValueDelegate<T> cast) where T : IValue
        {
            if (self.TryGetIndex(index, out var value))
            {
                return cast(value);
            }

            throw new System.IndexOutOfRangeException();
        }

        public static int GetIndexInt(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).IntValue;
        }

        public static float GetIndexFloat(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).FloatValue;
        }

        public static double GetIndexDouble(this IArrayValue self, int index)
        {
            return self.GetIndex<NumberValue>(index).Value;
        }
    }
}