import Scope, { IReadOnlyScope } from "../scope";
import BuiltinFunctionValue from "../values/builtinFunctionValue";
import NumberValue, { isNumberValue } from "../values/numberValue";
import ObjectValue, { ObjectValueMap } from "../values/objectValue";

const degToRad = Math.PI / 180.0;

export const mathScope: IReadOnlyScope = createMathScope();

export function createMathScope()
{
    const result = new Scope();

    const mathFunctions: ObjectValueMap =
    {
        E: new NumberValue(Math.E),
        PI: new NumberValue(Math.PI),
        DegToRad: new NumberValue(degToRad),

        sin: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.sin(args.atNumber(0)));
        }),
        cos: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.cos(args.atNumber(0)));
        }),
        tan: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.tan(args.atNumber(0)));
        }),
        exp: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.exp(args.atNumber(0)));
        }),
        ceil: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.ceil(args.atNumber(0)));
        }),
        floor: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.floor(args.atNumber(0)));
        }),
        round: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.round(args.atNumber(0)));
        }),
        isFinite: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackBool(Number.isFinite(args.atNumber(0)));
        }),
        isNaN: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackBool(isNaN(args.atNumber(0)));
        }),
        parse: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.at(0);
            if (isNumberValue(top))
            {
                vm.pushStack(top);
            }
            else
            {
                vm.pushStackNumber(parseFloat(top.toString()));
            }
        }),
        log: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.log(args.atNumber(0)));
        }),
        abs: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.abs(args.atNumber(0)));
        }),
        max: new BuiltinFunctionValue((vm, args) =>
        {
            let max = args.value[0];
            for (let i = 1; i < args.value.length; i++)
            {
                const next = args.value[i];
                if (next.compareTo(max) > 0)
                {
                    max = next;
                }
            }
            vm.pushStack(max);
        }),
        min: new BuiltinFunctionValue((vm, args) =>
        {
            let min = args.value[0];
            for (let i = 1; i < args.value.length; i++)
            {
                const next = args.value[i];
                if (next.compareTo(min) < 0)
                {
                    min = next;
                }
            }
            vm.pushStack(min);
        }),

        inc: incNumber,
        dec: decNumber
    };

    result.define('math', new ObjectValue(mathFunctions));

    return result;
}

export const incNumber = new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(args.atNumber(0) + 1);
});
export const decNumber = new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(args.atNumber(0) - 1);
});