import { BuiltinFunctionValue, BuiltinFunctionCallback } from "./values/builtinFunctionValue";
import { IValue } from "./values/ivalues";
import { isNumberValue } from "./values/numberValue";

export interface ScopeData
{
    [key: string]: IValue;
}

interface ConstantMap
{
    [key: string]: boolean;
}

export interface IReadOnlyScope
{
    readonly get: (key: string) => IValue | undefined;
    readonly isConstant: (key: string) => boolean;

    get parent(): Scope | undefined;
    get values(): Readonly<ScopeData>;
    get constants(): Readonly<ConstantMap>;
}

export class Scope implements IReadOnlyScope
{
    public static readonly Empty: IReadOnlyScope = new Scope(undefined);
    private static readonly EmptyConstants: Readonly<ConstantMap> = {};

    private readonly _values: ScopeData = {};
    private readonly _parent: Scope | undefined;
    private _constants: ConstantMap | undefined;

    public get values(): Readonly<ScopeData>
    {
        return this._values;
    }

    public get constants(): Readonly<ConstantMap>
    {
        return this._constants ?? Scope.EmptyConstants;
    }

    public get parent(): Scope | undefined
    {
        return this._parent;
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

        for (const prop in input.constants)
        {
            this.setConstant(prop);
        }
    }

    public constant(key: string, value: IValue)
    {
        if (this.get(key) !== undefined)
        {
            return false;
        }

        this.define(key, value);
        this.setConstant(key);
        return true;
    }

    public constantFunc(key: string, value: BuiltinFunctionCallback, name: string | null = null)
    {
        this.constant(key, new BuiltinFunctionValue(value, name ?? key));
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

    public setConstant(key: string)
    {
        if (this._constants === undefined)
        {
            this._constants = {};
        }

        this._constants[key] = true;
    }

    public isConstant(key: string): boolean
    {
        return !!this._constants && this._constants[key] === true;
    }
}