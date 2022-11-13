import { CodeLine } from "./virtualMachine";

type CodeLines = ReadonlyArray<CodeLine>;
type Parameters = ReadonlyArray<string>;
interface Labels
{
    readonly [label: string]: number;
}

export default class VMFunction
{
    public static readonly Empty = new VMFunction([], [], {}, '');

    public readonly code: CodeLines;
    public readonly parameters: Parameters;
    public readonly labels: Labels;
    public readonly name: string;
    public readonly hasName: boolean;

    public get isEmpty()
    {
        return this.code.length == 0;
    }

    constructor (code: CodeLines, parameters: Parameters, labels: Labels, name: string)
    {
        this.code = code;
        this.parameters = parameters;
        this.labels = labels;
        this.name = name.length > 0 ? name : 'anonymous';
        this.hasName = name.length > 0;
    }
}