import { Token } from "../assembler/token";
import { CodeLocation, toStringCodeLocation } from "../virtualMachine";

export class ParserError extends Error
{
    public readonly location: CodeLocation;
    public readonly token: string;

    constructor (location: CodeLocation, token: string, message: string)
    {
        super(`${toStringCodeLocation(location)}: ${token.toString()}: ${message}`);

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

export function createErrorLogAt(sourceName: string, location: CodeLocation, fullText: ReadonlyArray<string>)
{
    let text = `${sourceName}:${location.startLineNumber + 1}:${location.startColumnNumber}\n`;

    const fromLineIndex = Math.max(0, location.startLineNumber - 1);
    const toLineIndex = Math.min(fullText.length, location.startLineNumber + 2);
    for (let i = fromLineIndex; i < toLineIndex; i++)
    {
        const lineNum = (i + 1).toString(10);
        text += `${lineNum}: ${fullText[i]}\n`;

        if (i === location.startLineNumber)
        {
            text += ' '.repeat(location.startColumnNumber + lineNum.length + 1) + '^';
            const diff = location.endColumnNumber - location.startColumnNumber;
            if (diff > 0)
            {
                text += '-'.repeat(diff - 1) + '^';
            }
            text += '\n';
        }
    }

    return text;
}