import { arrayCompareTo } from "./arrayValue";
import { isBoolValue } from "./boolValue";
import { CompareResult, IArrayValue, IValue } from "./ivalues";
import { isNumberValue } from "./numberValue";
import { isStringValue } from "./stringValue";

export default class ArgumentsValue implements IArrayValue
{
    public static readonly Empty = new ArgumentsValue([]);

    public readonly value: ReadonlyArray<IValue>;

    constructor (value: ReadonlyArray<IValue>)
    {
        this.value = value;
    }

    public typename() { return 'arguments'; }
    public toString() { return ''; }
    public arrayValues() { return this.value; }

    public compareTo(other: IValue): CompareResult
    {
        if ((this as any) === other) { return 0; }
        if (!(other instanceof ArgumentsValue)) { return 1; }

        return arrayCompareTo(this, other);
    }

    public tryGet(index: number): IValue | undefined
    {
        return this.value[index];
    }

    public get(index: number): IValue
    {
        if (index < 0 || index >= this.value.length)
        {
            throw new Error('Out of index');
        }

        return this.value[index];
    }

    public getCast<T>(index: number, guardCheck: (v: any) => v is T): T
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
        return this.getCast(index, isNumberValue).value;
    }

    public getString(index: number)
    {
        return this.getCast(index, isStringValue).value;
    }

    public getBool(index: number)
    {
        return this.getCast(index, isBoolValue).value;
    }

    public sublist(index: number): ArgumentsValue
    {
        if (index === 0)
        {
            return this;
        }

        return new ArgumentsValue(this.value.slice(index));
    }
}