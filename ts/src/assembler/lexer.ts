import { createErrorLogAt, ParserError } from "../errors/errors";
import { Editable } from "../standardLibrary/standardObjectLibrary";
import { BoolValue } from "../values/boolValue";
import { IValue } from "../values/ivalues";
import { NullValue } from "../values/nullValue";
import { NumberValue } from "../values/numberValue";
import { StringValue } from "../values/stringValue";
import { VariableValue } from "../values/variableValue";
import { EmptyCodeLocation } from "../virtualMachine";
import { Token, TokenMap, TokenType } from "./token";
import { Tokeniser } from "./tokeniser";

export class Lexer
{
    public static readFromLines(sourceName: string, input: string[])
    {
        const tokeniser = new Tokeniser(input);
        const result: Token[] = [];

        while (tokeniser.moveNext())
        {
            result.push(Lexer.readFromTokeniser(sourceName, tokeniser));
        }

        return Token.list(EmptyCodeLocation, result);
    }

    public static readFromTokeniser(sourceName: string, tokeniser: Tokeniser)
    {
        const token = tokeniser.current;
        switch (token)
        {
            case '(':
            {
                return Lexer.parseList(sourceName, tokeniser, true, ')');
            }
            case '[':
            {
                return Lexer.parseList(sourceName, tokeniser, false, ']');
            }
            case '{':
            {
                return Lexer.parseMap(sourceName, tokeniser);
            }

            case ')':
            case ']':
            case '}':
            {
                throw this.makeError(sourceName, tokeniser, token, `Unexpected ${token}`);
            }

            default:
            {
                return Token.value(tokeniser.currentLocation(), Lexer.parseConstant(token));
            }
        }
    }

    public static parseList(sourceName: string, tokeniser: Tokeniser, isExpression: boolean, endToken: string)
    {
        const startLineNumber = tokeniser.lineNumber;
        const startColumnNumber = tokeniser.columnNumber;
        const list: Token[] = [];
        while (tokeniser.moveNext())
        {
            if (tokeniser.current === endToken)
            {
                break;
            }

            list.push(Lexer.readFromTokeniser(sourceName, tokeniser));
        }

        const location = tokeniser.createLocation(startLineNumber, startColumnNumber);
        const tokenType: TokenType = isExpression ? 'expression' : 'list';
        return new Token(location, tokenType, undefined, list);
    }

    public static parseMap(sourceName: string, tokeniser: Tokeniser)
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

            const key = Lexer.readFromTokeniser(sourceName, tokeniser).toString();
            tokeniser.moveNext();

            const value = Lexer.readFromTokeniser(sourceName, tokeniser);
            if (value.type === 'expression')
            {
                throw this.makeError(sourceName, tokeniser, '', 'Expression found in map literal');
            }
            map[key] = value;
        }

        return Token.map(tokeniser.createLocation(startLineNumber, startColumnNumber), map);
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

    private static makeError(sourceName: string, tokeniser: Tokeniser, atToken: string, message: string)
    {
        const location = tokeniser.currentLocation();
        const trace = createErrorLogAt(sourceName, location, tokeniser.input);
        throw new ParserError(location, atToken, trace, message);
    }
}