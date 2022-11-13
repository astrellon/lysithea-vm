import fs from "fs";
import VirtualMachineAssembler from "../src/assembler";
import Scope from "../src/scope";
import BuiltinFunctionValue from "../src/values/builtinFunctionValue";
import VirtualMachine from "../src/virtualMachine";

// let counter = 0;
const perfScope = new Scope();
perfScope.define('add', new BuiltinFunctionValue((vm, args) =>
{
    const num1 = args.getNumber(0);
    const num2 = args.getNumber(1);
    vm.pushStackNumber(num1+ num2);
}));
perfScope.define('rand', new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(Math.random());
}));
perfScope.define('lessThan', new BuiltinFunctionValue((vm, args) =>
{
    const left = args.getNumber(0);
    const right = args.getNumber(1);
    vm.pushStackBool(left < right);
}));

perfScope.define('print', new BuiltinFunctionValue((vm, args) =>
{
    console.log(args.value.map(c => c.toString()).join(''));
}));

const file = fs.readFileSync('../examples/perfTest.lys', {encoding: 'utf-8'});

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