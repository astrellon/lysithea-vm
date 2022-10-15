import { InputScope, parseScopes } from "../src/assemblerOld";
import { ArrayValue, Value } from "../src/types";
import VirtualMachine from "../src/virtualMachine";
import VirtualMachineRunner from "../src/virtualMachineRunner";
import fs from "fs";
import readline from "readline";

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

let isShopEnabled = false;
let playerName = '<Unset>';
let choiceBuffer: Value[] = [];

function runHandler(command: string, vm: VirtualMachine)
{
    if (command === 'say')
    {
        say(vm.popStack());
    }
    else if (command === 'getPlayerName')
    {
        vm.paused = true;
        rl.question('', (name) =>
        {
            playerName = name;
            vm.paused = false;
        });
    }
    else if (command === 'randomSay')
    {
        randomSay(vm.popStackArray());
    }
    else if (command === 'choice')
    {
        const choiceJumpLabel = vm.popStack();
        const choiceText = vm.popStack();
        choiceBuffer.push(choiceJumpLabel);
        sayChoice(choiceText);
    }
    else if (command === 'waitForChoice')
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
    }
    else if (command === 'openTheShop')
    {
        isShopEnabled = true;
    }
    else if (command === 'openShop')
    {
        console.log('Opening the shop to the player and quitting dialogue');
    }
    else if (command === 'isShopEnabled')
    {
        vm.pushStack(isShopEnabled);
    }
    else
    {
        console.error('Unknown command:', command);
    }
}

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
    vm.jumpValue(choice);
    return true;
}

const inputJson: InputScope[] = JSON.parse(fs.readFileSync('../examples/testDialogue.json', 'utf-8'));
const scopes = parseScopes(inputJson);

const vm = new VirtualMachine(64, runHandler);
vm.addScopes(scopes);
vm.setCurrentScope('Main');

const runner = new VirtualMachineRunner(vm);
runner.start();