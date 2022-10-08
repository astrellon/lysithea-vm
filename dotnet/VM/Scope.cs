using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM
{
    public class Scope
    {
        #region Fields
        private readonly Dictionary<string, IValue> values = new Dictionary<string, IValue>();
        public IReadOnlyDictionary<string, IValue> Values => this.values;

        private readonly Scope? parent;
        #endregion

        #region Constructor
        public Scope(Scope? parent = null)
        {
            this.parent = parent;
        }
        #endregion

        #region Methods
        public void Set(string key, IValue value)
        {
            this.values[key] = value;
        }

        public bool TryGet(string key, out IValue value)
        {
            if (this.values.TryGetValue(key, out var foundValue))
            {
                value = foundValue;
                return true;
            }

            if (this.parent != null)
            {
                return this.parent.TryGet(key, out value);
            }

            value = NullValue.Value;
            return false;
        }

        public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (this.TryGet(key, out var testValue))
            {
                if (testValue is T result)
                {
                    value = result;
                    return true;
                }
            }

            value = default(T);
            return false;
        }
        #endregion
    }
}