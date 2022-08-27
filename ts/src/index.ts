import { CodeLine, Operator, Scope, Value } from "./types";
import VirtualMachine from "./virtualMachine";

const code: CodeLine[] = [
    { operator: Operator.Push, value: 5 },
    { operator: Operator.Push, value: 7 },
    { operator: Operator.Push, value: 'add' },
    { operator: Operator.Run },
    { operator: Operator.Push, value: 'text' },
    { operator: Operator.Run },
];
const scope: Scope = {
    name: 'Start',
    code,
    labels: {}
}

function runHandler(value: Value, vm: VirtualMachine)
{
    const commandName = value?.toString();
    if (commandName === 'add')
    {
        const num1 = vm.popObject() as number;
        const num2 = vm.popObject() as number;
        vm.pushObject(num1 + num2);
        return;
    }
    if (commandName === 'text')
    {
        const top = vm.popObject();
        console.log('TEXT:', top);
    }
}

const vm = new VirtualMachine(64, runHandler);
vm.addScope(scope);

vm.run('Start');