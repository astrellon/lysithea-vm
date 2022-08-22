using System.Collections.Generic;
using System.Text;

namespace SimpleStackVM
{
    public struct ObjectValue : IValue
    {
        #region Fields
        public readonly IReadOnlyDictionary<string, IValue> Value;
        public object RawValue => this.Value;
        #endregion

        #region Constructor
        public ObjectValue(IReadOnlyDictionary<string, IValue> value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other is ObjectValue otherObject)
            {
                if (this.Value.Count != otherObject.Value.Count)
                {
                    return false;
                }

                foreach (var kvp in this.Value)
                {
                    if (otherObject.Value.TryGetValue(kvp.Key, out var otherKvp))
                    {
                        if (!kvp.Value.Equals(otherKvp.RawValue))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            return false;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append('{');
            var first = true;
            foreach (var kvp in this.Value)
            {
                if (!first)
                {
                    result.Append(',');
                }
                first = false;

                result.Append(kvp.Key);
                result.Append(':');
                result.Append(kvp.Value.ToString());
            }
            result.Append('}');
            return result.ToString();
        }

        public bool TryGetValue(string path, out IValue result)
        {
            var split = path.Split('.');
            return this.TryGetValue(new ObjectPath(split, 0), out result);
        }

        public bool TryGetValue(ObjectPath path, out IValue result)
        {
            if (this.Value.TryGetValue(path.Current, out var tempResult))
            {
                if (path.HasMorePath && tempResult is ObjectValue nextObject)
                {
                    return nextObject.TryGetValue(path.NextIndex(), out result);
                }

                result = tempResult;
                return true;
            }

            result = NullValue.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #endregion
    }
}