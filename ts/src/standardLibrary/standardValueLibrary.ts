import { valueCompareTo, valueToString, valueTypeof } from "../types";
import VirtualMachine from "../virtualMachine";

export const handleName = 'value';

export function addHandler(vm: VirtualMachine)
{
    vm.addRunHandler(handleName, handler)
}

export function handler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'toString':
            {
                const top = vm.popStack();
                vm.pushStack(valueToString(top));
                break;
            }
        case 'typeof':
            {
                const top = vm.popStack();
                vm.pushStack(valueTypeof(top));
                break;
            }
        case 'compareTo':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right));
                break;
            }
    }
}