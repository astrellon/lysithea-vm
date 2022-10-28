import { CompareResult, IObjectValue, IValue } from "./ivalues";

const keys: ReadonlyArray<string> = [ 'length' ];
export default class StringValue implements IObjectValue
{
    public readonly value: string;

    constructor (value: string)
    {
        this.value = value;
    }

    public typename() { return 'string'; }
    public toString() { return this.value; }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof StringValue))
        {
            return 1;
        }

        const diff = this.value.localeCompare(other.value);
        if (diff == 0) { return 0; }
        if (diff < 0) { return -1; }
        return 1;
    }

    public getIndex(index: number): number
    {
        if (index < 0)
        {
            return this.value.length + index;
        }

        return index;
    }

    public getValue(key: string)
    {
        return undefined;
    }

    public objectKeys()
    {
        return keys;
    }
}

export function isStringValue(input: IValue | undefined): input is StringValue
{
    return input instanceof StringValue;
}