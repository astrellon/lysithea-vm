import Scope from "./scope";
import VirtualMachine from "./virtualMachine";

export enum Operator
{
    Unknown,
    Push,
    Call, CallDirect, Return,
    GetProperty, Get, Set, Define,
    Jump, JumpTrue, JumpFalse
}

export type Editable<T> = {
    -readonly [P in keyof T]: T[P];
};

export type VariableValue = Symbol;
export type Value = string | boolean | number | ArrayValue | ObjectValue | FunctionValue | BuiltinFunctionValue | VariableValue | null;
export type ArrayValue = ReadonlyArray<Value>;
export interface ObjectValue
{
    readonly [key: string]: Value
}

export interface CodeLine
{
    readonly operator: Operator;
    readonly value?: Value;
}

export interface VMFunction
{
    name: string;
    readonly code: ReadonlyArray<CodeLine>;
    readonly parameters: ReadonlyArray<string>;
    readonly labels: { readonly [label: string]: number };
}
export const EmptyFunction: VMFunction =
{
    name: '',
    code: [],
    labels: {},
    parameters: []
}
export const EmptyArrayValue: ArrayValue = [];

export interface FunctionValue
{
    readonly funcValue: VMFunction;
}

export type BuiltinFunctionValue = (vm: VirtualMachine, numArgs: number) => void;

export interface ScopeFrame
{
    readonly lineNumber: number;
    readonly function: VMFunction;
    readonly scope: Scope;
}

export function isValueString(value: Value | undefined): value is string
{
    return typeof(value) === 'string';
}
export function isValueBoolean(value: Value | undefined): value is boolean
{
    return typeof(value) === 'boolean';
}
export function isValueNumber(value: Value | undefined): value is number
{
    return typeof(value) === 'number';
}
export function isValueTrue(value: Value | undefined): value is true
{
    return value === true;
}
export function isValueFalse(value: Value | undefined): value is false
{
    return value === false;
}
export function isValueArray(value: Value | undefined): value is ArrayValue
{
    return Array.isArray(value);
}
export function isValueObject(value: Value | undefined): value is ObjectValue
{
    return value != null && typeof(value) === 'object' &&
        !isValueArray(value) &&
        !isValueFunction(value);
}
export function isValueFunction(value: Value | undefined): value is FunctionValue
{
    return value != null && typeof(value) === 'object' && typeof((value as any)['funcValue']) === 'object';
}
export function isValueBuiltinFunction(value: Value | undefined): value is BuiltinFunctionValue
{
    return typeof(value) === 'function';
}
export function isValueAnyFunction(value: Value | undefined): value is FunctionValue | BuiltinFunctionValue
{
    return isValueBuiltinFunction(value) || isValueFunction(value);
}
export function isValueVariable(value: Value | undefined): value is VariableValue
{
    return typeof(value) === 'symbol';
}
export function isValueNull(value: Value | undefined)
{
    return value === null;
}
export function valueTypeof(value: Value | undefined)
{
    if (value === null)
    {
        return 'null';
    }
    const type = typeof(value);
    switch (type)
    {
        case 'string':
        case 'number':
            return type;
        case 'boolean':
            return 'bool';
        case 'symbol':
            return 'variable';
    }

    if (isValueObject(value))
    {
        return 'object';
    }
    if (isValueArray(value))
    {
        return 'array';
    }
    if (isValueFunction(value))
    {
        return 'function';
    }
    if (isValueBuiltinFunction(value))
    {
        return 'builtin-function';
    }
    return 'unknown';
}
export function valueToString(value?: Value): string
{
    if (value === null)
    {
        return 'null';
    }
    if (value === undefined)
    {
        return 'unknown';
    }

    switch (typeof(value))
    {
        case 'string': return value;

        case 'number':
        case 'boolean': return value.toString();

        case 'symbol': return value.description || '';

        case 'function': return 'builtin-function';

        case 'object':
            {
                if (isValueFunction(value))
                {
                    return `function:${value.funcValue.name}`;
                }

                let result = '';
                let first = true;
                if (isValueArray(value))
                {
                    result += '[';

                    for (const item of value)
                    {
                        if (!first)
                        {
                            result += ',';
                        }
                        first = false;
                        result += valueToString(item);
                    }

                    result += ']';
                    return result;
                }
                else if (isValueObject(value))
                {
                    result += '{';

                    for (const prop in value)
                    {
                        if (!first)
                        {
                            result += ',';
                        }
                        first = false;

                        result += `"${prop}":`;
                        result += valueToString(value[prop]);
                    }

                    result += '}';
                    return result;
                }
            }
    }

    return 'unknown';
}

export function numberCompareTo(left: number, right: number)
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
export function stringCompareTo(left: string, right: string)
{
    const diff = left.localeCompare(right);
    if (diff == 0)
    {
        return 0;
    }
    if (diff < 0)
    {
        return -1;
    }
    return 1;
}
export function boolCompareTo(left: boolean, right: boolean)
{
    if (left === right)
    {
        return 0;
    }
    return left ? -1 : 1;
}
export function arrayCompareTo(left: ArrayValue, right: ArrayValue)
{
    const compareLength = numberCompareTo(left.length, right.length);
    if (compareLength !== 0)
    {
        return compareLength;
    }

    for (let i = 0; i < left.length; i++)
    {
        const compare = valueCompareTo(left[i], right[i]);
        if (compare !== 0)
        {
            return compare;
        }
    }

    return 0;
}
export function objectLength(input: ObjectValue)
{
    return Object.keys(input).length;
}
export function objectCompareTo(left: ObjectValue, right: ObjectValue)
{
    const compareLength = numberCompareTo(objectLength(left), objectLength(right));
    if (compareLength !== 0)
    {
        return compareLength;
    }

    for (const prop in left)
    {
        const rightValue = right[prop];
        if (rightValue === undefined)
        {
            return 1;
        }
        const compare = valueCompareTo(left[prop], rightValue);
        if (compare !== 0)
        {
            return compare;
        }
    }

    return 0;
}

export function variableCompareTo(left: VariableValue, right: VariableValue): number
{
    return stringCompareTo(left.description || '', right.description || '');
}

export function valueCompareTo(left: Value, right: Value): number
{
    const leftType = valueTypeof(left);
    const rightType = valueTypeof(right);
    if (leftType !== rightType)
    {
        return 1;
    }

    switch (leftType)
    {
        case 'number':
            {
                return numberCompareTo(left as number, right as number);
            }
        case 'string':
            {
                return stringCompareTo(left as string, right as string);
            }
        case 'bool':
            {
                return boolCompareTo(left as boolean, right as boolean);
            }
        case 'array':
            {
                return arrayCompareTo(left as ArrayValue, right as ArrayValue);
            }
        case 'object':
            {
                return objectCompareTo(left as ObjectValue, right as ObjectValue);
            }
        case 'function':
            {
                return (left as FunctionValue).funcValue === (right as FunctionValue).funcValue ? 0 : 1;
            }
        case 'builtin-function':
            {
                return (left as BuiltinFunctionValue) === (right as BuiltinFunctionValue) ? 0 : 1;
            }
        case 'variable':
            {
                return variableCompareTo(left as VariableValue, right as VariableValue);
            }
        case 'null': return 0;
    }

    return 1;
}