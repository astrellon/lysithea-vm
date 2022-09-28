import { ArrayValue, Value, valueCompareTo, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const assertHandleName = 'assert';

export function addAssertHandler(vm: VirtualMachine)
{
    vm.addRunHandler(assertHandleName, assertHandler)
}

export function assertHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'true':
            {
                const top = vm.popStackBool();
                if (!top)
                {
                    vm.running = false;
                    console.error(vm.createStackTrace().join('\n'));
                    console.error('Assert expected true');
                }
                break;
            }
        case 'false':
            {
                const top = vm.popStackBool();
                if (top)
                {
                    vm.running = false;
                    console.error(vm.createStackTrace().join('\n'));
                    console.error('Assert expected false');
                }
                break;
            }
        case 'equals':
            {
                const toCompare = vm.popStack();
                const top = vm.peekStack();
                if (valueCompareTo(top, toCompare) != 0)
                {
                    vm.running = false;
                    console.error(vm.createStackTrace().join('\n'));
                    console.error(`Assert expected equals:\nExpected: ${valueToString(toCompare)}\nActual: ${valueToString(top)}`);
                }
                break;
            }
        case 'notEquals':
            {
                const toCompare = vm.popStack();
                const top = vm.peekStack();
                if (valueCompareTo(top, toCompare) == 0)
                {
                    vm.running = false;
                    console.error(vm.createStackTrace().join('\n'));
                    console.error(`Assert expected not equals:\nExpected: ${valueToString(toCompare)}\nActual: ${valueToString(top)}`);
                }
                break;
            }
    }
}