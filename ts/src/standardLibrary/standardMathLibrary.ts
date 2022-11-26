import { Scope, IReadOnlyScope } from "../scope";
import { BuiltinFunctionValue } from "../values/builtinFunctionValue";
import { NumberValue, isNumberValue } from "../values/numberValue";
import { ObjectValue, ObjectValueMap } from "../values/objectValue";

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
            vm.pushStackNumber(Math.sin(args.getNumber(0)));
        }, "math.sin"),
        cos: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.cos(args.getNumber(0)));
        }, "math.cos"),
        tan: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.tan(args.getNumber(0)));
        }, "math.tan"),
        exp: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.exp(args.getNumber(0)));
        }, "math.exp"),
        ceil: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.ceil(args.getNumber(0)));
        }, "math.ceil"),
        floor: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.floor(args.getNumber(0)));
        }, "math.floor"),
        round: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.round(args.getNumber(0)));
        }, "math.round"),
        isFinite: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackBool(Number.isFinite(args.getNumber(0)));
        }, "math.isFinite"),
        isNaN: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackBool(isNaN(args.getNumber(0)));
        }, "math.isNaN"),
        parse: new BuiltinFunctionValue((vm, args) =>
        {
            const top = args.getIndex(0);
            if (isNumberValue(top))
            {
                vm.pushStack(top);
            }
            else
            {
                vm.pushStackNumber(parseFloat(top.toString()));
            }
        }, "math.parse"),
        log: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.log(args.getNumber(0)));
        }, "math.log"),
        abs: new BuiltinFunctionValue((vm, args) =>
        {
            vm.pushStackNumber(Math.abs(args.getNumber(0)));
        }, "math.abs"),
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
        }, "math.max"),
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
        }, "math.min"),
        sum: new BuiltinFunctionValue((vm, args) =>
        {
            let result = 0.0;
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
        }, "math.sum")
    };

    result.define('math', new ObjectValue(mathFunctions));

    return result;
}
