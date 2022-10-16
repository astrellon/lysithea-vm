import Scope, { IReadOnlyScope } from "../scope";
import { ArrayValue, isValueArray, isValueNumber, ObjectValue, Value, valueCompareTo } from "../types";

export const arrayScope: IReadOnlyScope = createArrayScope();

export function createArrayScope()
{
    const result = new Scope();

    const arrayFunctions: ObjectValue =
    {
        join: (vm, numArgs) =>
        {
            const args = vm.getArgs(numArgs);
            vm.pushStack(args);
        },

        length: (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(top.length);
        },

        get: (vm, numArgs) =>
        {
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(get(top, index));
        },

        set: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(set(top, index, value));

        },

        insert: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(insert(top, index, value));
        },

        insertFlatten: (vm, numArgs) =>
        {
            const value = vm.popStackCast(isValueArray);
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(insertFlatten(top, index, value));
        },

        remove: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(remove(top, value));
        },

        removeAt: (vm, numArgs) =>
        {
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(removeAt(top, index));
        },

        removeAll: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(removeAll(top, value));
        },

        contains: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(contains(top, value));
        },

        indexOf: (vm, numArgs) =>
        {
            const value = vm.popStack();
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(indexOf(top, value));
        },

        sublist: (vm, numArgs) =>
        {
            const length = vm.popStackCast(isValueNumber);
            const index = vm.popStackCast(isValueNumber);
            const top = vm.popStackCast(isValueArray);
            vm.pushStack(sublist(top, index, length));
        },
    }

    result.define('array', arrayFunctions);

    return result;
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