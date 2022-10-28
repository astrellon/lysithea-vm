import { CompareResult, IValue } from "./ivalues";

export default class NumberValue implements IValue
{
    public readonly value: number;

    constructor (value: number)
    {
        this.value = value;
    }

    public typename() { return 'number'; }
    public toString() { return this.value.toString(); }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof NumberValue))
        {
            return 1;
        }

        return numberCompareTo(this.value, other.value);
    }
}

export function numberCompareTo(left: number, right: number) : CompareResult
{
    const diff = left - right;
    if (Math.abs(diff) < 0.0001)
    {
        return 0;
    }

    if (diff < 0)
    {
        return -1;
    }

    return 1;
}

export function isNumberValue(input: IValue | undefined): input is NumberValue
{
    return input instanceof NumberValue;
}