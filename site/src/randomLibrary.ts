import { Scope } from "lysithea-vm/src/scope";
import { isArrayValue } from "lysithea-vm/src/values/arrayValue";
import { BuiltinFunctionValue } from "lysithea-vm/src/values/builtinFunctionValue";
import { IArrayValue } from "lysithea-vm/src/values/ivalues";
import { ObjectValue, ObjectValueMap } from "lysithea-vm/src/values/objectValue";

function createScope()
{
    const result = new Scope();

    const randomFunctions: ObjectValueMap =
    {
        pick: new BuiltinFunctionValue((vm, args) =>
        {
            let input: IArrayValue = args;
            if (args.value.length === 1)
            {
                const list = args.getIndexCast(0, isArrayValue);
                if (list)
                {
                    input = list;
                }
            }

            const index = Math.floor(Math.random() * input.arrayValues().length);
            vm.pushStack(input.arrayValues()[index]);
        }, "random.pick")
    }

    result.define("random", new ObjectValue(randomFunctions));

    return result;
}

export const randomScope = createScope();