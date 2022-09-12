import { InputScope, parseScopes } from "./assembler";
import { Value } from "./types";
import VirtualMachine from "./virtualMachine";
import fs from "fs";

let counter = 0;
function runHandler(value: Value, vm: VirtualMachine)
{
    const commandName = value?.toString();
    if (commandName === 'add')
    {
        const num1 = vm.popObject() as number;
        const num2 = vm.popObject() as number;
        vm.pushObject(num1 + num2);
    }
    else if (commandName === 'rand')
    {
        vm.pushObject(Math.random());
    }
    else if (commandName === 'isDone')
    {
        vm.pushObject(counter++ > 1000000);
    }
    else if (commandName === 'done')
    {
        const total = vm.popObject() as number;
        console.log('Total:', total);
    }
    else if (commandName === 'text')
    {
        const top = vm.popObject();
        console.log('TEXT:', top);
    }
}

const inputJson: InputScope[] = JSON.parse(fs.readFileSync('../examples/perfTest.json', 'utf-8'));
const scopes = parseScopes(inputJson);

const vm = new VirtualMachine(64, runHandler);
vm.addScopes(scopes);

const before = Date.now();
vm.run('Main');
const after = Date.now();

console.log('Time taken:', (after - before), 'ms');