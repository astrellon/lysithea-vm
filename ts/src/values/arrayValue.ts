import { CompareResult, IArrayValue, IObjectValue, IValue } from "./ivalues";
import { numberCompareTo } from "./numberValue";

const keys: ReadonlyArray<string> = [ "length" ];

export default class ArrayValue implements IArrayValue, IObjectValue
{
    public static readonly Empty = new ArrayValue([]);

    public readonly value: ReadonlyArray<IValue>;

    constructor (value: ReadonlyArray<IValue>)
    {
        this.value = value;
    }

    public typename() { return 'array'; }
    public toString() { return ''; }
    public arrayValues() { return this.value; }

    public compareTo(other: IValue): CompareResult
    {
        if ((this as any) === other) { return 0; }
        if (!(other instanceof ArrayValue)) { return 1; }

        return arrayCompareTo(this, other);
    }

    public getIndex(index: number): number
    {
        if (index < 0)
        {
            return this.value.length + index;
        }

        return index;
    }

    public get(index: number): IValue | undefined
    {
        return this.value[this.getIndex(index)];
    }

    public getValue(key: string): IValue | undefined
    {
        return undefined;
    }

    public objectKeys()
    {
        return keys;
    }
}

export function arrayCompareTo(left: IArrayValue, right: IArrayValue): CompareResult
{
    const leftValue = left.arrayValues();
    const rightValue = right.arrayValues();
    const compareLength = numberCompareTo(leftValue.length, rightValue.length);
    if (compareLength !== 0)
    {
        return compareLength;
    }

    for (let i = 0; i < leftValue.length; i++)
    {
        const compare = leftValue[i].compareTo(rightValue[i]);
        if (compare !== 0)
        {
            return compare;
        }
    }

    return 0;
}

export function isArrayValue(input: IValue | undefined): input is ArrayValue
{
    return input !== undefined && input instanceof ArrayValue;
}