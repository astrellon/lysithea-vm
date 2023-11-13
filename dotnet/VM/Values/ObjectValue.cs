using System.Linq;
using System.Collections.Generic;
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

        public override string ToString() => StandardObjectLibrary.GeneralToString(this, serialise: false);
        public string ToStringSerialise() => StandardObjectLibrary.GeneralToString(this, serialise: true);

        public int CompareTo(IValue? other) => StandardObjectLibrary.GeneralCompareTo(this, other);
        #endregion
    }
}