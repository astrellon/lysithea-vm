import { BoolValue } from "../index";
import { Scope, IReadOnlyScope } from "../scope";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.tryDefineFunc('toString', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).toString());
    });

    result.tryDefineFunc('typeof', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).typename());
    });

    result.tryDefineFunc('isDefined', (vm, args) =>
    {
        const top = args.getIndex(0).toString();
        const isDefined = vm.currentScope.get(top) !== undefined;
        vm.pushStackBool(isDefined);
    });

    result.tryDefineFunc('isBuiltin', (vm, args) =>
    {
        const top = args.getIndex(0).toString();
        const isBuiltin = !!vm.builtinScope && vm.builtinScope.get(top) !== undefined;
        vm.pushStackBool(isBuiltin);
    });

    result.tryDefineFunc('compareTo', (vm, args) =>
    {
        vm.pushStackNumber(args.getIndex(0).compareTo(args.getIndex(1)));
    });

    result.tryDefineFunc('print', (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}