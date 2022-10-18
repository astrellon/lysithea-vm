using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ArrayValue : IArrayValue
    {
        #region Fields
        public static ArrayValue Empty = new ArrayValue(new IValue[0]);

        public readonly IReadOnlyList<IValue> Value;

        public string TypeName => "array";

        public IEnumerable<IValue> ArrayValues => this.Value;
        public int ArrayLength => this.Value.Count;
        public IValue this[int index] => Value[this.GetIndex(index)];

        #endregion

        #region Constructor
        public ArrayValue(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Count + index;
            }

            return index;
        }

        public bool TryGet(int index, [NotNullWhen(true)] out IValue result)
        {
            index = this.GetIndex(index);
            if (index >= 0 && index < this.Value.Count)
            {
                result = this.Value[index];
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append('(');
            var first = true;
            foreach (var value in this.Value)
            {
                if (!first)
                {
                    result.Append(' ');
                }
                first = false;

                result.Append(value.ToString());
            }

            result.Append(')');
            return result.ToString();
        }

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is ArrayValue otherArray)
            {
                var compareLength = this.Value.Count.CompareTo(otherArray.Value.Count);
                if (compareLength != 0)
                {
                    return compareLength;
                }

                for (var i = 0; i < this.Value.Count; i++)
                {
                    var compare = this.Value[i].CompareTo(otherArray.Value[i]);
                    if (compare != 0)
                    {
                        return compare;
                    }
                }

                return 0;
            }

            return 1;
        }
        #endregion
    }
}