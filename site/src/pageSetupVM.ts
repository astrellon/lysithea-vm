import { getTextFor, tryAssemble, tryExecute } from './common';
import { randomScope } from './randomLibrary';

import { VirtualMachine, Assembler, Scope, addToScope, LibraryType, AssemblerError, ParserError, VirtualMachineError, toStringCodeLocation, stringScope } from 'lysithea-vm';

const pageSetupScope = new Scope();
pageSetupScope.trySetConstantFunc("setBackground", (vm, args) =>
{
    document.body.setAttribute('background', args.getString(0));
});
pageSetupScope.trySetConstantFunc("setTheme", (vm, args) =>
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

    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);
    assembler.builtinScope.combineScope(pageSetupScope);
    assembler.builtinScope.combineScope(randomScope);
    assembler.builtinScope.tryDefineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        codeContext.output.innerHTML += text + '<br/>';
    });

    const vm = new VirtualMachine(16);
    const script = tryAssemble(assembler, 'codePageSetup', codeContext.text as string);
    if (typeof(script) === 'string')
    {
        codeContext.output.innerHTML = script;
        return;
    }

    const result = tryExecute(script, vm);
    if (result !== true)
    {
        codeContext.output.innerHTML = result;
    }
}

(globalThis as any).runPageSetup = runPageSetup;