import { CodeLine, isValueArray, isValueString, Operator, Scope, Value } from "./types";

export type InputArrayValue = ReadonlyArray<InputDataArg>;
export interface InputObjectValue
{
    readonly [key: string]: InputDataArg;
}
export type InputDataArg = string | boolean | number | InputArrayValue | InputObjectValue;
export type InputDataLine = InputDataArg[];
export interface InputScope
{
    readonly name: string;
    readonly data: ReadonlyArray<InputDataLine>;
}

interface TempCodeLine
{
    readonly label?: string;
    readonly operator?: Operator;
    readonly value?: Value;
}

export function parseScopes(input: ReadonlyArray<InputScope>)
{
    return input.map(parseScope);
}

export function parseScope(input: InputScope)
{
    const tempCodeLines: TempCodeLine[] = [];
    for (let inputLine of input.data)
    {
        if (!isArray(inputLine))
        {
            inputLine = [inputLine];
        }

        for (const tempCodeLine of parseCodeLine(inputLine))
        {
            tempCodeLines.push(tempCodeLine);
        }
    }

    return processScope(input.name, tempCodeLines);
}

export function processScope(name: string, tempCode: ReadonlyArray<TempCodeLine>): Scope
{
    const labels: { [label: string]: number } = {}
    const code: CodeLine[] = [];

    for (const tempLine of tempCode)
    {
        if (tempLine.label != null)
        {
            labels[tempLine.label] = code.length;
        }
        else if (tempLine.operator != null)
        {
            code.push({ operator: tempLine.operator, value: tempLine.value });
        }
    }

    return { name, code, labels }
}

export function *parseCodeLine(input: InputDataLine): IterableIterator<TempCodeLine>
{
    if (input.length === 0)
    {
        return;
    }

    const first = input[0];
    if (typeof(first) === 'string' && first[0] === ':')
    {
        return yield { label: first }
    }

    let opCode = parseOperator(first);
    let codeLineInput: Value = null;
    let pushChildOffset = 1;
    if (opCode === 'unknown')
    {
        opCode = 'run';
        codeLineInput = parseRunCommand(first);
        if (codeLineInput == null)
        {
            throw new Error(`Error parsing run command: ${JSON.stringify(input)}`);
        }
        pushChildOffset = 0;
    }
    else if (input.length > 1)
    {
        const last = input[input.length - 1];
        if (isJumpCall(opCode))
        {
            codeLineInput = parseJumpLabel(last)
            if (codeLineInput == null)
            {
                throw new Error(`Error parsing ${opCode} input: ${JSON.stringify(input)}`);
            }
        }
        else
        {
            codeLineInput = parseValue(last);
            if (codeLineInput == null)
            {
                throw new Error(`Error parsing input for line: ${JSON.stringify(input)}`);
            }
        }
    }

    for (let i = 1; i < input.length - pushChildOffset; i++)
    {
        const parsedValue = parseValue(input[i]);
        if (parsedValue == null)
        {
            throw new Error(`Error parsing child for value`);
        }

        yield { operator: 'push', value: parsedValue }
    }

    yield { operator: opCode, value: codeLineInput }
}

function isJumpCall(input: Operator): boolean
{
    return input === 'call' || input === 'jump' ||
        input === 'jumpFalse' || input === 'jumpTrue';
}

function isArray(input: InputDataArg): input is InputArrayValue
{
    return Array.isArray(input);
}

function isObject(input: InputDataArg): input is InputObjectValue
{
    return typeof(input) === 'object' && !isArray(input);
}

function parseJumpLabel(input: InputDataArg)
{
    return parseTwoStringInput(input, ':', true);
}
function parseRunCommand(input: InputDataArg)
{
    return parseTwoStringInput(input, '.', false);
}

function parseTwoStringInput(input: InputDataArg, delimiter: string, includeDelimiter: boolean): Value
{
    if (typeof(input) === 'string')
    {
        const delimiterIndex = input.indexOf(delimiter);
        if (delimiterIndex > 0)
        {
            const array: Value[] = [input.substr(0, delimiterIndex)];
            if (includeDelimiter)
            {
                array.push(input.substring(delimiterIndex));
            }
            else
            {
                array.push(input.substring(delimiterIndex + 1));
            }

            return array;
        }

        return input;
    }
    else if (isArray(input))
    {
        if (input.length === 1)
        {
            return parseTwoStringInput(input[0], delimiter, includeDelimiter);
        }

        const parsed = parseValue(input);
        if (isValueArray(parsed))
        {
            if (parsed.length === 2 && isValueString(parsed[0]) && isValueString(parsed[1]))
            {
                return parsed;
            }
        }
    }

    return null;
}

function parseValue(input: InputDataArg) : Value
{
    if (input == null)
    {
        return null;
    }

    const type = typeof(input)
    if (type === 'number' || type === 'boolean' || type === 'string')
    {
        return input;
    }

    if (isArray(input))
    {
        const result: Value[] = [];
        for (const child of input)
        {
            const value = parseValue(child);
            if (value == null)
            {
                throw new Error(`Unable to parse value`);
            }
            result.push(value);
        }
        return result;
    }

    if (isObject(input))
    {
        const result: { [key: string]: Value } = {};
        for (const prop in input)
        {
            const value = parseValue(input[prop]);
            if (value == null)
            {
                throw new Error(`Unable to parse value`);
            }
            result[prop] = value;
        }
        return result;
    }

    return null;
}

function parseOperator(input: InputDataArg) : Operator
{
    if (typeof(input) !== 'string')
    {
        return 'unknown';
    }

    const lower = input.toLowerCase();
    switch (lower)
    {
        case 'push':
        case 'pop':
        case 'copy':
        case 'swap':
        case 'call':
        case 'return':
        case 'jump':
        case 'run': return lower as Operator;
        case 'jumptrue': return 'jumpTrue';
        case 'jumpfalse': return 'jumpFalse';
        default: return 'unknown';
    }
}