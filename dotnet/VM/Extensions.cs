using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
        public static int Count(this IArrayValue self)
        {
            return self.ArrayValues.Count;
        }

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

        public static string GetIndexString(this IArrayValue self, int index)
        {
            return self.GetIndex<StringValue>(index).Value;
        }

        public static bool GetIndexBoolean(this IArrayValue self, int index)
        {
            return self.GetIndex<BoolValue>(index).Value;
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

    public static class IObjectValueExtensions
    {
        #region Methods
        public static IValue Get(this IObjectValue self, string key)
        {
            if (self.TryGetKey(key, out var result))
            {
                return result;
            }
            return NullValue.Value;
        }

        public static bool TryGetKey<T>(this IObjectValue self, string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (self.TryGetKey(key, out var result))
            {
                if (result is T castedValue)
                {
                    value = castedValue;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        public static bool GetBoolean(this IObjectValue self, string key, bool defaultValue)
        {
            if (self.TryGetValue(key, out bool result))
            {
                return result;
            }

            return defaultValue;
        }

        public static int GetInt(this IObjectValue self, string key, int defaultValue)
        {
            if (self.TryGetValue(key, out int result))
            {
                return result;
            }

            return defaultValue;
        }

        public static float GetFloat(this IObjectValue self, string key, float defaultValue)
        {
            if (self.TryGetValue(key, out float result))
            {
                return result;
            }

            return defaultValue;
        }

        public static double GetDouble(this IObjectValue self, string key, double defaultValue)
        {
            if (self.TryGetValue(key, out double result))
            {
                return result;
            }

            return defaultValue;
        }

        public static string GetString(this IObjectValue self, string key, string defaultValue)
        {
            if (self.TryGetValue(key, out string result))
            {
                return result;
            }

            return defaultValue;
        }

        public static bool? GetBoolean(this IObjectValue self, string key)
        {
            if (self.TryGetValue(key, out bool result))
            {
                return result;
            }

            return null;
        }

        public static int? GetInt(this IObjectValue self, string key)
        {
            if (self.TryGetValue(key, out int result))
            {
                return result;
            }

            return null;
        }

        public static float? GetFloat(this IObjectValue self, string key)
        {
            if (self.TryGetValue(key, out float result))
            {
                return result;
            }

            return null;
        }

        public static double? GetDouble(this IObjectValue self, string key)
        {
            if (self.TryGetValue(key, out double result))
            {
                return result;
            }

            return null;
        }

        public static string? GetString(this IObjectValue self, string key)
        {
            if (self.TryGetValue(key, out string result))
            {
                return result;
            }

            return null;
        }

        public static bool TryGetValue(this IObjectValue self, string key, out float result)
        {
            if (self.TryGetKey<NumberValue>(key, out var numValue))
            {
                result = numValue.FloatValue;
                return true;
            }

            result = 0.0f;
            return false;
        }

        public static bool TryGetValue(this IObjectValue self, string key, out double result)
        {
            if (self.TryGetKey<NumberValue>(key, out var numValue))
            {
                result = numValue.Value;
                return true;
            }

            result = 0.0;
            return false;
        }

        public static bool TryGetValue(this IObjectValue self, string key, out int result)
        {
            if (self.TryGetKey<NumberValue>(key, out var numValue))
            {
                result = numValue.IntValue;
                return true;
            }

            result = 0;
            return false;
        }

        public static bool TryGetValue(this IObjectValue self, string key, out bool result)
        {
            if (self.TryGetKey<BoolValue>(key, out var boolValue))
            {
                result = boolValue.Value;
                return true;
            }

            result = false;
            return false;
        }

        public static bool TryGetValue(this IObjectValue self, string key, out string result)
        {
            if (self.TryGetKey<StringValue>(key, out var strValue))
            {
                result = strValue.Value;
                return true;
            }

            result = "";
            return false;
        }
        #endregion
    }

    public static class StringExtensions
    {
        #region Methods
        public static string Repeat(this string self, int count)
        {
            var builder = new StringBuilder(count * self.Length);
            for (var i = 0; i < count; i++)
            {
                builder.Append(self);
            }
            return builder.ToString();
        }
        #endregion
    }
}