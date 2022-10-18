using System.Collections.Generic;

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
        public static readonly IReadOnlyScope Empty = new Scope();

        private readonly Dictionary<string, IValue> values = new Dictionary<string, IValue>();
        public IReadOnlyDictionary<string, IValue> Values => this.values;

        public Scope? Parent;
        #endregion

        #region Constructor
        public Scope(Scope? parent = null)
        {
            this.Parent = parent;
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

        public void Define(string key, IValue value)
        {
            this.values[key] = value;
        }

        public void Define(string key, BuiltinFunctionValue.BuiltinFunctionDelegate builtinFunction)
        {
            this.values[key] = new BuiltinFunctionValue(builtinFunction);
        }

        public bool TrySet(string key, IValue value)
        {
            if (this.values.ContainsKey(key))
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

            if (this.Parent != null)
            {
                return this.Parent.TryGetKey(key, out value);
            }

            value = NullValue.Value;
            return false;
        }

        public bool TryGetProperty(ArrayValue input, out IValue value)
        {
            this.TryGetKey(input[0].ToString(), out var current);
            for (var i = 1; i < input.ArrayLength; i++)
            {
                if (current is IObjectValue currentObject)
                {
                    if (!currentObject.TryGetValue(input[i].ToString(), out current))
                    {
                        value = NullValue.Value;
                        return false;
                    }
                }
                else if (current is IArrayValue currentArray)
                {
                    if (TryParseIndex(input[i], out var index))
                    {
                        if (!currentArray.TryGet(index, out current))
                        {
                            value = NullValue.Value;
                            return false;
                        }
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

        public static bool TryParseIndex(IValue input, out int result)
        {
            if (input is NumberValue numberValue)
            {
                result = numberValue.IntValue;
                return result >= 0;
            }
            else if (input is StringValue stringValue && int.TryParse(stringValue.Value, out result))
            {
                return true;
            }

            result = -1;
            return false;
        }
        #endregion
    }
}