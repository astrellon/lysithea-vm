export type Operator = 'unknown' | 'push' | 'pop' | 'swap' | 'copy' | 'call' | 'return' | 'jump' | 'jumpTrue' | 'jumpFalse' | 'run';

export type Value = string | boolean | number | ArrayValue | ObjectValue | null;
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

export interface Scope
{
    readonly name: string;
    readonly code: ReadonlyArray<CodeLine>;
    readonly labels: { readonly [label: string]: number };
}

export interface ScopeFrame
{
    readonly lineNumber: number;
    readonly scope: Scope;
}

export function isValueString(value: Value): value is string
{
    return typeof(value) === 'string';
}
export function isValueBoolean(value: Value): value is boolean
{
    return typeof(value) === 'boolean';
}
export function isValueNumber(value: Value): value is number
{
    return typeof(value) === 'number';
}
export function isValueTrue(value: Value): value is true
{
    return value === true;
}
export function isValueFalse(value: Value): value is false
{
    return value === false;
}
export function isValueArray(value: Value): value is ArrayValue
{
    return Array.isArray(value);
}
export function isValueObject(value: Value): value is ObjectValue
{
    return typeof(value) === 'object' && !isValueArray(value);
}
export function isValueNull(value: Value)
{
    return value == null;
}
export function valueTypeof(value: Value)
{
    if (value == null)
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
    }

    if (isValueObject(value))
    {
        return "object";
    }
    if (isValueArray(value))
    {
        return "array";
    }
    return "unknown";
}
export function valueToString(value: Value)
{
    if (value == null)
    {
        return 'null';
    }

    const type = typeof(value);
    switch (type)
    {
        case 'string':
            return value as string;
        case 'number':
        case 'boolean': return value.toString();
        case 'object':
            {
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

    return '<<unknown>>';
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

    return 1;
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

    return 1;
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
        case 'null': return 0;
    }

    return 1;
}