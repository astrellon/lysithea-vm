import { ObjectValueMap, Scope, IArrayValue, isArrayValue, ObjectValue, BuiltinFunctionValue } from 'lysithea-vm';

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

    result.tryDefine("random", new ObjectValue(randomFunctions));

    return result;
}

export const randomScope = createScope();