import VirtualMachine from "../virtualMachine";
import ArrayValue from "./arrayValue";
import { CompareResult, IFunctionValue, IValue } from "./ivalues";

export type BuiltinFunctionCallback = (vm: VirtualMachine, args: ArrayValue) => void;

export default class BuiltinFunctionValue implements IFunctionValue
{
    public readonly value: BuiltinFunctionCallback;

    constructor (value: BuiltinFunctionCallback)
    {
        this.value = value;
    }

    public typename() { return 'builtin-function'; }
    public toString() { return this.typename(); }

    public compareTo(other: IValue): CompareResult
    {
        if (!(other instanceof BuiltinFunctionValue)) { return 1; }

        return this.value === other.value ? 0 : 1;
    }

    public invoke(vm: VirtualMachine, args: ArrayValue, pushToStackTrace: boolean)
    {
        this.value(vm, args);
    }
}