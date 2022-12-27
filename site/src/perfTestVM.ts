import { getTextFor, tryAssemble, tryExecute } from './common';

import { VirtualMachine, Assembler, Scope } from 'lysithea-vm';

const perfTestScope = new Scope();
perfTestScope.tryDefineFunc("rand", (vm, args) =>
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

    const assembler = new Assembler();
    assembler.builtinScope.combineScope(perfTestScope);
    assembler.builtinScope.tryDefineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        codeContext.output.innerHTML += text + '<br/>';
    });

    const script = tryAssemble(assembler, 'codePerfTest', codeContext.text);
    if (typeof(script) === 'string')
    {
        codeContext.output.innerHTML = script;
        return;
    }

    const vm = new VirtualMachine(16);

    const before = Date.now();
    const result = tryExecute(script, vm);
    if (result !== true)
    {
        codeContext.output.innerHTML = result;
    }
    const after = Date.now();
    codeContext.output.innerHTML += `Time taken: ${after - before}ms <br/>`;
}

(globalThis as any).runPerfTest = runPerfTest;