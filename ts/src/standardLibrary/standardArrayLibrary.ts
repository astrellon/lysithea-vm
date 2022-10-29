import Scope, { IReadOnlyScope } from "../scope";
import ArrayValue, { isArrayValue } from "../values/arrayValue";
import BuiltinFunctionValue from "../values/builtinFunctionValue";
import { IArrayValue, isIArrayValue, IValue } from "../values/ivalues";
import NullValue from "../values/nullValue";
import ObjectValue, { ObjectValueMap } from "../values/objectValue";

export const arrayScope: IReadOnlyScope = createArrayScope();

export function createArrayScope()
{
    const result = new Scope();

    const arrayFunctions: ObjectValueMap =
    {
        join: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStack(new ArrayValue(args.value));
        }),

        length: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            vm.pushStackNumber(top.arrayValues().length);
        }),

        get: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            const result = get(top, index);
            if (result !== undefined)
            {
                vm.pushStack(result);
            }
            else
            {
                vm.pushStack(NullValue.Value);
            }
        }),

        set: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            const value = args.getIndex(2);
            vm.pushStack(set(top, index, value));

        }),

        insert: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            const value = args.getIndex(2);
            vm.pushStack(insert(top, index, value));
        }),

        insertFlatten: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            const value = args.getIndexCast(2, isIArrayValue);
            vm.pushStack(insertFlatten(top, index, value));
        }),

        remove: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const value = args.getIndex(1);
            vm.pushStack(remove(top, value));
        }),

        removeAt: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            vm.pushStack(removeAt(top, index));
        }),

        removeAll: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const value = args.getIndex(1);
            vm.pushStack(removeAll(top, value));
        }),

        contains: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const value = args.getIndex(1);
            vm.pushStackBool(contains(top, value));
        }),

        indexOf: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const value = args.getIndex(1);
            vm.pushStackNumber(indexOf(top, value));
        }),

        sublist: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isArrayValue);
            const index = args.getNumber(1);
            const length = args.getNumber(2);
            vm.pushStack(sublist(top, index, length));
        }),
    }

    result.define('array', new ObjectValue(arrayFunctions));

    return result;
}

export function set(target: ArrayValue, index: number, input: IValue): ArrayValue
{
    let result = [...target.value];
    result[target.calcIndex(index)] = input;
    return new ArrayValue(result, target.isArgumentValue);
}

export function get(target: IArrayValue, index: number): IValue | undefined
{
    return target.tryGetIndex(index);
}

export function insert(target: ArrayValue, index: number, input: IValue): ArrayValue
{
    index = target.calcIndex(index);
    const result = [...target.value];
    result.splice(index, 0, input);
    return new ArrayValue(result, target.isArgumentValue);
}

export function insertFlatten(target: ArrayValue, index: number, input: IArrayValue): ArrayValue
{
    index = target.calcIndex(index);
    const result = [...target.value];
    result.splice(index, 0, ...input.arrayValues());
    return new ArrayValue(result, target.isArgumentValue);
}

export function removeAt(target: ArrayValue, index: number): ArrayValue
{
    index = target.calcIndex(index);
    const result = [...target.value];
    result.splice(index, 1);
    return new ArrayValue(result, target.isArgumentValue);
}

export function remove(target: ArrayValue, value: IValue): ArrayValue
{
    const index = indexOf(target, value);
    if (index < 0)
    {
        return target;
    }

    return removeAt(target, index);
}

export function removeAll(target: ArrayValue, value: IValue): ArrayValue
{
    return new ArrayValue(target.value.filter(v => v.compareTo(value) !== 0), target.isArgumentValue);
}

export function contains(target: IArrayValue, value: IValue): boolean
{
    return indexOf(target, value) >= 0;
}

export function indexOf(target: IArrayValue, value: IValue): number
{
    return target.arrayValues().findIndex(v => v.compareTo(value) === 0);
}

export function sublist(target: ArrayValue, index: number, length: number): ArrayValue
{
    index = target.calcIndex(index);
    if (length < 0)
    {
        return new ArrayValue(target.value.slice(index), target.isArgumentValue);
    }
    return new ArrayValue(target.value.slice(index, index + length), target.isArgumentValue);
}