using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace SimpleStackVM
{
    public interface IReadOnlyScope
    {
        bool TryGet(string key, out IValue value);
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
        public void Define(string key, IValue value)
        {
            this.values[key] = value;
        }

        public void Define(string key, Action<VirtualMachine> builtinProcedure)
        {
            this.values[key] = new BuiltinProcedureValue(builtinProcedure);
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        #endregion
    }
}