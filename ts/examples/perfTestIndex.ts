import fs from "fs";
import { VirtualMachine } from "../src/virtualMachine";
import { Assembler } from "../src/assembler/assembler";
import { Scope } from "../src/scope";

function createScope()
{
    const result = new Scope();
    result.tryDefineFunc('rand', (vm, args) =>
    {
        vm.pushStackNumber(Math.random());
    });

    return result;
}

const perfScope = createScope();

const filename = '../examples/perfTest.lys';
const file = fs.readFileSync(filename, {encoding: 'utf-8'});

const assembler = new Assembler();
assembler.builtinScope.combineScope(perfScope);
const script = assembler.parseFromText(filename, file);

const vm = new VirtualMachine(8);

let before = Date.now();
vm.execute(script);

let after = Date.now();
console.log('Time taken:', (after - before), 'ms');
