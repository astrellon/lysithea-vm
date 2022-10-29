import VirtualMachine from "../src/virtualMachine";
import fs from "fs";
import { addToScope, LibraryType } from "../src/standardLibrary/index";
import VirtualMachineAssembler from "../src/assembler";
import { assertScope } from "../src/standardLibrary/standardAssertLibrary";

const file = fs.readFileSync('../examples/testStandardLibrary.lisp', {encoding: 'utf-8'});

const assembler = new VirtualMachineAssembler();
addToScope(assembler.builtinScope, LibraryType.all);
assembler.builtinScope.combineScope(assertScope);
const script = assembler.parseFromText(file);

const vm = new VirtualMachine(16);

const before = Date.now();

vm.execute(script);

const after = Date.now();
console.log('Time taken:', (after - before), 'ms');