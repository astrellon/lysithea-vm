using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM.Example
{
    public class PersonValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "name", "age", "address" };
        public IReadOnlyList<string> ObjectKeys => Keys;
        public string TypeName => "person";

        public readonly string Name;
        public readonly int Age;
        public readonly IReadOnlyList<string> Address;
        #endregion

        #region Constructor
        public PersonValue(string name, int age, IReadOnlyList<string> address)
        {
            this.Name = name;
            this.Age = age;
            this.Address = address;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"Person: {this.Name} {this.Age} [{string.Join(", ", this.Address)}]";
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
                    value = new StringValue(this.Name);
                    return true;
                }
                case "age":
                {
                    value = new NumberValue(this.Age);
                    return true;
                }
                case "address":
                {
                    value = new ArrayValue(this.Address.Select(c => new StringValue(c) as IValue));
                    return true;
                }
            }

            value = null;
            return false;
        }
        #endregion
    }
}