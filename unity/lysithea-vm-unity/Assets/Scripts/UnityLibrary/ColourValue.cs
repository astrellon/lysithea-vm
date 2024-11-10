using System;
using UnityEngine;

namespace LysitheaVM
{
    public struct ColourValue : IValue
    {
        #region Fields
        public readonly Color Value;

        public string TypeName => "colour";
        #endregion

        #region Constructor
        public ColourValue(Color value)
        {
            this.Value = value;
        }
        #endregion

        #region Methods
        public override string ToString() => this.Value.ToString();

        public string ToStringSerialise()
        {
            return this.ToString();
        }

        public int CompareTo(IValue? other)
        {
            if (other == null || !(other is ColourValue otherColour))
            {
                return 1;
            }

            // Not ideal
            return this.Value.ToString().CompareTo(otherColour.Value.ToString());
        }

        public static ColourValue Cast(IValue input)
        {
            if (input is ColourValue colourValue)
            {
                return colourValue;
            }

            if (input is StringValue stringValue)
            {
                if (ColorUtility.TryParseHtmlString(stringValue.Value, out var parsed))
                {
                    return new ColourValue(parsed);
                }
                throw new Exception($"Invalid Colour string: {stringValue}");
            }

            throw new Exception($"Invalid Colour cast: {input.ToString()}");
        }
        #endregion
    }
}