using System.Linq;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace SimpleStackVM
{
    public struct ObjectValue : IValue
    {
        #region Fields
        public readonly IReadOnlyDictionary<string, IValue> Value;
        public object RawValue => this.Value;
        public bool IsNull => false;
        #endregion

        #region Constructor
        public ObjectValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is ObjectValue otherObject)
            {
                if (this.Value.Count != otherObject.Value.Count)
                {
                    return false;
                }

                foreach (var kvp in this.Value)
                {
                    if (otherObject.Value.TryGetValue(kvp.Key, out var otherKvp))
                    {
                        if (!kvp.Value.Equals(otherKvp.RawValue))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            return false;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append('{');
            var first = true;
            foreach (var kvp in this.Value)
            {
                if (!first)
                {
                    result.Append(',');
                }
                first = false;

                result.Append('"');
                result.Append(kvp.Key);
                result.Append("\":");
                result.Append(kvp.Value.ToString());
            }
            result.Append('}');
            return result.ToString();
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is ObjectValue otherObject)
            {
                var compareLength = this.Value.Count.CompareTo(otherObject.Value);
                if (compareLength != 0)
                {
                    return compareLength;
                }

                foreach (var kvp in this.Value)
                {
                    if (otherObject.Value.TryGetValue(kvp.Key, out var otherValue))
                    {
                        var compare = kvp.Value.CompareTo(otherValue);
                        if (compare != 0)
                        {
                            return compare;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }

                return 0;
            }

            return 1;
        }
        #endregion
    }
}