import Scope, { IReadOnlyScope } from "../scope";
import { isValueNumber, isValueString, valueCompareTo, valueToString } from "../types";

export const operatorScope: IReadOnlyScope = createOperatorScope();

export function createOperatorScope()
{
    const result = new Scope();

    result.define('>', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) > 0);
    });

    result.define('>=', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) >= 0);
    });

    result.define('==', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) === 0);
    });

    result.define('!=', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) !== 0);
    });

    result.define('<', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) < 0);
    });

    result.define('<=', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right) <= 0);
    });

    result.define('+', (vm, numArgs) =>
    {
        if (numArgs === 0)
        {
            return;
        }

        const args = vm.getArgs(numArgs);
        if (isValueString(args[0]))
        {
            const result = args.map(valueToString).join('');
            vm.pushStack(result);
        }
        else if (isValueNumber(args[0]))
        {
            let result = 0;
            for (let i = 0; i < args.length; i++)
            {
                const item = args[i];
                if (isValueNumber(item))
                {
                    result += item;
                }
                else
                {
                    throw new Error('Add only works on numbers and strings');
                }
            }
            vm.pushStack(result);
        }
    });

    result.define('-', (vm, numArgs) =>
    {
        const right = vm.popStackCast(isValueNumber);
        const left = vm.popStackCast(isValueNumber);
        vm.pushStack(left - right);
    });

    result.define('*', (vm, numArgs) =>
    {
        const right = vm.popStackCast(isValueNumber);
        const left = vm.popStackCast(isValueNumber);
        vm.pushStack(left * right);
    });

    result.define('/', (vm, numArgs) =>
    {
        const right = vm.popStackCast(isValueNumber);
        const left = vm.popStackCast(isValueNumber);
        vm.pushStack(left / right);
    });

    result.define('%', (vm, numArgs) =>
    {
        const right = vm.popStackCast(isValueNumber);
        const left = vm.popStackCast(isValueNumber);
        vm.pushStack(left % right);
    });

    return result;
}