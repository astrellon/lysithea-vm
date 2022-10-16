import VirtualMachine from "../src/virtualMachine";
import fs from "fs";
import { addToScope, LibraryType } from "../src/standardLibrary/index";
import VirtualMachineAssembler from "../src/assembler";

const file = fs.readFileSync('../examples/testStandardLibrary.json', {encoding: 'utf-8'});

const assembler = new VirtualMachineAssembler();
addToScope(assembler.builtinScope, LibraryType.all);
const code = assembler.parseFromText(file);

const vm = new VirtualMachine(16);
vm.currentCode = code;
vm.running = true;

const before = Date.now();
while (vm.running && !vm.paused)
{
    vm.step();
}

const after = Date.now();
console.log('Time taken:', (after - before), 'ms');
