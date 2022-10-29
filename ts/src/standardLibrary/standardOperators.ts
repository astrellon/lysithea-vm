import Scope, { IReadOnlyScope } from "../scope";
import BuiltinFunctionValue from "../values/builtinFunctionValue";
import { isNumberValue } from "../values/numberValue";
import { isStringValue } from "../values/stringValue";

export const operatorScope: IReadOnlyScope = createOperatorScope();

export function createOperatorScope()
{
    const result = new Scope();

    result.define('>', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) > 0);
    }));

    result.define('>=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) >= 0);
    }));

    result.define('==', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) === 0);
    }));

    result.define('!=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) !== 0);
    }));

    result.define('<', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) < 0);
    }));

    result.define('<=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.get(0).compareTo(args.get(1)) <= 0);
    }));

    result.define('+', new BuiltinFunctionValue((vm, args) =>
    {
        if (args.value.length === 0)
        {
            return;
        }

        const first = args.get(0);
        if (isStringValue(first))
        {
            const result = args.value.map(v => v.toString()).join('');
            vm.pushStackString(result);
        }
        else if (isNumberValue(first))
        {
            let result = 0;
            for (let i = 0; i < args.value.length; i++)
            {
                const item = args.value[i];
                if (isNumberValue(item))
                {
                    result += item.value;
                }
                else
                {
                    throw new Error('Add only works on numbers and strings');
                }
            }
            vm.pushStackNumber(result);
        }
    }));

    result.define('-', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackNumber(args.getNumber(0) - args.getNumber(1));
    }));

    result.define('*', new BuiltinFunctionValue((vm, args) =>
    {
        if (args.value.length < 2)
        {
            throw new Error('Multiply operator expects more than 1 input');
        }

        let total = 1.0;
        for (let i = 0; i < args.value.length; i++)
        {
            total *= args.getNumber(i);
        }
        vm.pushStackNumber(total);
    }));

    result.define('/', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackNumber(args.getNumber(0) / args.getNumber(1));
    }));

    result.define('%', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackNumber(args.getNumber(0) % args.getNumber(1));
    }));

    return result;
}