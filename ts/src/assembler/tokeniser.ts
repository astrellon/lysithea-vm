import { ArrayValue } from "../values/arrayValue";
import { BoolValue } from "../values/boolValue";
import { IValue } from "../values/ivalues";
import { NullValue } from "../values/nullValue";
import { NumberValue } from "../values/numberValue";
import { ObjectValue } from "../values/objectValue";
import { StringValue } from "../values/stringValue";
import { VariableValue } from "../values/variableValue";
import { CodeLocation } from "../virtualMachine";

export class Tokeniser
{
    private _returnSymbol = '\0';
    private _inQuote = '\0';
    private _escaped = false;
    private _inComment = false;
    private _accumulator = '';
    private _lineNumber = 0;
    private _columnNumber = 0;
    private _startLineNumber = 0;
    private _startColumnNumber = 0;
    private _current = '';

    public get current() { return this._current; }
    public get lineNumber() { return this._lineNumber; }
    public get columnNumber() { return this._columnNumber; }

    private readonly input: string[];

    constructor(input: string[])
    {
        this.input = input;
    }

    public currentLocation(): CodeLocation
    {
        return {
            startLineNumber: this._startLineNumber,
            startColumnNumber: this._startColumnNumber,
            endLineNumber: this._lineNumber,
            endColumnNumber: this._columnNumber
        }
    }

    public createLocation(startLineNumber: number, startColumnNumber: number): CodeLocation
    {
        return {
            startLineNumber, startColumnNumber,
            endLineNumber: this._lineNumber,
            endColumnNumber: this._columnNumber
        }
    }

    public moveNext(): boolean
    {
        if (this._returnSymbol !== '\0')
        {
            this._current = this._returnSymbol;
            this._returnSymbol = '\0';
            return true;
        }

        while (this._lineNumber < this.input.length)
        {
            const line = this.input[this._lineNumber];
            if (line.length === 0)
            {
                this._lineNumber++;
                continue;
            }

            const ch = line[this._columnNumber++];
            const atEndOfLine = this._columnNumber >= line.length;
            if (atEndOfLine)
            {
                this._columnNumber = 0;
                this._lineNumber++;
            }

            if (this._inComment)
            {
                if (atEndOfLine)
                {
                    this._inComment = false;
                }
                continue;
            }

            if (this._inQuote !== '\0')
            {
                if (this._escaped)
                {
                    switch (ch)
                    {
                        case '"':
                        case '\'':
                        case '\\':
                        {
                            this.appendChar(ch);
                            break;
                        }
                        case 't':
                        {
                            this.appendChar('\t');
                            break;
                        }
                        case 'r':
                        {
                            this.appendChar('\r');
                            break;
                        }
                        case 'n':
                        {
                            this.appendChar('\n');
                            break;
                        }
                    }

                    this._escaped = false;
                    continue;
                }
                else if (ch === '\\')
                {
                    this._escaped = true;
                    continue;
                }

                this.appendChar(ch);
                if (atEndOfLine)
                {
                    this.appendChar('\n');
                }

                if (ch === this._inQuote)
                {
                    this._current = this._accumulator;
                    this._accumulator = '';
                    this._inQuote = '\0';
                    return true;
                }
            }
            else
            {
                switch (ch)
                {
                    case ';':
                    {
                        this._inComment = true;
                        break;
                    }

                    case '"':
                    case '\'':
                    {
                        this._inQuote = ch;
                        this.appendChar(ch);
                        break;
                    }

                    case '(': case ')':
                    case '[': case ']':
                    case '{': case '}':
                    {
                        if (this._accumulator.length > 0)
                        {
                            this._returnSymbol = ch;
                            this._current = this._accumulator;
                            this._accumulator = '';
                        }
                        else
                        {
                            this._current = ch;
                        }
                        return true;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    {
                        if (this._accumulator.length > 0)
                        {
                            this._current = this._accumulator;
                            this._accumulator = '';
                            return true;
                        }
                        break;
                    }

                    default:
                    {
                        this.appendChar(ch);
                        break;
                    }
                }
            }
        }

        return false;
    }

    private appendChar(ch: string)
    {
        if (this._accumulator.length === 0)
        {
            this._startLineNumber = this._lineNumber;
            this._startColumnNumber = this._columnNumber - 1;
        }
        this._accumulator += ch;
    }

}

// export function tokenize(input: string)
// {
//     let inQuote = "\0";
//     let escaped = false;
//     let inComment = false;
//     let accumulator = "";
//     let index = 0;
//     const result: string[] = [];

//     while (index < input.length)
//     {
//         const ch = input.charAt(index++);
//         if (inComment)
//         {
//             if (ch === '\n' || ch === '\r')
//             {
//                 inComment = false;
//             }
//             continue;
//         }

//         if (inQuote != '\0')
//         {
//             if (escaped)
//             {
//                 switch (ch)
//                 {
//                     case '"':
//                     case '\'':
//                     case '\\':
//                         {
//                             accumulator += ch;
//                             break;
//                         }
//                     case 't':
//                         {
//                             accumulator += '\t';
//                             break;
//                         }
//                     case 'r':
//                         {
//                             accumulator += '\r';
//                             break;
//                         }
//                     case 'n':
//                         {
//                             accumulator += '\n';
//                             break;
//                         }
//                 }
//                 escaped = false;
//                 continue;
//             }
//             else if (ch == '\\')
//             {
//                 escaped = true;
//                 continue;
//             }

//             accumulator += ch;
//             if (ch == inQuote)
//             {
//                 result.push(accumulator);
//                 accumulator = "";
//                 inQuote = '\0';
//             }
//         }
//         else
//         {
//             switch (ch)
//             {
//                 case ';':
//                     {
//                         inComment = true;
//                         break;
//                     }

//                 case '"':
//                 case '\'':
//                     {
//                         inQuote = ch;
//                         accumulator += ch;
//                         break;
//                     }

//                 case '(':
//                 case ')':
//                 case '{':
//                 case '}':
//                     {
//                         if (accumulator.length > 0)
//                         {
//                             result.push(accumulator);
//                             accumulator = "";
//                         }
//                         result.push(ch);
//                         break;
//                     }

//                 case ' ':
//                 case '\t':
//                 case '\n':
//                 case '\r':
//                     {
//                         if (accumulator.length > 0)
//                         {
//                             result.push(accumulator);
//                             accumulator = "";
//                         }
//                         break;
//                     }
//                 default:
//                     {
//                         accumulator += ch;
//                         break;
//                     }
//             }
//         }
//     }

//     return result;
// }

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