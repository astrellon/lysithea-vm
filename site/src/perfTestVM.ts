import { getTextFor } from './common';

import { VirtualMachine } from 'lysithea-vm/src/virtualMachine';
import { VirtualMachineAssembler } from 'lysithea-vm/src/assembler';
import { Scope } from 'lysithea-vm/src/scope';

const perfTestScope = new Scope();
perfTestScope.defineFunc("rand", (vm, args) =>
{
    vm.pushStackNumber(Math.random());
});

function runPerfTest()
{
    const codeContext = getTextFor('codePerfTest');
    if (codeContext === false)
    {
        return;
    }

    const assembler = new VirtualMachineAssembler();
    assembler.builtinScope.combineScope(perfTestScope);
    assembler.builtinScope.defineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        codeContext.output.innerHTML += text + '<br/>';
    });

    const script = assembler.parseFromText(codeContext.text as string);
    const vm = new VirtualMachine(16);

    const before = Date.now();
    vm.execute(script);
    const after = Date.now();
    codeContext.output.innerHTML += `Time taken: ${after - before}ms <br/>`;
}

(globalThis as any).runPerfTest = runPerfTest;