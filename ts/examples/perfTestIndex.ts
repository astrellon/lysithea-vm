import { InputScope, parseScopes } from "../src/assembler";
import { Value } from "../src/types";
import VirtualMachine from "../src/virtualMachine";
import fs from "fs";

let counter = 0;
function runHandler(command: string, vm: VirtualMachine)
{
    if (command === 'add')
    {
        const num1 = vm.popStack() as number;
        const num2 = vm.popStack() as number;
        vm.pushStack(num1 + num2);
    }
    else if (command === 'rand')
    {
        vm.pushStack(Math.random());
    }
    else if (command === 'isDone')
    {
        vm.pushStack(counter++ > 1000000);
    }
    else if (command === 'done')
    {
        const total = vm.popStack() as number;
        console.log('Total:', total);
    }
    else if (command === 'text')
    {
        const top = vm.popStack();
        console.log('TEXT:', top);
    }
}

const inputJson: InputScope[] = JSON.parse(fs.readFileSync('../examples/perfTest.json', 'utf-8'));
const scopes = parseScopes(inputJson);

const vm = new VirtualMachine(64, runHandler);
vm.addScopes(scopes);

const before = Date.now();
vm.setCurrentScope('Main');
vm.running = true;
while (vm.running && !vm.paused)
{
    vm.step();
}
const after = Date.now();

console.log('Time taken:', (after - before), 'ms');