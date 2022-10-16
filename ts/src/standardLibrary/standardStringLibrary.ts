import Scope, { IReadOnlyScope } from "../scope";
import { isValueNumber, isValueString, ObjectValue, Value, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const stringScope: IReadOnlyScope = createStringScope();

export function createStringScope()
{
    const result = new Scope();

    const stringFunctions: ObjectValue =
    {
        'join': (vm, numArgs) =>
        {
            if (numArgs < 2)
            {
                throw new Error('Not enough arguments for string join');
            }
            const args = vm.getArgs(numArgs);
            const separator = args[0];
            const result = args.slice(1).join(valueToString(separator));
            vm.pushStack(result);
        },

        'length': (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueString);
            vm.pushStack(top.length);
        },

        'get': (vm, numArgs) =>
        {
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(top[index]);
        },

        'set': (vm, numArgs) =>
        {
            const value = vm.popStack();
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(set(top, index, valueToString(value)));
        },

        'insert': (vm, numArgs) =>
        {
            const value = vm.popStack();
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(insert(top, index, valueToString(value)));
        },

        'substring': (vm, numArgs) =>
        {
            const length = vm.popStackCast(isValueNumber);
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(substring(top, index, length));
        },

        'removeAt': (vm, numArgs) =>
        {
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(removeAt(top, index));
        },

        'removeAll': (vm, numArgs) =>
        {
            const values = vm.popStackCast(isValueString);
            const top = vm.popStackCast(isValueString);
            vm.pushStack(removeAll(top, values));
        }
    }

    result.define('string', stringFunctions);

    return result;
}

export function getIndex(input: string, index: number): number
{
    if (index < 0)
    {
        return input.length + index;
    }

    return index;
}

export function append(left: Value, right: Value): string
{
    return valueToString(left) + valueToString(right);
}

export function prepend(left: Value, right: Value): string
{
    return valueToString(right) + valueToString(left);
}

export function set(input: string, index: number, value: string): string
{
    index = getIndex(input, index);
    return `${input.substring(0, index)}${value}${input.substring(index + 1)}`;
}

export function insert(input: string, index: number, value: string): string
{
    index = getIndex(input, index);
    return `${input.substring(0, index)}${value}${input.substring(index)}`;
}

export function removeAt(input: string, index: number): string
{
    index = getIndex(input, index);
    return `${input.substring(0, index)}${input.substring(index + 1)}`;
}

export function removeAll(input: string, values: string): string
{
    return input.replace(values, '');
}

export function substring(input: string, index: number, length: number)
{
    index = getIndex(input, index);
    return input.substring(index, index + length);
}
