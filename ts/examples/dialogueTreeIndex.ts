import { VirtualMachine } from "../src/virtualMachine";
import { VirtualMachineRunner } from "../src/virtualMachineRunner";
import fs from "fs";
import readline from "readline";
import { Scope } from "../src/scope";
import { Assembler } from "../src/assembler/assembler";
import { arrayScope } from "../src/standardLibrary/standardArrayLibrary";
import { IArrayValue, IFunctionValue, isIArrayValue, isIFunctionValue, IValue, StringValue } from "../src/values/index";

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

let isShopEnabled = false;
let choiceBuffer: IFunctionValue[] = [];

const dialogueScope = new Scope();
dialogueScope.trySetConstantFunc('say', (vm, args) =>
{
    say(args.getIndex(0));
});
dialogueScope.trySetConstantFunc('getPlayerName', (vm, args) =>
{
    vm.paused = true;
    rl.question('', (name) =>
    {
        vm.globalScope.tryDefine('playerName', new StringValue(name));
        vm.paused = false;
    });
});
dialogueScope.trySetConstantFunc('randomSay', (vm, args) =>
{
    randomSay(args.getIndexCast(0, isIArrayValue));
});
dialogueScope.trySetConstantFunc('choice', (vm, args) =>
{
    const choiceText = args.getIndex(0);
    const choiceFunc = args.getIndexCast(1, isIFunctionValue);
    choiceBuffer.push(choiceFunc);
    sayChoice(choiceText);
});
dialogueScope.trySetConstantFunc('waitForChoice', (vm, args) =>
{
    if (choiceBuffer.length === 0)
    {
        throw new Error('No choices to wait for!');
    }

    vm.paused = true;
    let choiceIndexStr = '-1';
    rl.question('Enter choice: ', (choice) =>
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
dialogueScope.trySetConstantFunc('openTheShop', (vm, args) =>
{
    isShopEnabled = true;
});
dialogueScope.trySetConstantFunc('openShop', (vm, args) =>
{
    console.log('Opening the shop to the player and quitting dialogue');
});
dialogueScope.trySetConstantFunc('isShopEnabled', (vm, args) =>
{
    vm.pushStackBool(isShopEnabled);
});
dialogueScope.trySetConstantFunc('moveTo', (vm, args) =>
{
    const proc = args.getIndexCast(0, isIFunctionValue);
    const label = args.getIndex(1).toString();
    vm.callFunction(proc, 0, false);
    vm.jump(label);
});

function say(value: IValue)
{
    console.log('Say:', value.toString());
}

function randomSay(value: IArrayValue)
{
    const values = value.arrayValues();
    const randIndex = Math.floor(Math.random() * values.length);
    say(values[randIndex]);
}

function sayChoice(value: IValue)
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

const file = fs.readFileSync('../examples/testDialogue.lys', { encoding: 'utf-8' });

const assembler = new Assembler();
assembler.builtinScope.combineScope(dialogueScope);
assembler.builtinScope.combineScope(arrayScope);
const script = assembler.parseFromText(file);

const vm = new VirtualMachine(64);
// Some functions are looked up dynamically, so the VM must know then as well as the assembler.
vm.changeToScript(script);
vm.running = true;

const runner = new VirtualMachineRunner(vm);
runner.start()
    .then(() =>
    {
        console.log('Program done!');
        process.exit(1);
    });