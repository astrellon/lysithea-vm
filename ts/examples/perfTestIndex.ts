import fs from "fs";
import VirtualMachineAssembler from "../src/assembler";
import Scope from "../src/scope";
import BuiltinFunctionValue from "../src/values/builtinFunctionValue";
import { isNumberValue } from "../src/values/numberValue";
import VirtualMachine from "../src/virtualMachine";

let counter = 0;
const perfScope = new Scope();
perfScope.define('add', new BuiltinFunctionValue((vm, args) =>
{
    const num1 = vm.popStackCast(isNumberValue).value;
    const num2 = vm.popStackCast(isNumberValue).value;
    vm.pushStackNumber(num1+ num2);
}));
perfScope.define('rand', new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(Math.random());
}));
perfScope.define('isDone', new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackBool(counter++ > 1000000);
}));

perfScope.define('done', new BuiltinFunctionValue((vm, args) =>
{
    const total = vm.popStackCast(isNumberValue).value;
    console.log('Total:', total);
}));

const file = fs.readFileSync('../examples/perfTest.lisp', {encoding: 'utf-8'});

const assembler = new VirtualMachineAssembler();
assembler.builtinScope.combineScope(perfScope);
const script = assembler.parseFromText(file);

const vm = new VirtualMachine(16);
vm.changeToScript(script);
vm.running = true;

let before = Date.now();
while (vm.running && !vm.paused)
{
    vm.step();
}

let after = Date.now();
console.log('Time taken:', (after - before), 'ms');