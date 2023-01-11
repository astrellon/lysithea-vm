import { CodeLocation } from "../virtualMachine";
import { IValue } from "../values/ivalues";
import { Assembler, NullValue, StringValue } from "../index";
import { AssemblerError } from "../errors/errors";

export type TokenType = 'empty' | 'value' | 'expression' | 'list' | 'map';

export type TokenList = ReadonlyArray<Token>;
export interface TokenMap { readonly [key: string]: Token }

const EmptyTokenList: TokenList = [];
const EmptyTokenMap: TokenMap = {};

export class Token
{
    public readonly location: CodeLocation;
    public readonly value: IValue;
    public readonly type: TokenType;
    public readonly tokenList: TokenList;
    public readonly tokenMap: TokenMap;

    constructor (location: CodeLocation, type: TokenType, value: IValue | undefined = undefined, tokenList: TokenList = EmptyTokenList, tokenMap: TokenMap = EmptyTokenMap)
    {
        this.location = location;
        this.value = value ?? NullValue.Value;
        this.type = type;
        this.tokenList = tokenList;
        this.tokenMap = tokenMap;
    }

    public static empty(location: CodeLocation)
    {
        return new Token(location, 'empty');
    }

    public static value(location: CodeLocation, value: IValue | undefined)
    {
        return new Token(location, 'value', value);
    }

    public static expression(location: CodeLocation, list: TokenList)
    {
        return new Token(location, 'expression', undefined, list);
    }

    public static list(location: CodeLocation, list: TokenList)
    {
        return new Token(location, 'list', undefined, list);
    }

    public static map(location: CodeLocation, map: TokenMap)
    {
        return new Token(location, 'map', undefined, EmptyTokenList, map);
    }

    public getValue(assembler: Assembler): IValue
    {
        if (this.type === 'value')
        {
            return this.value;
        }

        throw assembler.makeError(this,  `Cannot get value of ${this.type} token`);
    }

    public getValueCanBeEmpty(assembler: Assembler) : IValue | undefined
    {
        return this.type === 'empty' ? undefined : this.getValue(assembler);
    }

    public keepLocation(value: string | IValue | undefined)
    {
        if (typeof (value) === 'string')
        {
            return Token.value(this.location, new StringValue(value));
        }
        return Token.value(this.location, value);
    }

    public isNestedExpression()
    {
        return this.tokenList.length > 0 && this.tokenList.every(t => t.type === 'expression');
    }

    public toEmpty()
    {
        return Token.empty(this.location);
    }

    public toString()
    {
        switch (this.type)
        {
            default:
            case 'empty': return '<empty>';
            case 'expression': return '<TokenExpression>';
            case 'value': return this.value.toString();
            case 'list': return '<TokenList>';
            case 'map': return '<TokenMap>';
        }
    }
}