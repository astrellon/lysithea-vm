using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimpleStackVM
{
    public static class ListExtensions
    {
        #region Methods
        public static T Pop<T>(this List<T> list)
        {
            if (list.Any())
            {
                var result = list.Last();
                list.RemoveAt(list.Count - 1);
                return result;
            }

            throw new InvalidOperationException("Empty list cannot be popped");
        }

        public static bool TryPop<T>(this List<T> list, [MaybeNullWhen(false)] out T result)
        {
            if (list.Any())
            {
                result = list.Last();
                list.RemoveAt(list.Count - 1);
                return true;
            }

            result = default(T);
            return false;
        }
        #endregion
    }
}