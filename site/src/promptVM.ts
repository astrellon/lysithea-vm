import { getTextFor, tryAssemble } from "./common";

import { VirtualMachine, Assembler, LibraryType, addToScope, VirtualMachineRunner, NullValue, VirtualMachineError } from 'lysithea-vm';

let runner: VirtualMachineRunner | undefined = undefined;

function runPrompt(codeId: string)
{
    const codeContext = getTextFor(codeId);
    if (codeContext === false)
    {
        return;
    }

    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);

    assembler.builtinScope.tryDefineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        codeContext.output.innerHTML += text + '<br/>';
    });
    assembler.builtinScope.tryDefineFunc("rand", (vm, args) =>
    {
        vm.pushStackNumber(Math.random());
    });
    assembler.builtinScope.tryDefineFunc("wait", (vm, args) =>
    {
        if (runner == undefined)
        {
            alert('Error, no runner found');
            throw new Error('No runner found');
        }

        runner.waitUntil = args.getNumber(0);
    });
    assembler.builtinScope.tryDefineFunc("prompt", (vm, args) =>
    {
        const promptMessage = args.getString(0);
        console.log(promptMessage);
        const userInput = prompt(promptMessage);
        if (userInput === null)
        {
            vm.pushStack(NullValue.Value);
        }
        else
        {
            vm.pushStackString(userInput);
        }
    });

    const script = tryAssemble(assembler, codeId, codeContext.text);
    if (typeof(script) === 'string')
    {
        codeContext.output.innerHTML = script;
        return;
    }

    const vm = new VirtualMachine(16);
    vm.changeToScript(script);
    vm.running = true;

    if (runner != undefined)
    {
        runner.running = false;
    }

    runner = new VirtualMachineRunner(vm);
    runner.start()
        .then(() =>
        {
            console.log('Program done');
        })
        .catch((err) =>
        {
            if (err instanceof VirtualMachineError)
                codeContext.output.innerHTML += err.message + '<br/>' + err.stackTrace.join('<br/>');
        })
}

(globalThis as any).runPrompt = runPrompt;