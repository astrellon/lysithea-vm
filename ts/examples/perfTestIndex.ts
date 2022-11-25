import fs from "fs";
import VirtualMachine from "../src/virtualMachine";
import VirtualMachineAssembler from "../src/assembler";
import Scope from "../src/scope";

function createScope()
{
    const result = new Scope();
    result.defineFunc('rand', (vm, args) =>
    {
        vm.pushStackNumber(Math.random());
    });

    result.defineFunc('print', (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}

const perfScope = createScope();

const file = fs.readFileSync('../examples/perfTest.lys', {encoding: 'utf-8'});

const assembler = new VirtualMachineAssembler();
assembler.builtinScope.combineScope(perfScope);
const script = assembler.parseFromText(file);

const vm = new VirtualMachine(8);

let before = Date.now();
vm.execute(script);

let after = Date.now();
console.log('Time taken:', (after - before), 'ms');
