import { Scope, IReadOnlyScope } from "../scope";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.trySetConstantFunc('toString', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).toString());
    });

    result.trySetConstantFunc('typeof', (vm, args) =>
    {
        vm.pushStackString(args.getIndex(0).typename());
    });

    result.trySetConstantFunc('compareTo', (vm, args) =>
    {
        vm.pushStackNumber(args.getIndex(0).compareTo(args.getIndex(1)));
    });

    result.trySetConstantFunc('print', (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}