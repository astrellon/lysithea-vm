using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM.Extensions
{
    public static class StandardObjectLibraryExtensions
    {
        #region Methods
        public static bool TryGetValue(this ObjectValue self, string key, [NotNullWhen(true)] out IValue? value)
        {
            return self.Value.TryGetValue(key, out value);
        }

        public static bool TryGetValue<T>(this ObjectValue self, string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (!self.TryGetValue(key, out var result))
            {
                value = default(T);
                return false;
            }

            if (result is T castedValue)
            {
                value = castedValue;
                return true;
            }

            value = default(T);
            return false;
        }

        public static ObjectValue Set(this ObjectValue self, string key, IValue value)
        {
            var result = self.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            result[key] = value;
            return new ObjectValue(result);
        }
        #endregion
    }
}