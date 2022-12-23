import { Scope, IReadOnlyScope } from "../scope";
import { BuiltinFunctionValue } from "../values/builtinFunctionValue";
import { ObjectValue, ObjectValueMap } from "../values/objectValue";
import { StringValue, isStringValue } from "../values/stringValue";

export const stringScope: IReadOnlyScope = createStringScope();

export function createStringScope()
{
    const result = new Scope();

    const stringFunctions: ObjectValueMap =
    {
        join: new BuiltinFunctionValue((vm, args) =>
        {
            if (args.value.length < 2)
            {
                throw new Error('Not enough arguments for string join');
            }

            const separator = args.value[0];
            const result = args.value.slice(1).join(separator.toString());
            vm.pushStackString(result);
        }, 'string.join'),

        length: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getString(0);
            vm.pushStackNumber(top.length);
        }, 'string.length'),

        get: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const index = args.getNumber(1);
            vm.pushStackString(top.value[top.getIndex(index)]);
        }, 'string.get'),

        set: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const index = args.getNumber(1);
            const value = args.getIndex(2).toString();
            vm.pushStack(set(top, index, value));
        }, 'string.set'),

        insert: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const index = args.getNumber(1);
            const value = args.getIndex(2).toString();
            vm.pushStack(insert(top, index, value));
        }, 'string.insert'),

        substring: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const index = args.getNumber(1);
            const length = args.getNumber(2);
            vm.pushStack(substring(top, index, length));
        }, 'string.substring'),

        removeAt: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const index = args.getNumber(1);
            vm.pushStack(removeAt(top, index));
        }, 'string.removeAt'),

        removeAll: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndexCast(0, isStringValue);
            const values = args.getIndex(1).toString();
            vm.pushStack(removeAll(top, values));
        }, 'string.removeAll')
    }

    result.tryDefine('string', new ObjectValue(stringFunctions));

    return result;
}

export function set(input: StringValue, index: number, value: string): StringValue
{
    index = input.getIndex(index);
    return new StringValue(`${input.value.substring(0, index)}${value}${input.value.substring(index + 1)}`);
}

export function insert(input: StringValue, index: number, value: string): StringValue
{
    index = input.getIndex(index);
    return new StringValue(`${input.value.substring(0, index)}${value}${input.value.substring(index)}`);
}

export function removeAt(input: StringValue, index: number): StringValue
{
    index = input.getIndex(index);
    return new StringValue(`${input.value.substring(0, index)}${input.value.substring(index + 1)}`);
}

export function removeAll(input: StringValue, values: string): StringValue
{
    return new StringValue(input.value.replace(values, ''));
}

export function substring(input: StringValue, index: number, length: number): StringValue
{
    index = input.getIndex(index);
    return new StringValue(input.value.substring(index, index + length));
}
