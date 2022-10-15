import { ArrayValue, isValueArray, isValueNumber, isValueObject, isValueString, Value, valueToString } from './types';

export interface IReadOnlyScope
{
    get: (key: Value) => Value;
    getKey: (key: string) => Value;
    getProperty: (key: ArrayValue) => Value;
    get values(): Readonly<ScopeData>;
}

export interface ScopeData
{
    [key: string]: Value;
}

export default class Scope implements IReadOnlyScope
{
    private readonly _values: ScopeData = {};
    private readonly _parent: Scope | null;

    public get values(): Readonly<ScopeData>
    {
        return this.values;
    }

    constructor(parent: Scope | null = null)
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

    public get(key: Value): Value
    {
        if (isValueString(key))
        {
            return this.getKey(key);
        }
        if (isValueArray(key))
        {
            return this.getProperty(key);
        }

        throw new Error('Unable to get using a value of ' + valueToString(key));
    }

    public getKey(key: string): Value
    {
        const result = this._values[key];
        if (result != null)
        {
            return result;
        }

        if (this._parent != null)
        {
            this._parent.getKey(key);
        }

        return null;
    }

    public getProperty(key: ArrayValue): Value
    {
        let current = this.getKey(key[0] as string);
        for (let i = 1; i < key.length; i++)
        {
            if (isValueObject(current))
            {
                current = current[key[i] as string];
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
                return null;
            }
        }

        return current;
    }
}