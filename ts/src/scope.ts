import { ArrayValue, isValueArray, isValueNumber, isValueObject, isValueString, Value, valueToString } from './types';

export interface IReadOnlyScope
{
    get: (key: Value) => Value | undefined;
    getKey: (key: string) => Value | undefined;
    getProperty: (key: ArrayValue) => Value | undefined;
    get values(): Readonly<ScopeData>;
}

export interface ScopeData
{
    [key: string]: Value;
}

export default class Scope implements IReadOnlyScope
{
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

    public define(key: string, value: Value)
    {
        this._values[key] = value;
    }

    public set(key: string, value: Value): boolean
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

    public get(key: Value): Value | undefined
    {
        if (isValueString(key))
        {
            return this.getKey(key);
        }
        if (isValueArray(key))
        {
            return this.getProperty(key);
        }

        return undefined;
    }

    public getKey(key: string): Value | undefined
    {
        const result = this._values[key];
        if (result != null)
        {
            return result;
        }

        if (this._parent !== undefined)
        {
            return this._parent.getKey(key);
        }

        return undefined;
    }

    public getProperty(key: ArrayValue): Value | undefined
    {
        let current = this.getKey(valueToString(key[0]));
        if (current === undefined)
        {
            return current;
        }

        for (let i = 1; i < key.length; i++)
        {
            if (isValueObject(current))
            {
                current = current[valueToString(key[i])];
            }
            else if (isValueArray(current))
            {
                const index = key[i];
                if (isValueNumber(index))
                {
                    current = current[index];
                }
                else if (isValueString(index))
                {
                    current = current[parseInt(index)];
                }
            }
            else
            {
                return undefined;
            }
        }

        return current;
    }
}