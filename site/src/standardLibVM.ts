import { getTextFor } from "./common";

import { VirtualMachine, VirtualMachineAssembler, LibraryType, addToScope } from 'lysithea-vm';

function runStdLib(codeId: string)
{
    const codeContext = getTextFor(codeId);
    if (codeContext === false)
    {
        return;
    }

    const assembler = new VirtualMachineAssembler();
    addToScope(assembler.builtinScope, LibraryType.all);

    assembler.builtinScope.defineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        codeContext.output.innerHTML += text + '<br/>';
    });

    const script = assembler.parseFromText(codeContext.text as string);
    const vm = new VirtualMachine(16);
    vm.execute(script);
}

(globalThis as any).runStdLib = runStdLib;