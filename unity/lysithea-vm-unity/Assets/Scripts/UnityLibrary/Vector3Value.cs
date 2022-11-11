using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#nullable enable

namespace LysitheaVM
{
    public struct Vector3Value : IObjectValue
    {
        #region Fields
        public static readonly IReadOnlyList<string> Keys = new[] { "x", "y", "z" };
        public IReadOnlyList<string> ObjectKeys => Keys;

        public readonly Vector3 Value;

        public string TypeName => "vector3";
        #endregion

        #region Constructor
        public Vector3Value(Vector3 value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override string ToString() => this.Value.ToString();

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is Vector3Value otherVector))
            {
                return 1;
            }

            // Not ideal
            return this.Value.ToString().CompareTo(otherVector.Value.ToString());
        }

        public static Vector3Value Cast(IValue input)
        {
            if (input is Vector3Value vector3Value)
            {
                return vector3Value;
            }

            if (input is NumberValue numberValue)
            {
                return new Vector3Value(Vector3.one * numberValue.FloatValue);
            }

            throw new Exception($"Invalid Vector3 cast: {input.ToString()}");
        }

        public bool TryGetKey(string key, [NotNullWhen(true)] out IValue value)
        {
            switch (key)
            {
                case "x": value = new NumberValue(this.Value.x); return true;
                case "y": value = new NumberValue(this.Value.y); return true;
                case "z": value = new NumberValue(this.Value.z); return true;
            }

            value = NullValue.Value;
            return false;
        }
        #endregion
    }
}