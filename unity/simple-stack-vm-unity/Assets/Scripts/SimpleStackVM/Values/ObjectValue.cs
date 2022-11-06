using System.Linq;
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

        public IReadOnlyList<string> ObjectKeys => this.Value.Keys.ToList();

        public IValue this[string key] => this.Value[key];
        #endregion

        #region Constructor
        public ObjectValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value)
        {
            return this.Value.TryGetValue(key, out value);
        }

        public bool TryGetKey<T>(string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (this.TryGetKey(key, out var result))
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

                result.Append('"');
                result.Append(kvp.Key);
                result.Append('"');

                result.Append(' ');
                result.Append(kvp.Value.ToString());
            }
            result.Append('}');
            return result.ToString();
        }

        public int CompareTo(IValue? other) => StandardObjectLibrary.GeneralCompareTo(this, other);
        #endregion
    }
}