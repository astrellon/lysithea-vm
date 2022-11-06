import Scope, { IReadOnlyScope } from "../scope";
import BuiltinFunctionValue from "../values/builtinFunctionValue";
import { isNumberValue } from "../values/numberValue";

export const operatorScope: IReadOnlyScope = createOperatorScope();

export function createOperatorScope()
{
    const result = new Scope();

    result.define('>', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) > 0);
    }));

    result.define('>=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) >= 0);
    }));

    result.define('==', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) === 0);
    }));

    result.define('!=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) !== 0);
    }));

    result.define('<', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) < 0);
    }));

    result.define('<=', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(args.getIndex(0).compareTo(args.getIndex(1)) <= 0);
    }));

    result.define('+', new BuiltinFunctionValue((vm, args) =>
    {
        if (args.value.length === 0)
        {
            throw new Error('Addition operator expects at least 1 input');
        }

        const first = args.getIndex(0);
        if (isNumberValue(first))
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
                    throw new Error('Addition operator expects all numbers');
                }
            }
            vm.pushStackNumber(result);
        }
        else
        {
            const result = args.value.map(v => v.toString()).join('');
            vm.pushStackString(result);
        }
    }));

    result.define('-', new BuiltinFunctionValue((vm, args) =>
    {
        if (args.value.length === 0)
        {
            throw new Error('Subtraction operator expects at least 1 input');
        }

        let total = args.getNumber(0);
        if (args.value.length === 1)
        {
            vm.pushStackNumber(-total);
            return
        }

        for (let i = 1; i < args.value.length; i++)
        {
            total -= args.getNumber(i);
        }
        vm.pushStackNumber(total);
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
        if (args.value.length < 2)
        {
            throw new Error('Divide operator expects more than 1 input');
        }

        let total = args.getNumber(0);
        for (let i = 1; i < args.value.length; i++)
        {
            total /= args.getNumber(i);
        }
        vm.pushStackNumber(total);
    }));

    result.define('%', new BuiltinFunctionValue((vm, args) =>
    {
        if (args.value.length < 2)
        {
            throw new Error('Modulo operator expects 2 inputs');
        }

        vm.pushStackNumber(args.getNumber(0) % args.getNumber(1));
    }));

    return result;
}