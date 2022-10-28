import VirtualMachine from "../virtualMachine";
import ArgumentsValue from "./argumentsValue";
import { CompareResult, IFunctionValue, IValue } from "./ivalues";

export type BuiltinVMFunction = (vm: VirtualMachine, args: ArgumentsValue) => void;

export default class BuiltinFunctionValue implements IFunctionValue
{
    public readonly value: BuiltinVMFunction;

    constructor (value: BuiltinVMFunction)
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

    public invoke(vm: VirtualMachine, args: ArgumentsValue, pushToStackTrace: boolean)
    {
        this.value(vm, args);
    }
}