import { InputScope, parseScopes } from "./assembler";
import { isValueArray, isValueBoolean, isValueNumber, isValueObject, isValueString, Value, valueToString } from "./types";
import VirtualMachine from "./virtualMachine";
import fs from "fs";

function runHandler(value: Value, vm: VirtualMachine)
{
    if (value == null)
    {
        throw new Error('Should not have null command');
    }

    if (isValueString(value))
    {
        const commandName = value.toString();
        switch (commandName)
        {
            case 'add':
            {
                const num1 = vm.popObject() as number;
                const num2 = vm.popObject() as number;
                vm.pushObject(num1 + num2);
                break;
            }
            case 'print':
            {
                const top = vm.popObject();
                console.log(`Print: ${valueToString(top)}`);
                break;
            }
            default:
            {
                console.error(`Error! Unknown string run command: ${value}`);
                break;
            }
        }
    }
    else if (isValueNumber(value))
    {
        console.log('The number:', value);
    }
    else if (isValueBoolean(value))
    {
        console.log('The boolean:', value);
    }
    else if (isValueArray(value))
    {
        console.log(`The array command: ${valueToString(value)}`);
        console.log(`- Top array value: ${valueToString(vm.popObject())}`);
    }
    else if (isValueObject(value))
    {
        console.log(`The object command: ${valueToString(value)}`);
    }
}

const inputJson: InputScope[] = JSON.parse(fs.readFileSync('../examples/testRunCommands.json', 'utf-8'));
const scopes = parseScopes(inputJson);

const vm = new VirtualMachine(64, runHandler);
vm.addScopes(scopes);

vm.run('Main');