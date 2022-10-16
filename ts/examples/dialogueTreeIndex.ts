import { ArrayValue, BuiltinFunctionValue, FunctionValue, isValueAnyFunction, isValueArray, isValueFunction, numberCompareTo, Value, valueToString, VMFunction } from "../src/types";
import VirtualMachine from "../src/virtualMachine";
import VirtualMachineRunner from "../src/virtualMachineRunner";
import fs from "fs";
import readline from "readline";
import Scope from "../src/scope";
import VirtualMachineAssembler from "../src/assembler";

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

let isShopEnabled = false;
let playerName = '<Unset>';
let choiceBuffer: (FunctionValue | BuiltinFunctionValue)[] = [];

const dialogueScope = new Scope();
dialogueScope.define('say', (vm, numArgs) =>
{
    say(vm.popStack());
});
dialogueScope.define('getPlayerName', (vm, numArgs) =>
{
    vm.paused = true;
    rl.question('', (name) =>
    {
        playerName = name;
        vm.paused = false;
    });
});
dialogueScope.define('randomSay', (vm, numArgs) =>
{
    randomSay(vm.popStackCast(isValueArray));
});
dialogueScope.define('choice', (vm, numArgs) =>
{
    const choiceJumpLabel = vm.popStackCast(isValueAnyFunction);
    const choiceText = vm.popStack();
    choiceBuffer.push(choiceJumpLabel);
    sayChoice(choiceText);
});
dialogueScope.define('waitForChoice', (vm, numArgs) =>
{
    if (choiceBuffer.length === 0)
    {
        throw new Error('No choices to wait for!');
    }

    vm.paused = true;
    let choiceIndexStr = '-1';
    rl.question('Enter choice:', (choice) =>
    {
        choiceIndexStr = choice;
        let choiceIndex = Number.parseInt(choiceIndexStr);
        if (doChoice(choiceIndex, vm))
        {
            vm.paused = false;
        }
        else
        {
            console.log('Invalid choice');
        }
    });
});
dialogueScope.define('openTheShop', (vm, numArgs) =>
{
    isShopEnabled = true;
});
dialogueScope.define('openShop', (vm, numArgs) =>
{
    console.log('Opening the shop to the player and quitting dialogue');
});
dialogueScope.define('isShopEnabled', (vm, numArgs) =>
{
    vm.pushStack(isShopEnabled);
});
dialogueScope.define('moveTo', (vm, numArgs) =>
{
    const label = valueToString(vm.popStack());
    const proc = vm.popStackCast(isValueFunction);
    vm.callFunction(proc, 0, false);
    vm.jump(label);
});

function say(value: Value)
{
    const text = value?.toString().replace('{playerName}', playerName);
    console.log('Say:', text);
}

function randomSay(value: ArrayValue)
{
    var randIndex = Math.floor(Math.random() * value.length);
    say(value[randIndex]);
}

function sayChoice(value: Value)
{
    console.log('- ', choiceBuffer.length, ':', value?.toString());
}

function doChoice(index: number, vm: VirtualMachine): boolean
{
    if (index < 1 || index > choiceBuffer.length)
    {
        return false;
    }

    index--;

    const choice = choiceBuffer[index];
    choiceBuffer = [];
    vm.callFunction(choice, 0, false);
    return true;
}

const file = fs.readFileSync('../examples/testDialogue.lisp', { encoding: 'utf-8' });

const assembler = new VirtualMachineAssembler();
assembler.builtinScope.combineScope(dialogueScope);
const code = assembler.parseFromText(file);

const vm = new VirtualMachine(64);
// Some functions are looked up dynamically, so the VM must know then as well as the assembler.
vm.builtinScope.combineScope(dialogueScope);
vm.currentCode = code;
vm.running = true;

const runner = new VirtualMachineRunner(vm);
runner.start();