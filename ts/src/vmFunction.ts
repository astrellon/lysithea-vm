import { CodeLine, CodeLocation, EmptyCodeLocation } from "./virtualMachine";

type CodeLines = ReadonlyArray<CodeLine>;
type Parameters = ReadonlyArray<string>;
interface Labels
{
    readonly [label: string]: number;
}

export class DebugSymbols
{
    public static readonly empty: DebugSymbols = new DebugSymbols('empty', [], []);

    public readonly sourceName: string;
    public readonly fullText: string[];
    public readonly codeLineToText: CodeLocation[];

    constructor(sourceName: string, fullText: string[], codeLineToText: CodeLocation[])
    {
        this.sourceName = sourceName;
        this.fullText = fullText;
        this.codeLineToText = codeLineToText;
    }

    public getLocation(line: number)
    {
        if (line >= 0 && line < this.codeLineToText.length)
        {
            return this.codeLineToText[line];
        }

        return EmptyCodeLocation;
    }
}

export class VMFunction
{
    public static readonly Empty = new VMFunction([], [], {}, '', DebugSymbols.empty);

    public readonly code: CodeLines;
    public readonly parameters: Parameters;
    public readonly labels: Labels;
    public readonly name: string;
    public readonly debugSymbols: DebugSymbols;
    public readonly hasName: boolean;

    public get isEmpty()
    {
        return this.code.length == 0;
    }

    constructor (code: CodeLines, parameters: Parameters, labels: Labels, name: string, debugSymbols: DebugSymbols)
    {
        this.code = code;
        this.parameters = parameters;
        this.labels = labels;
        this.name = name.length > 0 ? name : 'anonymous';
        this.debugSymbols = debugSymbols;

        this.hasName = name.length > 0;
    }
}