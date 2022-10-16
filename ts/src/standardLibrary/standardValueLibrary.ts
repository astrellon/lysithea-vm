import Scope, { IReadOnlyScope } from "../scope";
import { isValueString, valueCompareTo, valueToString, valueTypeof } from "../types";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.define('toString', (vm, numArgs) =>
    {
        let top = vm.peekStack();
        if (isValueString(top))
        {
            return;
        }

        top = vm.popStack();
        vm.pushStack(valueToString(top));
    });
    result.define('typeof', (vm, numArgs) =>
    {
        const top = vm.popStack();
        vm.pushStack(valueTypeof(top));
    });

    result.define('compareTo', (vm, numArgs) =>
    {
        const right = vm.popStack();
        const left = vm.popStack();
        vm.pushStack(valueCompareTo(left, right));
    });

    result.define('print', (vm, numArgs) =>
    {
        const args = vm.getArgs(numArgs);
        console.log(args.join(''));
    });

    return result;
}