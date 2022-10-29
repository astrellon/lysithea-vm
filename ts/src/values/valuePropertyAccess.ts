import { IArrayValue, isIArrayValue, isIObjectValue, IValue } from "./ivalues";
import NumberValue from "./numberValue";
import StringValue from "./stringValue";
import VariableValue from "./variableValue";

export function getProperty(current: IValue, properties: IArrayValue): IValue | undefined
{
    const propValues = properties.arrayValues();
    for (let i = 0; i < propValues.length; i++)
    {
        const index = parseIndex(propValues[i]);
        if (index !== undefined && isIArrayValue(current))
        {
            const test = current.tryGetIndex(index);
            if (test === undefined)
            {
                return undefined;
            }
            current = test;
        }
        else if (isIObjectValue(current))
        {
            const test = current.tryGetKey(propValues[i].toString());
            if (test === undefined)
            {
                return undefined;
            }
            current = test;
        }

        return undefined;
    }

    return current;
}

export function parseIndex(input: IValue): number | undefined
{
    if (input instanceof NumberValue)
    {
        return input.value;
    }
    else if (input instanceof StringValue || input instanceof VariableValue)
    {
        const index = parseInt(input.value);
        return isFinite(index) ? index : undefined;
    }

    return undefined;
}