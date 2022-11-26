import { ArrayValue } from "./values/arrayValue";
import { BoolValue } from "./values/boolValue";
import { IValue } from "./values/ivalues";
import { NullValue } from "./values/nullValue";
import { NumberValue } from "./values/numberValue";
import { ObjectValue } from "./values/objectValue";
import { StringValue } from "./values/stringValue";
import { VariableValue } from "./values/variableValue";

export function tokenize(input: string)
{
    let inQuote = "\0";
    let escaped = false;
    let inComment = false;
    let accumulator = "";
    let index = 0;
    const result: string[] = [];

    while (index < input.length)
    {
        const ch = input.charAt(index++);
        if (inComment)
        {
            if (ch === '\n' || ch === '\r')
            {
                inComment = false;
            }
            continue;
        }

        if (inQuote != '\0')
        {
            if (escaped)
            {
                switch (ch)
                {
                    case '"':
                    case '\'':
                    case '\\':
                        {
                            accumulator += ch;
                            break;
                        }
                    case 't':
                        {
                            accumulator += '\t';
                            break;
                        }
                    case 'r':
                        {
                            accumulator += '\r';
                            break;
                        }
                    case 'n':
                        {
                            accumulator += '\n';
                            break;
                        }
                }
                escaped = false;
                continue;
            }
            else if (ch == '\\')
            {
                escaped = true;
                continue;
            }

            accumulator += ch;
            if (ch == inQuote)
            {
                result.push(accumulator);
                accumulator = "";
                inQuote = '\0';
            }
        }
        else
        {
            switch (ch)
            {
                case ';':
                    {
                        inComment = true;
                        break;
                    }

                case '"':
                case '\'':
                    {
                        inQuote = ch;
                        accumulator += ch;
                        break;
                    }

                case '(':
                case ')':
                case '{':
                case '}':
                    {
                        if (accumulator.length > 0)
                        {
                            result.push(accumulator);
                            accumulator = "";
                        }
                        result.push(ch);
                        break;
                    }

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    {
                        if (accumulator.length > 0)
                        {
                            result.push(accumulator);
                            accumulator = "";
                        }
                        break;
                    }
                default:
                    {
                        accumulator += ch;
                        break;
                    }
            }
        }
    }

    return result;
}

export function readAllTokens(tokens: string[]): ArrayValue
{
    const result: IValue[] = [];

    while (tokens.length > 0)
    {
        result.push(readFromTokens(tokens));
    }

    return new ArrayValue(result);
}

export function readFromTokens(tokens: string[]): IValue
{
    if (tokens.length === 0)
    {
        throw new Error('Unexpected end of tokens');
    }

    const token = popFront(tokens);
    if (token === '(')
    {
        const list: IValue[] = [];
        while (tokens[0] !== ')')
        {
            list.push(readFromTokens(tokens));
        }
        popFront(tokens);
        return new ArrayValue(list);
    }
    else if (token === ')')
    {
        throw new Error('Unexpected )');
    }
    else if (token === '{')
    {
        const map: { [key: string]: IValue } = {};
        while (tokens[0] !== '}')
        {
            const key = readFromTokens(tokens).toString();
            const value = readFromTokens(tokens);
            map[key] = value;
        }
        popFront(tokens);
        return new ObjectValue(map);
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

function atom(input: string): IValue
{
    const parsedNumber = parseFloat(input);
    if (!isNaN(parsedNumber))
    {
        return new NumberValue(parsedNumber);
    }
    if (input === 'true')
    {
        return BoolValue.True;
    }
    if (input === 'false')
    {
        return BoolValue.False;
    }
    if (input === 'null')
    {
        return NullValue.Value;
    }

    const first = input[0];
    const last = input[input.length - 1];
    if ((first === '"' && last === '"') ||
        (first === "'" && last === "'"))
    {
        return new StringValue(input.substring(1, input.length - 1));
    }

    return new VariableValue(input);
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