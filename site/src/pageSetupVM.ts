import { getTextFor } from './common';
import { randomScope } from './randomLibrary';

import { VirtualMachine, VirtualMachineAssembler, Scope, addToScope, LibraryType } from 'lysithea-vm';

const pageSetupScope = new Scope();
pageSetupScope.defineFunc("setBackground", (vm, args) =>
{
    document.body.setAttribute('background', args.getString(0));
});
pageSetupScope.defineFunc("setTheme", (vm, args) =>
{
    document.body.setAttribute('theme', args.getString(0));
});

function runPageSetup()
{
    const codeContext = getTextFor('codePageSetup');
    if (codeContext === false)
    {
        return;
    }

    const assembler = new VirtualMachineAssembler();
    addToScope(assembler.builtinScope, LibraryType.all);
    assembler.builtinScope.combineScope(pageSetupScope);
    assembler.builtinScope.combineScope(randomScope);
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

(globalThis as any).runPageSetup = runPageSetup;