import { ArrayValue, ObjectValue, Value, valueToString } from "./types";

const tokenRegex = /[^\s"']+|"([^"]*)"|'([^']*)'"/g;
const commentRegex = /^\s*(;|\/\/).*$/gm;
const bracketRegex = /([\(\)\{\}])/g;

export function tokenize(input: string)
{
    const cleaned = input.replace(bracketRegex, " $& ")
        .replace(commentRegex, '')

    return [...cleaned.matchAll(tokenRegex)].map(c => c[0]);
}

export function readFromTokens(tokens: string[]): Value
{
    if (tokens.length === 0)
    {
        throw new Error('Unexpected end of tokens');
    }

    const token = popFront(tokens);
    if (token === '(')
    {
        const list: Value[] = [];
        while (tokens[0] !== ')')
        {
            list.push(readFromTokens(tokens));
        }
        return list as ArrayValue;
    }
    else if (token === ')')
    {
        throw new Error('Unexpected )');
    }
    else if (token === '{')
    {
        const map: { [key: string]: Value } = {};
        while (tokens[0] !== '}')
        {
            const key = valueToString(readFromTokens(tokens));
            const value = readFromTokens(tokens);
            map[key] = value;
        }
        return map as ObjectValue;
    }
    else if (token === '}')
    {
        throw new Error('Unexpected }');
    }
    else
    {
        return atom(token);
    }
}

function atom(input: string): Value
{
    const parsedNumber = parseFloat(input);
    if (!isNaN(parsedNumber))
    {
        return parsedNumber;
    }
    if (input === 'true')
    {
        return true;
    }
    if (input === 'false')
    {
        return false;
    }
    if (input === 'null')
    {
        return null;
    }

    const first = input[0];
    const last = input[input.length - 1];
    if ((first === '"' && last === '"') ||
        (first === "'" && last === "'"))
    {
        return input.substring(1, input.length - 2);
    }

    return { 'symbolValue': input };
}

function popFront<T>(input: T[])
{
    if (input.length === 0)
    {
        throw new Error('Unable to pop empty list');
    }

    const result = input[0];
    input.splice(0, 1);
    return result;
}