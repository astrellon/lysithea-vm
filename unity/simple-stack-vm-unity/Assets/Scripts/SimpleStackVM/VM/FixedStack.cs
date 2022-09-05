using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public interface IReadOnlyFixedStack<T>
    {
        int Index { get; }
        IReadOnlyList<T> Data { get; }
    }

    public class FixedStack<T> : IReadOnlyFixedStack<T>
    {
        #region Fields
        private readonly T[] data;
        private int index = 0;

        public int Index => this.index;
        public IReadOnlyList<T> Data => this.data;
        #endregion

        #region Constructor
        public FixedStack(int size)
        {
            this.data = new T[size];
        }
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(T item)
        {
            if (this.index >= this.data.Length)
            {
                return false;
            }
            this.data[this.index++] = item;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop([MaybeNullWhen(false)] [NotNullWhen(true)] out T? result)
        {
            if (this.index <= 0)
            {
                result = default(T);
                return false;
            }

            result = this.data[--this.index];
            return result != null;
        }
        #endregion
    }
}