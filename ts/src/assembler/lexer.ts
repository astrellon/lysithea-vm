import { ParserError } from "../errors/errors";
import { Editable } from "../standardLibrary/standardObjectLibrary";
import { BoolValue } from "../values/boolValue";
import { IValue } from "../values/ivalues";
import { NullValue } from "../values/nullValue";
import { NumberValue } from "../values/numberValue";
import { StringValue } from "../values/stringValue";
import { VariableValue } from "../values/variableValue";
import { EmptyCodeLocation } from "../virtualMachine";
import { Token, TokenMap } from "./token";
import { Tokeniser } from "./tokeniser";

export class Lexer
{
    public static readFromLines(input: string[])
    {
        const tokeniser = new Tokeniser(input);
        const result: Token[] = [];

        while (tokeniser.moveNext())
        {
            result.push(Lexer.readFromTokeniser(tokeniser));
        }

        return Token.list(EmptyCodeLocation, result);
    }

    public static readFromTokeniser(tokeniser: Tokeniser)
    {
        const token = tokeniser.current;
        switch (token)
        {
            case '(':
            {
                const startLineNumber = tokeniser.lineNumber;
                const startColumnNumber = tokeniser.columnNumber;
                const list: Token[] = [];
                while (tokeniser.moveNext())
                {
                    if (tokeniser.current === ')')
                    {
                        break;
                    }

                    list.push(Lexer.readFromTokeniser(tokeniser));
                }

                return Token.expression(tokeniser.createLocation(startLineNumber, startColumnNumber), list);
            }
            case ')':
            {
                throw new ParserError(tokeniser.currentLocation(), token, 'Unexpected )');
            }
            case '[':
            {
                const startLineNumber = tokeniser.lineNumber;
                const startColumnNumber = tokeniser.columnNumber;
                const list: Token[] = [];
                while (tokeniser.moveNext())
                {
                    if (tokeniser.current === ']')
                    {
                        break;
                    }

                    const value = Lexer.readFromTokeniser(tokeniser);
                    if (value.type === 'expression')
                    {
                        throw new ParserError(value.location, '', 'Expression found in array literal');
                    }
                    list.push(Lexer.readFromTokeniser(tokeniser));
                }

                return Token.list(tokeniser.createLocation(startLineNumber, startColumnNumber), list);
            }
            case ']':
            {
                throw new ParserError(tokeniser.currentLocation(), token, 'Unexpected )');
            }
            case '{':
            {
                const startLineNumber = tokeniser.lineNumber;
                const startColumnNumber = tokeniser.columnNumber;
                const map: Editable<TokenMap> = {};
                while (tokeniser.moveNext())
                {
                    if (tokeniser.current === '}')
                    {
                        break;
                    }

                    const key = Lexer.readFromTokeniser(tokeniser).toString();
                    tokeniser.moveNext();

                    const value = Lexer.readFromTokeniser(tokeniser);
                    if (value.type === 'expression')
                    {
                        throw new ParserError(value.location, '', 'Expression found in map literal');
                    }
                    map[key] = value;
                }

                return Token.map(tokeniser.createLocation(startLineNumber, startColumnNumber), map);
            }
            case '}':
            {
                throw new ParserError(tokeniser.currentLocation(), token, 'Unexpected }');
            }
            default:
            {
                return Token.value(tokeniser.currentLocation(), Lexer.parseConstant(token));
            }
        }
    }

    public static parseConstant(input: string): IValue
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
}