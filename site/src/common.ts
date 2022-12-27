import { Assembler, AssemblerError, ParserError, Script, toStringCodeLocation, VirtualMachine, VirtualMachineError } from "lysithea-vm";

export interface CodeContext
{
    readonly output: HTMLElement;
    readonly text: string;
}

export function getTextFor(codeId: string) : CodeContext | false
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

export function tryAssemble(assembler: Assembler, sourceName: string, text: string)
{
    try
    {
        return assembler.parseFromText(sourceName, text);
    }
    catch (error)
    {
        if (error instanceof ParserError)
        {
            return ('Parser error: ' + error.message +
                '\nat location: ' + toStringCodeLocation(error.location) +
                '\nat token: ' + error.token);
        }
        else if (error instanceof AssemblerError)
        {
            return ('Assembling error: ' + error.message +
                '\nat location: ' + toStringCodeLocation(error.token.location) +
                '\nat token: ' + JSON.stringify(error.token));
        }
        else
        {
            return ('Unknown error: ' + error);
        }
    }
}

export function tryExecute(script: Script, vm: VirtualMachine)
{
    try
    {
        vm.execute(script);
        return true;
    }
    catch (error)
    {
        if (error instanceof VirtualMachineError)
        {
            return ('Runtime error: ' + error.message +
                '\nStack Trace: ' + error.stackTrace.join('\n'));
        }
        else
        {
            return ('Unknown error: ' + error);
        }
    }
}