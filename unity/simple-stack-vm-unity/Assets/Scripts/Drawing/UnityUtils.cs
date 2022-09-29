using System;
using System.Linq;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public static class UnityUtils
    {
        #region Methods
        public static bool TryGetColour(IValue input, out Color result)
        {
            if (input is AnyValue colourAny)
            {
                if (colourAny.Value is Color unityColour)
                {
                    result = unityColour;
                    return true;
                }
            }
            else if (input is StringValue stringValue)
            {
                if (ColorUtility.TryParseHtmlString(stringValue.Value, out result))
                {
                    return true;
                }
            }

            result = Color.black;
            return false;
        }

        public static bool TryGetVector(IValue input, out Vector3 result)
        {
            if (input is AnyValue anyInput && anyInput.Value is Vector3 inputVector)
            {
                result = inputVector;
                return true;
            }
            else if (input is ObjectValue objectValue)
            {
                result = Vector3.zero;
                if (objectValue.TryGetValue<NumberValue>("x", out var x))
                {
                    result.x = x;
                }
                if (objectValue.TryGetValue<NumberValue>("y", out var y))
                {
                    result.y = y;
                }
                if (objectValue.TryGetValue<NumberValue>("z", out var z))
                {
                    result.z = z;
                }

                return true;
            }
            else if (input is NumberValue numberValue)
            {
                result = Vector3.one * (float)numberValue.Value;
                return true;
            }

            result = Vector3.zero;
            return false;
        }
        #endregion
    }
}