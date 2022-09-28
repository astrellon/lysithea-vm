import { valueCompareTo } from "../types";
import VirtualMachine from "../virtualMachine";

export const comparisonHandleName = 'comp';

export function addComparisonHandler(vm: VirtualMachine)
{
    vm.addRunHandler(comparisonHandleName, comparisonHandler)
}

export function comparisonHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case '>':
        case 'greater':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) > 0);
                break;
            }
        case '>=':
        case 'greaterEquals':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) >= 0);
                break;
            }
        case '==':
        case 'equals':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) == 0);
                break;
            }
        case '!=':
        case 'notEquals':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) != 0);
                break;
            }
        case '<':
        case 'less':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) < 0);
                break;
            }
        case '<=':
        case 'lessEquals':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) <= 0);
                break;
            }
        case '!':
        case 'not':
            {
                const top = vm.popStackBool();
                vm.pushStack(!top);
                break;
            }
    }
}