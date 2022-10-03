using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace SimpleStackVM
{
    public class Scope
    {
        #region Fields
        private readonly Dictionary<string, IValue> values = new Dictionary<string, IValue>();
        public IReadOnlyDictionary<string, IValue> Values => this.values;
        #endregion

        #region Constructor
        public Scope()
        {

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

            value = NullValue.Value;
            return false;
        }
        #endregion
    }
}