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