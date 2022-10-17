using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public struct ArrayValue : IValue, IReadOnlyList<IValue>
    {
        #region Fields
        public static ArrayValue Empty = new ArrayValue(new IValue[0]);

        public readonly IReadOnlyList<IValue> Value;

        public int Count => Value.Count;
        public IValue this[int index] => Value[this.GetIndex(index)];

        public string TypeName => "array";
        #endregion

        #region Constructor
        public ArrayValue(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGet(IValue indexValue, out IValue result)
        {
            var index = -1;
            if (indexValue is NumberValue indexNum)
            {
                index = indexNum.IntValue;
            }
            else if (indexValue is StringValue || indexValue is VariableValue)
            {
                int.TryParse(indexValue.ToString(), out index);
            }

            if (index >= 0 && index < this.Value.Count)
            {
                result = this.Value[index];
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is ArrayValue otherArray)
            {
                if (this.Value.Count != otherArray.Value.Count)
                {
                    return false;
                }

                for (var i = 0; i < this.Value.Count; i++)
                {
                    if (!this.Value[i].Equals(otherArray.Value[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

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

        public override int GetHashCode() => this.Value.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Count + index;
            }

            return index;
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

        public IEnumerator<IValue> GetEnumerator() => Value.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
        #endregion
    }
}