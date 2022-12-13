import { CodeLocation } from "../virtualMachine";
import { IValue } from "../values/ivalues";

export interface IToken
{
    readonly location: CodeLocation;

    readonly getValue: () => IValue;
    readonly getValueCanBeEmpty: () => IValue | undefined;

    readonly keepLocation: (value: IValue | undefined) => Token;
    readonly toEmpty: () => Token;
}

export class Token implements IToken
{
    public readonly location: CodeLocation;
    public readonly value: IValue | undefined;

    constructor (location: CodeLocation, value: IValue | undefined)
    {
        this.location = location;
        this.value = value;
    }

    public getValue()
    {
        if (this.value === undefined)
        {
            throw new Error('Cannot get value of empty token');
        }

        return this.value;
    }

    public getValueCanBeEmpty()
    {
        return this.value;
    }

    public keepLocation(value: IValue | undefined)
    {
        return new Token(this.location, value);
    }

    public toEmpty()
    {
        return new Token(this.location, undefined);
    }
}

export class TokenList implements IToken
{
    public readonly location: CodeLocation;
    public readonly data: ReadonlyArray<IToken>;

    constructor (location: CodeLocation, data: ReadonlyArray<IToken>)
    {
        this.location = location;
        this.data = data;
    }
}