import { CompareResult, IObjectValue, IValue } from "./ivalues";
import { numberCompareTo } from "./numberValue";

export interface ObjectValueMap
{
    readonly [key: string]: IValue
}

export default class ObjectValue implements IObjectValue
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

    public tryGetValue(key: string)
    {
        return this.value[key];
    }

    public objectKeys()
    {
        return this.keys;
    }
}

export function isObjectValue(input: IValue | undefined): input is ObjectValue
{
    return input !== undefined && input instanceof ObjectValue;
}