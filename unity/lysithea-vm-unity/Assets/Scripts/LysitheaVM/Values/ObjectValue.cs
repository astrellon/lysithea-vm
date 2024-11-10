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

        public static ObjectValue Join(IReadOnlyList<IValue> argValues)
        {
            var map = new Dictionary<string, IValue>(argValues.Count / 2);
            for (var i = 0; i < argValues.Count; i++)
            {
                var arg = argValues[i];
                if (arg is StringValue argStr)
                {
                    var key = argStr.Value;
                    var value = argValues[++i];
                    map[key] = value;
                }
                else if (arg is IObjectValue argObj)
                {
                    foreach (var key in argObj.ObjectKeys)
                    {
                        if (argObj.TryGetKey(key, out var value))
                        {
                            map[key] = value;
                        }
                    }
                }
                else
                {
                    var key = arg.ToString();
                    var value = argValues[++i];
                    map[key] = value;
                }
            }

            return new ObjectValue(map);
        }

        #endregion
    }
}