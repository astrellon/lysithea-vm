import Scope, { IReadOnlyScope } from "../scope";
import { ArrayValue, isValueObject, isValueString, ObjectValue, Value } from "../types";

export const objectScope: IReadOnlyScope = createObjectScope();

export function createObjectScope()
{
    const result = new Scope();

    const objectFunctions: ObjectValue =
    {
        'set': (vm, numArgs) =>
        {
            const value = vm.popStack();
            const key = vm.popStackCast(isValueString);
            const top = vm.popStackCast(isValueObject);
            vm.pushStack(set(top, key, value));
        },

        'get': (vm, numArgs) =>
        {
            const key = vm.popStackCast(isValueString);
            const top = vm.popStackCast(isValueObject);
            vm.pushStack(get(top, key));
        },

        'keys': (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueObject);
            vm.pushStack(keys(top));
        },

        'values': (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueObject);
            vm.pushStack(values(top));
        },

        'length': (vm, numArgs) =>
        {
            const top = vm.popStackCast(isValueObject);
            vm.pushStack(length(top));
        }
    }

    result.define('object', objectFunctions);

    return result;
}

export function set(target: ObjectValue, key: string, value: Value)
{
    return { ...target, [key]: value };
}

export function get(target: ObjectValue, key: string)
{
    const result = target[key];
    return result != null ? result : null;
}

export function keys(target: ObjectValue): ArrayValue
{
    return Object.keys(target);
}

export function values(target: ObjectValue): ArrayValue
{
    return Object.values(target);
}

export function length(target: ObjectValue)
{
    return Object.keys(target).length;
}