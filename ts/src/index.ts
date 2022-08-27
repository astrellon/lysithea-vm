import { CodeLine, Scope, Value } from "./types";
import VirtualMachine from "./virtualMachine";

const code: CodeLine[] = [
    { operator: 'push', value: 5 },
    { operator: 'push', value: 7 },
    { operator: 'push', value: 'add' },
    { operator: 'run' },
    { operator: 'push', value: 'text' },
    { operator: 'run' },
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