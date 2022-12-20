import { Scope, IReadOnlyScope } from "../scope";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.constantFunc('toString', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).toString());
    });

    result.constantFunc('typeof', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).typename());
    });

    result.constantFunc('compareTo', (vm, args) =>
    {
        vm.pushStackNumber(args.getIndex(0).compareTo(args.getIndex(1)));
    });

    result.constantFunc('print', (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}