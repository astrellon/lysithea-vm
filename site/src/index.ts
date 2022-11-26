import { VirtualMachine } from 'lysithea-vm/src/virtualMachine';
import { VirtualMachineAssembler } from 'lysithea-vm/src/assembler';
import { addToScope, LibraryType } from 'lysithea-vm/src/standardLibrary';

function run(codeId: string)
{
    const textEl = document.getElementById(codeId) as HTMLTextAreaElement;
    const output = document.getElementById(codeId + '_output');
    if (textEl == null || output == null)
    {
        alert('Broken code example, unable to find code id: ' + codeId);
    }

    const inputText = textEl?.value;
    if (!inputText)
    {
        output.innerHTML = 'Needs text input to parse';
        return;
    }
    output.innerHTML = '';

    const assembler = new VirtualMachineAssembler();
    addToScope(assembler.builtinScope, LibraryType.all);

    assembler.builtinScope.defineFunc("print", (vm, args) =>
    {
        const text = args.value.map(c => c.toString()).join('');
        console.log(text);
        output.innerHTML += text + '<br/>';
    });

    const script = assembler.parseFromText(inputText as string);
    const vm = new VirtualMachine(16);
    vm.execute(script);
}

(globalThis as any).run = run;