import { isBoolValue } from "./boolValue";
import { CompareResult, IArrayValue, IObjectValue, IValue } from "./ivalues";
import { isNumberValue, numberCompareTo } from "./numberValue";
import { isStringValue } from "./stringValue";

const keys: ReadonlyArray<string> = [ "length" ];

export default class ArrayValue implements IArrayValue, IObjectValue
{
    public static readonly Empty = new ArrayValue([], false);
    public static readonly EmptyArgs = new ArrayValue([], true);

    public readonly value: ReadonlyArray<IValue>;
    public readonly isArgumentValue: boolean;

    constructor (value: ReadonlyArray<IValue>, isArgumentValue: boolean = false)
    {
        this.value = value;
        this.isArgumentValue = isArgumentValue;
    }

    public typename() { return this.isArgumentValue ? 'arguments' : 'array'; }
    public toString() { return ''; }
    public arrayValues() { return this.value; }

    public compareTo(other: IValue): CompareResult
    {
        if ((this as any) === other) { return 0; }
        if (!(other instanceof ArrayValue)) { return 1; }

        return arrayCompareTo(this, other);
    }

    public calcIndex(index: number): number
    {
        if (index < 0)
        {
            return this.value.length + index;
        }

        return index;
    }

    public tryGetIndex(index: number): IValue | undefined
    {
        return this.value[this.calcIndex(index)];
    }

    public getIndex(index: number): IValue
    {
        if (index < 0 || index >= this.value.length)
        {
            throw new Error('Out of index');
        }

        return this.value[index];
    }

    public getIndexCast<T>(index: number, guardCheck: (v: any) => v is T): T
    {
        const value = this.value[index];
        if (guardCheck(value))
        {
            return value as T;
        }

        throw new Error(`Unable to get argument: [${index}] failed guard check`);
    }

    public getNumber(index: number)
    {
        return this.getIndexCast(index, isNumberValue).value;
    }

    public getString(index: number)
    {
        return this.getIndexCast(index, isStringValue).value;
    }

    public getBool(index: number)
    {
        return this.getIndexCast(index, isBoolValue).value;
    }

    public tryGetKey(key: string): IValue | undefined
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