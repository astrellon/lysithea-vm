#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LysitheaVM
{
    public struct StringValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "length" };
        public IReadOnlyList<string> ObjectKeys => Keys;
        public string TypeName => "string";

        public readonly string Value;
        #endregion

        #region Constructor
        public StringValue(string value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public int CompareTo(IValue? other)
        {
            if (other == null) return 1;
            if (other is StringValue otherString)
            {
                return this.Value.CompareTo(otherString.Value);
            }

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int index)
        {
            if (index < 0)
            {
                return this.Value.Length + index;
            }

            return index;
        }

        public override string ToString() => this.Value;
        public string ToStringSerialise() => '"' + StandardStringLibrary.EscapedString(this.Value) + '"';

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value)
        {
            if (key == "length")
            {
                value = new NumberValue(this.Value.Length);
                return true;
            }

            value = null;
            return false;
        }
        #endregion
    }
}