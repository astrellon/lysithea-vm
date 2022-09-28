import { Value, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const stringHandleName = 'string';

export function addStringHandler(vm: VirtualMachine)
{
    vm.addRunHandler(stringHandleName, stringHandler)
}

export function stringHandler(command: string, vm: VirtualMachine)
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
                const top = vm.popStackString();
                vm.pushStack(top.length);
                break;
            }
        case 'get':
            {
                const index = vm.popStackNumber();
                const top = vm.popStackString();
                vm.pushStack(top[index]);
                break;
            }
        case 'set':
            {
                const value = vm.popStack();
                const index = vm.popStackNumber();
                const top = vm.popStackString();
                vm.pushStack(set(top, index, valueToString(value)));
                break;
            }
        case 'insert':
            {
                const value = vm.popStack();
                const index = vm.popStackNumber();
                const top = vm.popStackString();
                vm.pushStack(insert(top, index, valueToString(value)));
                break;
            }
        case 'substring':
            {
                const length = vm.popStackNumber();
                const index = vm.popStackNumber();
                const top = vm.popStackString();
                vm.pushStack(substring(top, index, length));
                break;
            }
        case 'removeAt':
            {
                const index = vm.popStackNumber();
                const top = vm.popStackString();
                vm.pushStack(removeAt(top, index));
                break;
            }
        case 'removeAll':
            {
                const values = vm.popStackString();
                const top = vm.popStackString();
                vm.pushStack(removeAll(top, values));
                break;
            }
    }
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
