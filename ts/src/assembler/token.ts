import { CodeLocation } from "../virtualMachine";
import { IValue } from "../values/ivalues";
import { ArrayValue, ObjectValue, ObjectValueMap } from "../index";
import { Editable } from "../standardLibrary/standardObjectLibrary";

export type TokenType = 'empty' | 'value' | 'expression' | 'list' | 'map';

export type TokenList = ReadonlyArray<Token>;
export interface TokenMap { readonly [key: string]: Token }

const EmptyTokenList: TokenList = [];
const EmptyTokenMap: TokenMap = {};

export class Token
{
    public readonly location: CodeLocation;
    public readonly value: IValue | undefined;
    public readonly type: TokenType;
    public readonly tokenList: TokenList;
    public readonly tokenMap: TokenMap;

    constructor (location: CodeLocation, type: TokenType, value: IValue | undefined = undefined, tokenList: TokenList = EmptyTokenList, tokenMap: TokenMap = EmptyTokenMap)
    {
        this.location = location;
        this.value = value;
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

    public getValue(): IValue
    {
        switch (this.type)
        {
            case 'empty':
            {
                throw new Error('Cannot get value of empty token');
            }
            case 'expression':
            {
                throw new Error('Cannot get value of expression');
            }
            case 'value':
            {
                if (this.value === undefined)
                {
                    throw new Error('Cannot get value of empty token');
                }
                return this.value;
            }
            case 'list':
            {
                return new ArrayValue(this.tokenList.map(t => t.getValue()));
            }
            case 'map':
            {
                const map: Editable<ObjectValueMap> = {};
                for (const prop in this.tokenMap)
                {
                    map[prop] = this.tokenMap[prop].getValue();
                }
                return new ObjectValue(map);
            }
        }
    }

    public getValueCanBeEmpty()
    {
        return this.value;
    }

    public keepLocation(value: IValue | undefined)
    {
        return Token.value(this.location, value);
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
            case 'value': return this.getValue().toString();
            case 'list': return '<TokenList>';
            case 'map': return '<TokenMap>';
        }
    }
}