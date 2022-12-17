import { CodeLocation } from "../virtualMachine";

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