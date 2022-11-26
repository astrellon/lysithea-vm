import { Scope, IReadOnlyScope } from "../scope";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.defineFunc('toString', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).toString());
    });

    result.defineFunc('typeof', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).typename());
    });

    result.defineFunc('compareTo', (vm, args) =>
    {
        vm.pushStackNumber(args.getIndex(0).compareTo(args.getIndex(1)));
    });

    result.defineFunc('print', (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}