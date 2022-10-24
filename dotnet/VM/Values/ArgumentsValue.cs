using System.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public struct ArgumentsValue : IArrayValue
    {
        #region Fields
        public static readonly ArgumentsValue Empty = new ArgumentsValue(new IValue[0]);
        // IValue
        public string TypeName => "arguments";

        // IArrayValue
        public IReadOnlyList<IValue> ArrayValues => this.Value;
        public int Length => this.Value.Count;

        // Internal
        public readonly IReadOnlyList<IValue> Value;

        public IValue this[int index] => this.Value[index];
        #endregion

        #region Constructor
        public ArgumentsValue(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGet(int index, [NotNullWhen(true)] out IValue result)
        {
            if (index >= 0 && index < this.Value.Count)
            {
                result = this.Value[index];
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public bool TryGet<T>(int index, [NotNullWhen(true)] out T? result) where T : IValue
        {
            if (this.TryGet(index, out var foundValue))
            {
                if (foundValue.GetType() == typeof(T))
                {
                    result = (T)foundValue;
                    return true;
                }
            }

            result = default(T);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IValue Get(int index)
        {
            if (TryGet(index, out var value))
            {
                return value;
            }

            throw new System.IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(int index) where T : IValue
        {
            if (TryGet(index, out var value))
            {
                if (value.GetType() == typeof(T))
                {
                    return (T)value;
                }
                throw new System.Exception($"Unable to cast argument to: {typeof(T).FullName} for {value.ToString()}");
            }

            throw new System.IndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(int index, VirtualMachine.CastValueDelegate<T> cast) where T : IValue
        {
            if (TryGet(index, out var value))
            {
                return cast(value);
            }

            throw new System.IndexOutOfRangeException();
        }

        public ArgumentsValue SubList(int index)
        {
            if (index == 0)
            {
                return this;
            }

            var length = this.Value.Count - index;

            var result = new IValue[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = this.Value[i + index];
            }
            return new ArgumentsValue(result);
        }

        public override string ToString() => StandardArrayLibrary.GeneralToString(this);
        public int CompareTo(IValue? other) => StandardArrayLibrary.GeneralCompareTo(this, other);
        #endregion
    }
}