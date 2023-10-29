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

        public List<T> Copy()
        {
            var result = new List<T>(this.data.Length);
            for (var i = 0; i <= this.index; i++)
            {
                var item = this.data[i];
                if (item != null)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public List<T2> Copy<T2>(System.Func<T, int, T2> copyCallback)
        {
            var result = new List<T2>(this.data.Length);
            for (var i = 0; i <= this.index; i++)
            {
                var item = this.data[i];
                if (item != null)
                {
                    var copied = copyCallback.Invoke(item, i);
                    result.Add(copied);
                }
            }
            return result;
        }

        public void From(IReadOnlyList<T> input)
        {
            if (this.data.Length < input.Count)
            {
                throw new InvalidOperationException("Given stack has more data than this stack can fit");
            }

            for (var i = 0; i < input.Count; i++)
            {
                this.data[i] = input[i];
            }
            this.index = input.Count - 1;
        }

        public void From<T2>(IReadOnlyList<T2> input, System.Func<T2, int, T> copyCallback)
        {
            if (this.data.Length < input.Count)
            {
                throw new InvalidOperationException("Given stack has more data than this stack can fit");
            }

            for (var i = 0; i < input.Count; i++)
            {
                var item = input[i];
                if (item != null)
                {
                    this.data[i] = copyCallback.Invoke(item, i);
                }
            }
            this.index = input.Count - 1;
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