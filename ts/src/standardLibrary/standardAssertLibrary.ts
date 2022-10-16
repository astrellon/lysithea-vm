import Scope, { IReadOnlyScope } from "../scope";
import { isValueBoolean, ObjectValue, valueCompareTo, valueToString } from "../types";

export const assertScope: IReadOnlyScope = createAssertScope();

export function createAssertScope()
{
    const result: Scope = new Scope();

    const assertFunctions: ObjectValue =
    {
        true: (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueBoolean);
            if (!top)
            {
                vm.running = false;
                console.error(vm.createStackTrace().join('\n'));
                console.error('Assert expected true');
            }
        },

        false: (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueBoolean);
            if (top)
            {
                vm.running = false;
                console.error(vm.createStackTrace().join('\n'));
                console.error('Assert expected false');
            }
        },

        equals: (vm, numArgs) =>
        {
            const actual = vm.popStack();
            const expected = vm.popStack();
            if (valueCompareTo(expected, actual) != 0)
            {
                vm.running = false;
                console.error(vm.createStackTrace().join('\n'));
                console.error(`Assert expected equals:\nExpected: ${valueToString(expected)}\nActual: ${valueToString(actual)}`);
            }
        },

        notEquals: (vm, numArgs) =>
        {
            const actual = vm.popStack();
            const expected = vm.popStack();
            if (valueCompareTo(expected, actual) == 0)
            {
                vm.running = false;
                console.error(vm.createStackTrace().join('\n'));
                console.error(`Assert expected not equals:\nActual: ${valueToString(actual)}`);
            }
        }
    };

    result.define('assert', assertFunctions);

    return result;
}