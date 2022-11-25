import { IValue } from "./values/ivalues";
import { isNumberValue } from "./values/numberValue";

export interface ScopeData
{
    [key: string]: IValue;
}

export interface IReadOnlyScope
{
    readonly get: (key: string) => IValue | undefined;
    get values(): Readonly<ScopeData>;
}

export default class Scope implements IReadOnlyScope
{
    public static readonly Empty: IReadOnlyScope = new Scope(undefined);

    private readonly _values: ScopeData = {};
    private readonly _parent: Scope | undefined;

    public get values(): Readonly<ScopeData>
    {
        return this._values;
    }

    constructor(parent: Scope | undefined = undefined)
    {
        this._parent = parent;
    }

    public combineScope(input: IReadOnlyScope)
    {
        for (const prop in input.values)
        {
            this.define(prop, input.values[prop]);
        }
    }

    public define(key: string, value: IValue)
    {
        this._values[key] = value;
    }

    public set(key: string, value: IValue): boolean
    {
        if (this._values.hasOwnProperty(key))
        {
            this._values[key] = value;
            return true;
        }

        if (this._parent)
        {
            return this._parent.set(key, value);
        }

        return false;
    }

    public get(key: string): IValue | undefined
    {
        const result = this._values[key];
        if (result != null)
        {
            return result;
        }

        if (this._parent !== undefined)
        {
            return this._parent.get(key);
        }

        return undefined;
    }

    public getNumber(key: string): number | undefined
    {
        const result = this.get(key);
        if (isNumberValue(result))
        {
            return result.value;
        }

        return undefined;
    }
}