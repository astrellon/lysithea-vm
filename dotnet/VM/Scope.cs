using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public interface IReadOnlyScope
    {
        bool TryGet(IValue key, out IValue value);
        bool TryGetKey(string key, out IValue value);
        bool TryGetProperty(ArrayValue key, out IValue value);

        IReadOnlyDictionary<string, IValue> Values { get; }
    }

    public class Scope : IReadOnlyScope
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
        public void CombineScope(IReadOnlyScope input)
        {
            foreach (var kvp in input.Values)
            {
                this.Define(kvp.Key, kvp.Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Define(string key, IValue value)
        {
            this.values[key] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Define(string key, BuiltinFunctionValue.BuiltinFunctionDelegate builtinFunction)
        {
            this.values[key] = new BuiltinFunctionValue(builtinFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(string key, IValue value)
        {
            if (this.values.ContainsKey(key))
            {
                this.values[key] = value;
                return true;
            }

            if (this.parent != null)
            {
                return this.parent.TrySet(key, value);
            }

            return false;
        }

        public bool TryGet(IValue key, out IValue value)
        {
            if (key is ArrayValue list)
            {
                return this.TryGetProperty(list, out value);
            }

            return this.TryGetKey(key.ToString(), out value);
        }

        public bool TryGetKey(string key, out IValue value)
        {
            if (this.values.TryGetValue(key, out var foundValue))
            {
                value = foundValue;
                return true;
            }

            if (this.parent != null)
            {
                return this.parent.TryGetKey(key, out value);
            }

            value = NullValue.Value;
            return false;
        }

        public bool TryGetProperty(ArrayValue input, out IValue value)
        {
            this.TryGetKey(input[0].ToString(), out var current);
            for (var i = 1; i < input.Count; i++)
            {
                if (current is ObjectValue currentObject)
                {
                    if (!currentObject.TryGetValue(input[i].ToString(), out current))
                    {
                        value = NullValue.Value;
                        return false;
                    }
                }
                else if (current is ArrayValue currentArray)
                {
                    if (!currentArray.TryGet(input[i], out current))
                    {
                        value = NullValue.Value;
                        return false;
                    }
                }
                else
                {
                    value = NullValue.Value;
                    return false;
                }
            }

            value = current;
            return true;
        }
        #endregion
    }
}