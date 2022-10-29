import Scope, { IReadOnlyScope } from "../scope";
import BuiltinFunctionValue from "../values/builtinFunctionValue";

export const miscScope: IReadOnlyScope = createMiscScope();

export function createMiscScope()
{
    const result = new Scope();

    result.define('toString', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackString(args.get(0).toString());
    }));

    result.define('typeof', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackString(args.get(0).typename());
    }));

    result.define('compareTo', new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackNumber(args.get(0).compareTo(args.get(1)));
    }));

    result.define('print', new BuiltinFunctionValue((vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    }));

    return result;
}