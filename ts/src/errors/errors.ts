import { Token } from "../assembler/token";
import { CodeLocation, toStringCodeLocation } from "../virtualMachine";

export class ParserError extends Error
{
    public readonly location: CodeLocation;
    public readonly token: string;

    constructor (location: CodeLocation, token: string, message: string)
    {
        super(message);

        this.location = location;
        this.token = token;
    }
}

export class AssemblerError extends Error
{
    public readonly token: Token;

    constructor (token: Token, message: string)
    {
        super(`${toStringCodeLocation(token.location)}: ${token.toString()}: ${message}`);

        this.token = token;

    }
}

export class VirtualMachineError extends Error
{
    public readonly stackTrace: string[];

    constructor (stackTrace: string[], message: string)
    {
        super(message);
        this.stackTrace = stackTrace;
    }
}