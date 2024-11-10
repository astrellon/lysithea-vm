import fs from "fs";
import { addToScope, LibraryType } from "../src/standardLibrary/index";
import { assertScope } from "../src/standardLibrary/standardAssertLibrary";
import { VirtualMachine } from "../src/virtualMachine";
import { Assembler } from "../src/assembler/assembler";

const filename = '../examples/testObject.lys';
const file = fs.readFileSync(filename, {encoding: 'utf-8'});

const assembler = new Assembler();
addToScope(assembler.builtinScope, LibraryType.all);
assembler.builtinScope.combineScope(assertScope);
const script = assembler.parseFromText(filename, file);

const vm = new VirtualMachine(16);

const before = Date.now();

vm.execute(script);

const after = Date.now();
console.log('Time taken:', (after - before), 'ms');