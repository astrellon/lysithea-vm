import {
    VirtualMachine,
    Assembler,
    addToScope,
    LibraryType,
    VirtualMachineError,
    AssemblerError } from '../src/index';

const assembler = new Assembler();
addToScope(assembler.builtinScope, LibraryType.all);

const vm = new VirtualMachine(8);

function tryExecute(vm: VirtualMachine, sourceName: string, text: string)
{
    try
    {
        const script = assembler.parseFromText(sourceName, text);
        vm.execute(script);

        return true;
    }
    catch (err)
    {
        if (err instanceof VirtualMachineError)
        {
            const stackTrace = err.stackTrace.join('\n');
            console.error(`Runtime Error: ${err.message}\n${stackTrace}`);
        }
        else if (err instanceof AssemblerError)
        {
            console.error(`Assembler Error: ${err.message}\n${err.trace}`);
        }
        else
        {
            console.error('Unexpected error: ', err);
        }
        return false;
    }
}

const runtimeErrorExample = `
    (define num1 18)
    (define num2 3)
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'RuntimeErrorExample', runtimeErrorExample))
{
    console.log('Oh no!');
}

const unexpectedBracketExample = `
    (define num1 18)
    (define num2 3))
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'UnexpectedBracket', unexpectedBracketExample))
{
    console.log('Oh no!');
}

const assemblerErrorExample = `
    (const num1 18)
    (define num1 "Redefined")
    (define num2 3)
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'AssemblerErrorExample', assemblerErrorExample))
{
    console.log('Oh no!');
}
