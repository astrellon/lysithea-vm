using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace LysitheaVM
{
    public interface IReadOnlyFixedStack<T>
    {
        int Index { get; }
        IReadOnlyList<T?> Data { get; }
        bool TryPeek([MaybeNullWhen(false)] [NotNullWhen(true)] out T result);
    }

    public class FixedStack<T> : IReadOnlyFixedStack<T>
    {
        #region Fields
        private readonly T?[] data;
        private int index = -1;

        public int Index => this.index;
        public int StackSize => this.index + 1;
        public IReadOnlyList<T?> Data => this.data;
        #endregion

        #region Constructor
        public FixedStack(int size)
        {
            this.data = new T[size];
        }
        #endregion

        #region Methods
        public void Clear()
        {
            this.index = -1;
            for (var i = 0; i < this.data.Length; i++)
            {
                this.data[i] = default(T);
            }
        }

        public FixedStack<T> Copy()
        {
            var result = new FixedStack<T>(this.data.Length);
            for (var i = 0; i <= this.index; i++)
            {
                var item = this.data[i];
                if (item != null)
                {
                    result.TryPush(item);
                }
            }
            return result;
        }

        public FixedStack<T2> Copy<T2>(System.Func<T, int, T2> copyCallback)
        {
            var result = new FixedStack<T2>(this.data.Length);
            for (var i = 0; i <= this.index; i++)
            {
                var item = this.data[i];
                if (item != null)
                {
                    var copied = copyCallback.Invoke(item, i);
                    result.TryPush(copied);
                }
            }
            return result;
        }

        public void From(IReadOnlyFixedStack<T> input)
        {
            if (this.data.Length < input.Index)
            {
                throw new InvalidOperationException("Given stack has more data than this stack can fit");
            }

            for (var i = 0; i <= input.Index; i++)
            {
                this.data[i] = input.Data[i];
            }
            this.index = input.Index;
        }

        public void From<T2>(IReadOnlyFixedStack<T2> input, System.Func<T2, int, T> copyCallback)
        {
            if (this.data.Length < input.Index)
            {
                throw new InvalidOperationException("Given stack has more data than this stack can fit");
            }

            for (var i = 0; i <= input.Index; i++)
            {
                var item = input.Data[i];
                if (item != null)
                {
                    this.data[i] = copyCallback.Invoke(item, i);
                }
            }
            this.index = input.Index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(T item)
        {
            if (this.index >= this.data.Length)
            {
                return false;
            }
            this.data[++this.index] = item;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop([MaybeNullWhen(false)] [NotNullWhen(true)] out T result)
        {
            if (this.index < 0)
            {
                result = default(T);
                return false;
            }

            result = this.data[this.index--];
            return result != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek([MaybeNullWhen(false)] [NotNullWhen(true)] out T result)
        {
            if (this.index < 0)
            {
                result = default(T);
                return false;
            }

            result = this.data[this.index];
            return result != null;
        }
        #endregion
    }
}