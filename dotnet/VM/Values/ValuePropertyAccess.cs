#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace SimpleStackVM
{
    public static class ValuePropertyAccess
    {
        public static bool TryGetProperty(IValue current, ArrayValue properties, [NotNullWhen(true)] out IValue? result)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                if (current is IObjectValue currentObject)
                {
                    if (!currentObject.TryGetValue(properties[i].ToString(), out var test))
                    {
                        result = NullValue.Value;
                        return false;
                    }
                    current = test;
                }
                else if (current is IArrayValue currentArray)
                {
                    if (TryParseIndex(properties[i], out var index))
                    {
                        if (!currentArray.TryGet(index, out var test))
                        {
                            result = NullValue.Value;
                            return false;
                        }
                        current = test;
                    }
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