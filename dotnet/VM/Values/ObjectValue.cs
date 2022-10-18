using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ObjectValue : IObjectValue
    {
        #region Fields
        public readonly IReadOnlyDictionary<string, IValue> Value;
        public string TypeName => "object";

        public IEnumerable<KeyValuePair<string, IValue>> ObjectValues => this.Value;
        public int ObjectLength => this.Value.Count;
        #endregion

        #region Constructor
        public ObjectValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGetValue(string key, [NotNullWhen(true)] out IValue? value)
        {
            return this.Value.TryGetValue(key, out value);
        }

        public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (this.TryGetValue(key, out var result))
            {
                if (result is T castedValue)
                {
                    value = castedValue;
                    return true;
                }
            }

            value = default(T);
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
                    result.Append(' ');
                }
                first = false;

                result.Append(kvp.Key);
                result.Append(' ');
                result.Append(kvp.Value.ToString());
            }
            result.Append('}');
            return result.ToString();
        }

        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is ObjectValue otherObject)
            {
                var compareLength = this.Value.Count.CompareTo(otherObject.Value.Count);
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