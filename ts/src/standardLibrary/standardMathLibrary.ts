import Scope, { IReadOnlyScope } from "../scope";
import { isValueNumber, ObjectValue, valueCompareTo, valueToString } from "../types";

const degToRad = Math.PI / 180.0;

export const mathScope: IReadOnlyScope = createMathScope();

export function createMathScope()
{
    const result = new Scope();

    const mathFunctions: ObjectValue =
    {
        E: Math.E,
        PI: Math.PI,
        DegToRad: degToRad,

        sin: (vm, numArgs) =>
        {
            vm.pushStack(Math.sin(vm.popStackCast(isValueNumber)));
        },
        cos: (vm, numArgs) =>
        {
            vm.pushStack(Math.cos(vm.popStackCast(isValueNumber)));
        },
        tan: (vm, numArgs) =>
        {
            vm.pushStack(Math.tan(vm.popStackCast(isValueNumber)));
        },
        exp: (vm, numArgs) =>
        {
            vm.pushStack(Math.exp(vm.popStackCast(isValueNumber)));
        },
        ceil: (vm, numArgs) =>
        {
            vm.pushStack(Math.ceil(vm.popStackCast(isValueNumber)));
        },
        floor: (vm, numArgs) =>
        {
            vm.pushStack(Math.floor(vm.popStackCast(isValueNumber)));
        },
        round: (vm, numArgs) =>
        {
            vm.pushStack(Math.round(vm.popStackCast(isValueNumber)));
        },
        isFinite: (vm, numArgs) =>
        {
            vm.pushStack(Number.isFinite(vm.popStackCast(isValueNumber)));
        },
        isNaN: (vm, numArgs) =>
        {
            vm.pushStack(isNaN(vm.popStackCast(isValueNumber)));
        },
        parse: (vm, numArgs) =>
        {
            let top = vm.peekStack();
            if (isValueNumber(top))
            {
                return;
            }

            top = vm.popStack();
            const num = parseFloat(valueToString(top));
            vm.pushStack(num);
        },
        log: (vm, numArgs) =>
        {
            vm.pushStack(Math.log(vm.popStackCast(isValueNumber)));
        },
        abs: (vm, numArgs) =>
        {
            vm.pushStack(Math.abs(vm.popStackCast(isValueNumber)));
        },
        max: (vm, numArgs) =>
        {
            const right = vm.popStack();
            const left = vm.popStack();
            vm.pushStack(valueCompareTo(left, right) > 0 ? left : right);
        },
        min: (vm, numArgs) =>
        {
            const right = vm.popStack();
            const left = vm.popStack();
            vm.pushStack(valueCompareTo(left, right) < 0 ? left : right);
        }
    };

    result.define('math', mathFunctions);

    return result;
}