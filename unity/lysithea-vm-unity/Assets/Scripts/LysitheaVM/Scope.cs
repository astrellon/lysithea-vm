using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IReadOnlyScope
    {
        bool TryGetKey(string key, out IValue value);

        IReadOnlyDictionary<string, IValue> Values { get; }
    }

    public class Scope : IReadOnlyScope
    {
        #region Fields
        public static readonly IReadOnlyScope Empty = new Scope();
        private static readonly IReadOnlyDictionary<string, IValue> EmptyScopeValues = new Dictionary<string, IValue>();

        private Dictionary<string, IValue>? values = null;
        public IReadOnlyDictionary<string, IValue> Values => this.values ?? EmptyScopeValues;

        public Scope? Parent;
        #endregion

        #region Constructor
        public Scope(Scope? parent = null)
        {
            this.Parent = parent;
        }
        #endregion

        #region Methods
        public void Clear()
        {
            if (this.values != null)
            {
                this.values.Clear();
            }
        }

        public void CombineScope(IReadOnlyScope input)
        {
            foreach (var kvp in input.Values)
            {
                this.Define(kvp.Key, kvp.Value);
            }
        }

        public void Define(string key, IValue value)
        {
            if (this.values == null)
            {
                this.values = new Dictionary<string, IValue>();
            }
            this.values[key] = value;
        }

        public void Define(string key, BuiltinFunctionValue.BuiltinFunctionDelegate builtinFunction)
        {
            if (this.values == null)
            {
                this.values = new Dictionary<string, IValue>();
            }
            this.values[key] = new BuiltinFunctionValue(builtinFunction);
        }

        public bool TrySet(string key, IValue value)
        {
            if (this.values != null && this.values.ContainsKey(key))
            {
                this.values[key] = value;
                return true;
            }

            if (this.Parent != null)
            {
                return this.Parent.TrySet(key, value);
            }

            return false;
        }

        public bool TryGetKey(string key, out IValue value)
        {
            if (this.values != null && this.values.TryGetValue(key, out var foundValue))
            {
                value = foundValue;
                return true;
            }

            if (this.Parent != null)
            {
                return this.Parent.TryGetKey(key, out value);
            }

            value = NullValue.Value;
            return false;
        }

        public bool TryGetKey<T>(string key, [NotNullWhen(true)] out T? value) where T : IValue
        {
            if (this.TryGetKey(key, out var result))
            {
                if (result is T casted)
                {
                    value = casted;
                    return true;
                }
            }

            value = default(T);
            return false;
        }
        #endregion
    }
}