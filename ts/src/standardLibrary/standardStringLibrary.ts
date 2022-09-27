import { Value, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const handleName = 'string';

export function addHandler(vm: VirtualMachine)
{
    vm.addRunHandler(handleName, handler)
}

export function handler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'append':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(append(left, right));
                break;
            }
        case 'prepend':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(prepend(left, right));
                break;
            }
        case 'length':
            {
                const top = vm.popStackCast<string>();
                vm.pushStack(top.length);
                break;
            }
        case 'get':
            {
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<string>();
                vm.pushStack(top[index]);
                break;
            }
        case 'set':
            {
                const value = vm.popStack();
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<string>();
                vm.pushStack(set(top, index, valueToString(value)));
                break;
            }
        case 'insert':
            {
                const value = vm.popStack();
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<string>();
                vm.pushStack(insert(top, index, valueToString(value)));
                break;
            }
        case 'substring':
            {
                const length = vm.popStackCast<number>();
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<string>();
                vm.pushStack(top.substring(index, index + length));
                break;
            }
        case 'removeAt':
            {
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<string>();
                vm.pushStack(removeAt(top, index));
                break;
            }
        case 'removeAll':
            {
                const values = vm.popStackCast<string>();
                const top = vm.popStackCast<string>();
                vm.pushStack(removeAll(top, values));
                break;
            }
    }
}

export function getIndex(input: string, index: number)
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
    return `${input.substring(0, index)}${input.substring(index + 1)}}`;
}

export function removeAll(input: string, values: string): string
{
    return input.replace(values, '');
}
