import { InputScope, parseScopes } from "./assembler";
import { Value } from "./types";
import VirtualMachine from "./virtualMachine";

const rawCode: InputScope[] = [
    {
        "name": "Main",
        "data": [
            ["Push", 0],
            ":Start",
            ["Call", ["", "Step"]],
            ["$isDone"],
            ["JumpFalse", ":Start"],
            "$done"
        ]
    },
    {
        "name": "Step",
        "data": [
            ["$rand"],
            ["$rand"],
            ["$add"],
            ["$add"],
            "Return"
        ]
    }
]

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

const scopes = parseScopes(rawCode);

const vm = new VirtualMachine(64, runHandler);
vm.addScopes(scopes);

vm.run('Main');