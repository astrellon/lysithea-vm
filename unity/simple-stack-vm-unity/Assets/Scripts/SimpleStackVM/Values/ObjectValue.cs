using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public struct ObjectValue : IValue, IReadOnlyDictionary<string, IValue>
    {
        #region Fields
        public readonly IReadOnlyDictionary<string, IValue> Value;

        public IEnumerable<string> Keys => Value.Keys;
        public IEnumerable<IValue> Values => Value.Values;
        public int Count => Value.Count;
        public IValue this[string key] => Value[key];

        public string TypeName => "object";
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
                        if (kvp.Value.CompareTo(otherKvp) != 0)
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

        public override int GetHashCode() => this.Value.GetHashCode();
        public bool ContainsKey(string key) => Value.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, IValue>> GetEnumerator() => Value.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
        #endregion
    }
}