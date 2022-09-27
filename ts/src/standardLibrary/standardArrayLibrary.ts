import { ArrayValue, Value, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const handleName = 'array';

export function addHandler(vm: VirtualMachine)
{
    vm.addRunHandler(handleName, handler)
}

export function handler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case "append":
            {
                const right = vm.popStack();
                const left = vm.popStackCast<ArrayValue>();
                vm.pushStack(append(left, right));
                break;
            }
        case "prepend":
            {
                const right = vm.popStack();
                const left = vm.popStackCast<ArrayValue>();
                vm.pushStack(prepend(left, right));
                break;
            }
        case "length":
            {
                const top = vm.popStackCast<ArrayValue>();
                vm.pushStack(top.length);
                break;
            }
        case "get":
            {
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<ArrayValue>();
                vm.pushStack(get(top, index));
                break;
            }
        case "set":
            {
                const value = vm.popStack();
                const index = vm.popStackCast<number>();
                const top = vm.popStackCast<ArrayValue>();
                vm.pushStack(set(top, index, value));
                break;
            }
    }
}

export function getIndex(target: ArrayValue, index: number)
{
    if (index < 0)
    {
        return target.length + index;
    }

    return index;
}

export function append(target: ArrayValue, input: Value)
{
    return [...target, input];
}

export function prepend(target: ArrayValue, input: Value)
{
    return [input, ...target];
}

export function set(target: ArrayValue, index: number, input: Value)
{
    index = getIndex(target, index);
    let result = [...target];
    result[index] = input;
    return result;
}

export function get(target: ArrayValue, index: number)
{
    index = getIndex(target, index);
    return target[index];
}

export function insert(target: ArrayValue, index: number, input: Value)
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 0, input);
    return result;
}

export function insertFlatten(target: ArrayValue, index: number, input: ArrayValue)
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 0, ...input);
    return result;
}

export function removeAt(target: ArrayValue, index: number)
{
    index = getIndex(target, index);
    const result = [...target];
    result.splice(index, 1);
    return result;
}

export function removeAll(target: ArrayValue, value: Value)
{
    // const result = target.filter((v) => )
}