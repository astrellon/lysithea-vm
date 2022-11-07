import virtualMachine from "../virtualMachine";
import VMFunction from "../vmFunction";
import ArrayValue from "./arrayValue";
import { CompareResult, IFunctionValue, IValue } from "./ivalues";

export default class FunctionValue implements IFunctionValue
{
    public readonly value: VMFunction;

    constructor(value: VMFunction)
    {
        this.value = value;
    }

    public typename() { return 'function' ;}
    public toString() { return 'function:' + this.value.name; }

    public compareTo(other: IValue): CompareResult
    {
        if ((this as any) === other) { return 0; }
        if (!(other instanceof FunctionValue)) { return 1; }

        return this.value === other.value ? 0 : 1;
    }

    public invoke(vm: virtualMachine, args: ArrayValue, pushToStackTrace: boolean)
    {
        vm.executeFunction(this.value, args, pushToStackTrace);
    }
}