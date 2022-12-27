import { getTextFor, tryAssemble, tryExecute } from "./common";

import { VirtualMachine, Assembler, LibraryType, addToScope } from 'lysithea-vm';

function runStdLib(codeId: string)
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

    const script = tryAssemble(assembler, codeId, codeContext.text);
    if (typeof(script) === 'string')
    {
        codeContext.output.innerHTML = script;
        return;
    }

    const vm = new VirtualMachine(16);
    const result = tryExecute(script, vm);
    if (result !== true)
    {
        codeContext.output.innerHTML = result;
    }
}

(globalThis as any).runStdLib = runStdLib;