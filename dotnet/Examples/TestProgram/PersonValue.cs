using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM.Example
{
    public class PersonValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "name", "age", "address" };
        public IReadOnlyList<string> ObjectKeys => Keys;
        public string TypeName => "person";

        public readonly StringValue Name;
        public readonly NumberValue Age;
        public readonly ArrayValue Address;
        #endregion

        #region Constructor
        public PersonValue(StringValue name, NumberValue age, ArrayValue address)
        {
            this.Name = name;
            this.Age = age;
            this.Address = address;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"Person: {this.Name} {this.Age} {this.Address}";
        }

        public int CompareTo(IValue? other)
        {
            return StandardObjectLibrary.GeneralCompareTo(this, other);
        }

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue? value)
        {
            switch (key)
            {
                case "name":
                {
                    value = this.Name;
                    return true;
                }
                case "age":
                {
                    value = this.Age;
                    return true;
                }
                case "address":
                {
                    value = this.Address;
                    return true;
                }
            }

            value = null;
            return false;
        }
        #endregion
    }
}