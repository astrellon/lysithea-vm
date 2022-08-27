using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public class FixedStack<T>
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
        public bool TryPush(T item)
        {
            if (this.index >= this.data.Length)
            {
                return false;
            }
            this.data[this.index++] = item;
            return true;
        }

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