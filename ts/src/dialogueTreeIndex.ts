import { InputScope, parseScopes } from "./assembler";
import { ArrayValue, Value } from "./types";
import VirtualMachine from "./virtualMachine";
import fs from "fs";
import readline from "readline";

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

let isShopEnabled = false;
let playerName = '<Unset>';
let choiceBuffer: Value[] = [];

function runHandler(value: Value, vm: VirtualMachine)
{
    const commandName = value?.toString();
    if (commandName === 'say')
    {
        say(vm.popObject());
    }
    else if (commandName === 'getPlayerName')
    {
        vm.paused = true;
        rl.question('', (name) =>
        {
            playerName = name;
            vm.run(null);
        });
    }
    else if (commandName === 'randomSay')
    {
        randomSay(vm.popObjectCast<ArrayValue>());
    }
    else if (commandName === 'choice')
    {
        const choiceJumpLabel = vm.popObject();
        const choiceText = vm.popObject();
        choiceBuffer.push(choiceJumpLabel);
        sayChoice(choiceText);
    }
    else if (commandName === 'waitForChoice')
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
                vm.run(null);
            }
            else
            {
                console.log('Invalid choice');
            }
        });
    }
    else if (commandName === 'openTheShop')
    {
        isShopEnabled = true;
    }
    else if (commandName === 'openShop')
    {
        console.log('Opening the shop to the player and quitting dialogue');
    }
    else if (commandName === 'isShopEnabled')
    {
        vm.pushObject(isShopEnabled);
    }
    else
    {
        console.error('Unknown command:', commandName);
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

vm.run('Main');