using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IReadOnlyScope
    {
        bool TryGetKey(string key, out IValue value);
        bool TryGetKey<T>(string key, [NotNullWhen(true)] out T? value) where T : IValue;
        bool IsConstant(string key);

        IReadOnlyDictionary<string, IValue> Values { get; }
        IReadOnlyCollection<string> Constants { get; }
    }

    public class Scope : IReadOnlyScope
    {
        #region Fields
        public static readonly IReadOnlyScope Empty = new Scope();
        private static readonly IReadOnlyDictionary<string, IValue> EmptyScopeValues = new Dictionary<string, IValue>();
        private static readonly IReadOnlyCollection<string> EmptyConstants = new string[0];

        private Dictionary<string, IValue>? values = null;
        private HashSet<string>? constants = null;
        public IReadOnlyDictionary<string, IValue> Values => this.values ?? EmptyScopeValues;
        public IReadOnlyCollection<string> Constants => this.constants ?? EmptyConstants;

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

            if (this.constants != null)
            {
                this.constants.Clear();
            }
        }

        public void CombineScope(IReadOnlyScope input)
        {
            foreach (var kvp in input.Values)
            {
                this.TryDefine(kvp.Key, kvp.Value);
            }

            if (input.Constants.Count > 0 && this.constants == null)
            {
                this.constants = new HashSet<string>();
            }

            foreach (var item in input.Constants)
            {
                this.SetConstant(item);
            }
        }

        public bool TryConstant(string key, IValue value)
        {
            if (this.TryGetKey(key, out var temp))
            {
                return false;
            }

            this.TryDefine(key, value);
            this.SetConstant(key);
            return true;
        }

        public bool TryConstant(string key, BuiltinFunctionValue.BuiltinFunctionDelegate builtinFunction, string name = "")
        {
            var value = new BuiltinFunctionValue(builtinFunction, string.IsNullOrWhiteSpace(name) ? key : name);
            return this.TryConstant(key, value);
        }

        public bool TryDefine(string key, IValue value)
        {
            if (this.IsConstant(key))
            {
                return false;
            }

            if (this.values == null)
            {
                this.values = new Dictionary<string, IValue>();
            }
            this.values[key] = value;

            return true;
        }

        public bool TrySet(string key, IValue value)
        {
            if (this.IsConstant(key))
            {
                return false;
            }

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

        public void SetConstant(string key)
        {
            if (this.constants == null)
            {
                this.constants = new HashSet<string>();
            }

            this.constants.Add(key);
        }

        public bool IsConstant(string key)
        {
            if (this.constants == null)
            {
                return false;
            }
            return this.constants.Contains(key);
        }
        #endregion
    }
}