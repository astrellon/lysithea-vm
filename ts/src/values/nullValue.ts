import { CompareResult, IValue } from "./ivalues";

export default class NullValue implements IValue
{
    public static readonly Value = new NullValue();

    public typename() { return 'null'; }
    public toString() { return 'null'; }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof NullValue))
        {
            return 1;
        }

        return 0;
    }
}