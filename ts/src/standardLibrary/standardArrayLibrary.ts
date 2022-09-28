import { ArrayValue, Value, valueCompareTo } from "../types";
import VirtualMachine from "../virtualMachine";

export const arrayHandleName = 'array';

export function addArrayHandler(vm: VirtualMachine)
{
    vm.addRunHandler(arrayHandleName, arrayHandler)
}

export function arrayHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case "append":
            {
                const right = vm.popStack();
                const left = vm.popStackArray();
                vm.pushStack(append(left, right));
                break;
            }
        case "prepend":
            {
                const right = vm.popStack();
                const left = vm.popStackArray();
                vm.pushStack(prepend(left, right));
                break;
            }
        case "length":
            {
                const top = vm.popStackArray();
                vm.pushStack(top.length);
                break;
            }
        case "get":
            {
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(get(top, index));
                break;
            }
        case "set":
            {
                const value = vm.popStack();
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(set(top, index, value));
                break;
            }
        case "insert":
            {
                const value = vm.popStack();
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(insert(top, index, value));
                break;
            }
        case "insertFlatten":
            {
                const value = vm.popStackArray();
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(insertFlatten(top, index, value));
                break;
            }
        case "remove":
            {
                const value = vm.popStack();
                const top = vm.popStackArray();
                vm.pushStack(remove(top, value));
                break;
            }
        case "removeAt":
            {
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(removeAt(top, index));
                break;
            }
        case "removeAll":
            {
                const value = vm.popStack();
                const top = vm.popStackArray();
                vm.pushStack(removeAll(top, value));
                break;
            }
        case "contains":
            {
                const value = vm.popStack();
                const top = vm.popStackArray();
                vm.pushStack(contains(top, value));
                break;
            }
        case "indexOf":
            {
                const value = vm.popStack();
                const top = vm.popStackArray();
                vm.pushStack(indexOf(top, value));
                break;
            }
        case "sublist":
            {
                const length = vm.popStackNumber();
                const index = vm.popStackNumber();
                const top = vm.popStackArray();
                vm.pushStack(sublist(top, index, length));
                break;
            }
    }
}

export function getIndex(target: ArrayValue, index: number): number
{
    if (index < 0)
    {
        return target.length + index;
    }

    return index;
}

export function append(target: ArrayValue, input: Value): ArrayValue
{
    return [...target, input];
}

export function prepend(target: ArrayValue, input: Value): ArrayValue
{
    return [input, ...target];
}

export function set(target: ArrayValue, index: number, input: Value): ArrayValue
{
    index = getIndex(target, index);
    let result = [...target];
    result[index] = input;
    return result;
}

export function get(target: ArrayValue, index: number): Value
{
    index = getIndex(target, index);
    return target[index];
}

export function insert(target: ArrayValue, index: number, input: Value): ArrayValue
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 0, input);
    return result;
}

export function insertFlatten(target: ArrayValue, index: number, input: ArrayValue): ArrayValue
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 0, ...input);
    return result;
}

export function removeAt(target: ArrayValue, index: number): ArrayValue
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 1);
    return result;
}

export function remove(target: ArrayValue, value: Value): ArrayValue
{
    const index = indexOf(target, value);
    if (index < 0)
    {
        return target;
    }

    return removeAt(target, index);
}

export function removeAll(target: ArrayValue, value: Value): ArrayValue
{
    return target.filter((v) => valueCompareTo(v, value) != 0)
}

export function contains(target: ArrayValue, value: Value): boolean
{
    return indexOf(target, value) >= 0;
}

export function indexOf(target: ArrayValue, value: Value): number
{
    return target.findIndex(v => valueCompareTo(v, value) == 0);
}

export function sublist(target: ArrayValue, index: number, length: number): ArrayValue
{
    index = getIndex(target, index);
    return target.slice(index, index + length);
}