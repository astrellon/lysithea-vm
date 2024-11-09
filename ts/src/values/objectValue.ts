import { Editable } from "../standardLibrary/standardObjectLibrary";
import { ArrayValue } from "./arrayValue";
import { CompareResult, IObjectValue, isIObjectValue, IValue } from "./ivalues";
import { numberCompareTo } from "./numberValue";
import { isStringValue } from "./stringValue";

export interface ObjectValueMap
{
    readonly [key: string]: IValue
}

export class ObjectValue implements IObjectValue
{
    public readonly value: ObjectValueMap;
    public readonly keys: ReadonlyArray<string>;

    constructor (value: ObjectValueMap)
    {
        this.value = value;
        this.keys = Object.keys(value);
    }

    public typename() { return 'object'; }
    public toString()
    {
        let first = true;
        let result = '{';

        for (const prop in this.value)
        {
            if (!first)
            {
                result += ' ';
            }
            first = false;

            result += `"${prop}" `;
            result += this.value[prop].toString();
        }

        result += '}';
        return result;
    }

    public compareTo(other: IValue): CompareResult
    {
        if ((this as any) === other) { return 0; }
        if (!(other instanceof ObjectValue)) { return 1; }

        const compareLength = numberCompareTo(this.keys.length, other.keys.length);
        if (compareLength !== 0)
        {
            return compareLength;
        }

        // Looping over the object is faster than looping over the keys
        for (const prop in this.value)
        {
            const otherValue = other.value[prop];
            if (otherValue === undefined)
            {
                return 1;
            }

            const compare = this.value[prop].compareTo(otherValue);
            if (compare !== 0)
            {
                return compare;
            }
        }

        return 0;
    }

    public tryGetKey(key: string)
    {
        return this.value[key];
    }

    public objectKeys()
    {
        return this.keys;
    }

    public static join(args: ArrayValue)
    {
        const map: Editable<ObjectValueMap> = {};
        const argValues = args.arrayValues();

        for (let i = 0; i < argValues.length; i++)
        {
            const arg = argValues[i];
            if (isStringValue(arg))
            {
                const key = arg.value;
                const value = argValues[++i];
                map[key] = value;
            }
            else if (isIObjectValue(arg))
            {
                for (const key of arg.objectKeys())
                {
                    const value = arg.tryGetKey(key);
                    if (value !== undefined)
                    {
                        map[key] = value;
                    }
                }
            }
            else
            {
                const key = arg.toString();
                const value = argValues[++i];
                map[key] = value;
            }
        }

        return new ObjectValue(map);
    }
}

export function isObjectValue(input: IValue | undefined): input is ObjectValue
{
    return input !== undefined && input instanceof ObjectValue;
}