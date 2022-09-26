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
export function valueToString(value: Value)
{
    if (value == null)
    {
        return '<<unknown>>';
    }

    const type = typeof(value);
    switch (type)
    {
        case 'number':
        case 'string':
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