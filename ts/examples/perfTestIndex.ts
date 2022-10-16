import fs from "fs";
import VirtualMachineAssembler from "../src/assembler";
import Scope from "../src/scope";
import VirtualMachine from "../src/virtualMachine";

let counter = 0;
const perfScope = new Scope();
perfScope.define('add', (vm, numArgs) =>
{
    const num1 = vm.popStack() as number;
    const num2 = vm.popStack() as number;
    vm.pushStack(num1 + num2);
});
perfScope.define('rand', (vm, numArgs) =>
{
    vm.pushStack(Math.random());
});
perfScope.define('isDone', (vm, numArgs) =>
{
    vm.pushStack(counter++ > 1000000);
});

perfScope.define('done', (vm, numArgs) =>
{
    const total = vm.popStack() as number;
    console.log('Total:', total);
});

const file = fs.readFileSync('../examples/perfTest.lisp', {encoding: 'utf-8'});

const assembler = new VirtualMachineAssembler();
assembler.builtinScope.combineScope(perfScope);
const code = assembler.parseFromText(file);

const vm = new VirtualMachine(16);
vm.currentCode = code;
vm.running = true;

let before = Date.now();
while (vm.running && !vm.paused)
{
    vm.step();
}

let after = Date.now();
console.log('Time taken:', (after - before), 'ms');