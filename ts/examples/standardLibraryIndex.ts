import { InputScope, parseScopes } from "../src/assemblerOld";
import VirtualMachine from "../src/virtualMachine";
import { valueToString } from "../src/types";
import fs from "fs";
import { addToVirtualMachine, LibraryType } from "../src/standardLibrary/index";
import { addAssertHandler } from "../src/standardLibrary/standardAssertLibrary";

function runHandler(command: string, vm: VirtualMachine)
{
    if (command === 'print')
    {
        const top = vm.popStack();
        console.log('Print:', valueToString(top));
    }
    else
    {
        console.warn('Unknown command', command);
    }
}

const inputJson: InputScope[] = JSON.parse(fs.readFileSync('../examples/testStandardLibrary.json', 'utf-8'));
const scopes = parseScopes(inputJson);

const vm = new VirtualMachine(64, runHandler);
addToVirtualMachine(vm, LibraryType.all);
addAssertHandler(vm);
vm.addScopes(scopes);

vm.setCurrentScope('Main');
vm.running = true;
while (vm.running && !vm.paused)
{
    vm.step();
}