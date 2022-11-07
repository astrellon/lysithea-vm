import { CompareResult, IValue } from "./ivalues";

export default class BoolValue implements IValue
{
    public static readonly True: BoolValue = new BoolValue(true);
    public static readonly False: BoolValue = new BoolValue(false);

    public readonly value: boolean;

    constructor (value: boolean)
    {
        this.value = value;
    }

    public typename() { return 'boolean'; }
    public toString() { return this.value ? 'true' : 'false'; }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof BoolValue))
        {
            return 1;
        }

        return boolCompareTo(this.value, other.value);
    }
}

export function boolCompareTo(left: boolean, right: boolean): CompareResult
{
    if (left === right)
    {
        return 0;
    }
    return left ? -1 : 1;
}

export function isBoolValue(input: IValue | undefined): input is BoolValue
{
    return input instanceof BoolValue;
}