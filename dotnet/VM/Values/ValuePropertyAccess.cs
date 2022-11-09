using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public static class ValuePropertyAccess
    {
        public static bool TryGetProperty(IValue current, ArrayValue properties, [NotNullWhen(true)] out IValue? result)
        {
            result = NullValue.Value;
            for (var i = 0; i < properties.Length; i++)
            {
                if (TryParseIndex(properties[i], out var index) && current is IArrayValue currentArray)
                {
                    if (!currentArray.TryGetIndex(index, out var test))
                    {
                        return false;
                    }
                    current = test;
                }
                else if (current is IObjectValue currentObject)
                {
                    if (!currentObject.TryGetKey(properties[i].ToString(), out var test))
                    {
                        result = NullValue.Value;
                        return false;
                    }
                    current = test;
                }
                else
                {
                    return false;
                }
            }

            result = current;
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
            else if (input is VariableValue variableValue && int.TryParse(variableValue.Value, out result))
            {
                return true;
            }

            result = -1;
            return false;
        }
    }
}