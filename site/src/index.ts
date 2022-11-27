import { VirtualMachine } from 'lysithea-vm/src/virtualMachine';
import { VirtualMachineAssembler } from 'lysithea-vm/src/assembler';
import { addToScope, LibraryType } from 'lysithea-vm/src/standardLibrary';
import { Scope } from 'lysithea-vm/src/scope';

interface CodeContext
{
    readonly output: HTMLElement;
    readonly text: string;
}

function getTextFor(codeId: string) : CodeContext | false
{
    const textEl = document.getElementById(codeId) as HTMLTextAreaElement;
    const output = document.getElementById(codeId + '_output');
    if (textEl == null || output == null)
    {
        alert('Broken code example, unable to find code id: ' + codeId);
        return false;
    }

    const text = textEl?.value;
    if (!text)
    {
        output.innerHTML = 'Needs text input to parse';
        return false;
    }
    output.innerHTML = '';

    return { output, text };
}

function run(codeId: string)
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

(globalThis as any).run = run;

const pageSetupScope = new Scope();
pageSetupScope.defineFunc("setBackground", (vm, args) =>
{
    document.body.setAttribute('background', args.getString(0));
    vm.pushStackString('Setting background to: ' + args.getString(0));
});
pageSetupScope.defineFunc("setTheme", (vm, args) =>
{
    document.body.setAttribute('theme', args.getString(0));
    vm.pushStackString('Setting theme to: ' + args.getString(0));
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