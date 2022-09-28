import { isValueNumber, valueCompareTo, valueToString } from "../types";
import VirtualMachine from "../virtualMachine";

export const mathHandleName = 'math';

export function addMathHandler(vm: VirtualMachine)
{
    vm.addRunHandler(mathHandleName, mathHandler)
}

const degToRad = Math.PI / 180.0;

export function mathHandler(command: string, vm: VirtualMachine)
{
    switch (command)
    {
        case 'sin':
            {
                vm.pushStack(Math.sin(vm.popStackNumber()));
                break;
            }
        case 'cos':
            {
                vm.pushStack(Math.cos(vm.popStackNumber()));
                break;
            }
        case 'tan':
            {
                vm.pushStack(Math.tan(vm.popStackNumber()));
                break;
            }
        case 'sinDeg':
            {
                vm.pushStack(Math.sin(degToRad * vm.popStackNumber()));
                break;
            }
        case 'cosDeg':
            {
                vm.pushStack(Math.cos(degToRad * vm.popStackNumber()));
                break;
            }
        case 'tanDeg':
            {
                vm.pushStack(Math.tan(degToRad * vm.popStackNumber()));
                break;
            }
        case 'E':
            {
                vm.pushStack(Math.E);
                break;
            }
        case 'PI':
            {
                vm.pushStack(Math.PI);
                break;
            }
        case 'exp':
            {
                vm.pushStack(Math.exp(vm.popStackNumber()));
                break;
            }
        case 'ceil':
            {
                vm.pushStack(Math.ceil(vm.popStackNumber()));
                break;
            }
        case 'floor':
            {
                vm.pushStack(Math.floor(vm.popStackNumber()));
                break;
            }
        case 'round':
            {
                vm.pushStack(Math.round(vm.popStackNumber()));
                break;
            }
        case 'isFinite':
            {
                vm.pushStack(Number.isFinite(vm.popStackNumber()));
                break;
            }
        case 'isNaN':
            {
                vm.pushStack(isNaN(vm.popStackNumber()));
                break;
            }
        case 'parse':
            {
                let top = vm.peekStack();
                if (isValueNumber(top))
                {
                    break;
                }

                top = vm.popStack();
                const num = parseFloat(valueToString(top));
                vm.pushStack(num);
                break;
            }
        case 'log':
            {
                vm.pushStack(Math.log(vm.popStackNumber()));
                break;
            }
        case 'log2':
            {
                vm.pushStack(Math.log2(vm.popStackNumber()));
                break;
            }
        case 'log10':
            {
                vm.pushStack(Math.log10(vm.popStackNumber()));
                break;
            }
        case 'abs':
            {
                vm.pushStack(Math.abs(vm.popStackNumber()));
                break;
            }
        case 'max':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) > 0 ? left : right);
                break;
            }
        case 'min':
            {
                const right = vm.popStack();
                const left = vm.popStack();
                vm.pushStack(valueCompareTo(left, right) < 0 ? left : right);
                break;
            }
        case '+':
        case 'add':
            {
                const right = vm.popStackNumber();
                const left = vm.popStackNumber();
                vm.pushStack(left + right);
            }
        case '-':
        case 'sub':
            {
                const right = vm.popStackNumber();
                const left = vm.popStackNumber();
                vm.pushStack(left - right);
            }
        case '*':
        case 'mul':
            {
                const right = vm.popStackNumber();
                const left = vm.popStackNumber();
                vm.pushStack(left * right);
            }
        case '/':
        case 'div':
            {
                const right = vm.popStackNumber();
                const left = vm.popStackNumber();
                vm.pushStack(left / right);
            }
    }
}