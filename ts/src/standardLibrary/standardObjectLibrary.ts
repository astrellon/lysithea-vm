import { ArrayValue, ObjectValue, Value } from "../types";
import VirtualMachine from "../virtualMachine";

export const objectHandleName = 'object';

export function addObjectHandler(vm: VirtualMachine)
{
    vm.addRunHandler(objectHandleName, objectHandler)
}

export function objectHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'set':
            {
                const value = vm.popStack();
                const key = vm.popStackString();
                const top = vm.popStackObject();
                vm.pushStack(set(top, key, value));
                break;
            }
        case 'get':
            {
                const key = vm.popStackString();
                const top = vm.popStackObject();
                vm.pushStack(get(top, key));
                break;
            }
        case 'keys':
            {
                const top = vm.popStackObject();
                vm.pushStack(keys(top));
                break;
            }
        case 'values':
            {
                const top = vm.popStackObject();
                vm.pushStack(values(top));
                break;
            }
        case 'length':
            {
                const top = vm.popStackObject();
                vm.pushStack(length(top));
                break;
            }
    }
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