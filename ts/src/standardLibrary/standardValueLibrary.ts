import { isValueString, valueCompareTo, valueToString, valueTypeof } from "../types";
import VirtualMachine from "../virtualMachine";

export const valueHandleName = 'value';

export function addValueHandler(vm: VirtualMachine)
{
    vm.addRunHandler(valueHandleName, valueHandler)
}

export function valueHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'toString':
            {
                let top = vm.peekStack();
                if (isValueString(top))
                {
                    break;
                }

                top = vm.popStack();
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