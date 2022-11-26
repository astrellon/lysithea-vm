import { CompareResult, IValue } from "./ivalues";

export class VariableValue implements IValue
{
    public readonly value: string;

    public get isLabel() { return this.value.length > 0 && this.value[0] === ':'; }

    constructor (value: string)
    {
        this.value = value;
    }

    public typename() { return 'variable'; }
    public toString() { return this.value; }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof VariableValue))
        {
            return 1;
        }

        const diff = this.value.localeCompare(other.value);
        if (diff == 0) { return 0; }
        if (diff < 0) { return -1; }
        return 1;
    }
}