using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
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

        public override string ToString() => this.ToStringFormatted(-1, 0);

        public string ToStringFormatted(int indent, int depth)
        {
            var result = new StringBuilder();

            var indent1 = "";
            var indent2 = "";

            if (indent >= 0)
            {
                indent1 = " ".Repeat(indent * depth);
                indent2 = indent1 + " ".Repeat(indent);
            }

            result.Append('{');
            var first = true;
            foreach (var kvp in this.Value)
            {
                if (indent >= 0)
                {
                    result.Append('\n');
                    result.Append(indent2);
                }
                else if (!first)
                {
                    result.Append(' ');
                }
                first = false;

                if (HasWhiteSpace(kvp.Key))
                {
                    result.Append('"');
                    result.Append(kvp.Key);
                    result.Append('"');
                }
                else
                {
                    result.Append(kvp.Key);
                }

                result.Append(' ');
                result.Append(kvp.Value.ToStringFormatted(indent, depth + 1));
            }

            if (!first && indent >= 0)
            {
                result.Append('\n');
                result.Append(indent1);
            }
            result.Append('}');
            return result.ToString();
        }

        public int CompareTo(IValue? other) => StandardObjectLibrary.GeneralCompareTo(this, other);

        private static bool HasWhiteSpace(string input)
        {
            return input.Any(char.IsWhiteSpace);
        }
        #endregion
    }
}