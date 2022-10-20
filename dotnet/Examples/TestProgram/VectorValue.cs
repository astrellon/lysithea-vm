using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace SimpleStackVM.Example
{
    public class VectorValue : IObjectValue
    {
        #region Fields
        private static readonly IReadOnlyList<string> Keys = new [] { "x", "y", "z", "add" };
        public IReadOnlyList<string> ObjectKeys => Keys;

        public string TypeName => "vector";

        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        #endregion

        #region Constructor
        public VectorValue(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{this.X}, {this.Y}, {this.Z}";
        }

        public int CompareTo(IValue? other)
        {
            return StandardObjectLibrary.GeneralCompareTo(this, other);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out IValue? value)
        {
            switch (key)
            {
                case "x":
                {
                    value = new NumberValue(this.X);
                    return true;
                }
                case "y":
                {
                    value = new NumberValue(this.Y);
                    return true;
                }
                case "z":
                {
                    value = new NumberValue(this.Z);
                    return true;
                }
                case "add":
                {
                    value = new ClassBuiltinFunctionValue<VectorValue>(this, Add);
                    return true;
                }
            }

            value = null;
            return false;
        }

        public static void Add(VectorValue self, VirtualMachine vm, int numArgs)
        {
            var other = vm.PopStack<VectorValue>();
            var x = self.X + other.X;
            var y = self.Y + other.Y;
            var z = self.Z + other.Z;
            vm.PushStack(new VectorValue(x, y, z));
        }
        #endregion
    }
}