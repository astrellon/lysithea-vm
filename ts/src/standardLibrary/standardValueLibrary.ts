import { valueCompareTo, valueToString, valueTypeof } from "../types";
import VirtualMachine from "../virtualMachine";

export default class StandardValueLibrary
{
    public static readonly HandleName = "value";

    public static AddHandler(vm: VirtualMachine)
    {
        vm.addRunHandler(StandardValueLibrary.HandleName, StandardValueLibrary.Handler)
    }

    public static Handler(command: string, vm: VirtualMachine)
    {
        switch (command)
        {
            case "toString":
                {
                    const top = vm.popStack();
                    vm.pushStack(valueToString(top));
                    break;
                }
            case "typeof":
                {
                    const top = vm.popStack();
                    vm.pushStack(valueTypeof(top));
                    break;
                }
            case "compareTo":
                {
                    const right = vm.popStack();
                    const left = vm.popStack();
                    vm.pushStack(valueCompareTo(left, right));
                    break;
                }
        }
    }
}