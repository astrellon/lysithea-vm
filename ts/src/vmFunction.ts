import { CodeLine } from "./virtualMachine";

type CodeLines = ReadonlyArray<CodeLine>;
type Parameters = ReadonlyArray<string>;
interface Labels
{
    readonly [label: string]: number;
}

export default class VMFunction
{
    public static readonly Empty = new VMFunction([], [], {});

    public readonly code: CodeLines;
    public readonly parameters: Parameters;
    public readonly labels: Labels;
    public name: string = 'anonymous';

    public get isEmpty()
    {
        return this.code.length == 0;
    }

    constructor (code: CodeLines, parameters: Parameters, labels: Labels)
    {
        this.code = code;
        this.parameters = parameters;
        this.labels = labels;
    }
}