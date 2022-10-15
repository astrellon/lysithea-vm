import { readAllTokens, tokenize } from "./parser";
import Scope from "./scope";
import { ArrayValue, CodeLine, FunctionValue, isValueArray, isValueFunction, isValueSymbol, Operator, Value, valueToString, VMFunction } from "./types";

interface TempCodeLine
{
    readonly label?: string;
    readonly operator?: Operator;
    readonly value?: Value;
}

interface LoopLabels
{
    readonly start: string;
    readonly end: string;
}

const FunctionKeyword = 'function';
const LoopKeyword = 'loop';
const ContinueKeyword = 'continue';
const BreakKeyword = 'break';
const IfKeyword = 'if';
const UnlessKeyword = 'unless';
const SetKeyword = 'set';
const DefineKeyword = 'define';

function codeLine(operator: Operator, value?: Value): TempCodeLine
{
    return { operator, value };
}

function labelLine(label: string): TempCodeLine
{
    return { label };
}

function isLabel(input: Symbol)
{
    const str = valueToString(input);
    return str.length > 0 ? str[0] === ':' : false;
}

export default class VirtualMachineAssembler
{
    public builtinScope: Scope = new Scope();

    private labelCount: number = 0;
    private loopStack: LoopLabels[] = [];

    public parseFromText(input: string)
    {
        const tokens = tokenize(input);
        const parsed = readAllTokens(tokens);
        return this.parseGlobalFunction(parsed);
    }

    public parse(input: Value): TempCodeLine[]
    {
        if (isValueArray(input))
        {
            if (input.length === 0)
            {
                return [];
            }

            const first = input[0];
            // If the first item in an array is a symbol we assume that it is a function call or a label
            if (isValueSymbol(first))
            {
                const firstString = valueToString(first);
                if (isLabel(first))
                {
                    return [ labelLine(firstString) ];
                }

                // Check for keywords
                const keywordParse = this.parseKeyword(firstString, input);
                if (keywordParse.length > 0)
                {
                    return keywordParse;
                }

                // Attempt to parse as an op code
                const opCode = this.parseOperator(firstString);
                const isOpCode = opCode !== Operator.Unknown;
                if (isOpCode && this.isJumpCall(opCode))
                {
                    return [ codeLine(opCode, input[1]) ];
                }

                // Handle general opcode or function call.
                let result = input.slice(1).map(v => this.parse(v)).flat(1);

                // If it is not an opcode then it must be a function call
                if (!isOpCode)
                {
                    result = result.concat(this.optimiseCallSymbolValue(first, input.length - 1));
                }
                else if (opCode !== Operator.Push)
                {
                    result.push(codeLine(opCode));
                }

                return result;
            }

            // Any array that doesn't start with a symbol we assume it's a data array.
        }

        if (isValueSymbol(input) && isLabel(input))
        {
            return [ this.optimiseGetSymbolValue(input) ];
        }

        return [ codeLine(Operator.Push, input) ];
    }

    public parseSet(input: ArrayValue)
    {
        let result = this.parse(input[2]);
        result.push(codeLine(Operator.Set, input[1]));
        return result;
    }

    public parseDefine(input: ArrayValue)
    {
        let result = this.parse(input[2]);
        result.push(codeLine(Operator.Define, input[1]));

        if (result[0].value != null && isValueFunction(result[0].value))
        {
            result[0].value.funcValue.name = valueToString(input[1] as Value);
        }

        return result;
    }

    public parseLoop(input: ArrayValue)
    {
        if (input.length < 3)
        {
            throw new Error('Loop input has too few arguments');
        }

        const loopLabelNum = this.labelCount++;
        const labelStart = `:LoopStart${loopLabelNum}`;
        const labelEnd = `:LoopEnd${loopLabelNum}`;

        this.loopStack.push({ start: labelStart, end: labelEnd });

        const comparisonCall = input[1];
        let result = [ labelLine(labelStart), ...this.parse(comparisonCall) ];
        result.push(codeLine(Operator.JumpFalse, labelEnd));
        for (let i = 2; i < input.length; i++)
        {
            result = result.concat(this.parse(input[2]));
        }

        result.push(codeLine(Operator.Jump, labelStart));
        result.push(labelLine(labelEnd));

        this.loopStack.pop();

        return result;
    }

    public parseCond(input: ArrayValue, isIfStatement: boolean)
    {
        if (input.length < 3)
        {
            throw new Error('Condition input has too few inputs');
        }
        if (input.length > 4)
        {
            throw new Error('Condition input has too many inputs!');
        }

        const ifLabelNum = this.labelCount++;
        const labelElse = `:CondElse${ifLabelNum}`;
        const labelEnd = `:CondEnd${ifLabelNum}`;

        const hasElseCall = input.length === 4;
        const jumpOperator = isIfStatement ? Operator.JumpFalse : Operator.JumpTrue;

        const comparisonCall = input[1];
        const firstBlock = input[2] as ArrayValue;

        let result = this.parse(comparisonCall);

        if (hasElseCall)
        {
            // Jump to else if the condition doesn't match
            result.push(codeLine(jumpOperator, labelElse));

            // First block of code
            result = result.concat(this.parseFlatten(firstBlock));
            // Jump after the condition, skipping second block of code.
            result.push(codeLine(Operator.Jump, labelEnd));

            // Jump target for else
            result.push(labelLine(labelElse));

            // Second 'else' block of code
            const secondBlock = input[3] as ArrayValue;
            result = result.concat(this.parseFlatten(secondBlock));
        }
        else
        {
            // We only have one block, so jump to the end of the block if the condition doesn't match
            result.push(codeLine(jumpOperator, labelEnd));

            result = result.concat(this.parseFlatten(firstBlock));
        }

        result.push(labelLine(labelEnd));

        return result;
    }

    public parseFlatten(input: ArrayValue)
    {
        if (input.every(isValueArray))
        {
            return input.map(v => this.parse(v)).flat(1);
        }

        return this.parse(input);
    }

    public parseLoopJump(keyword: string, jumpToStart: boolean)
    {
        if (this.loopStack.length === 0)
        {
            throw new Error(`Unexpected ${keyword} outside of loop`);
        }

        const loopLabel = this.loopStack[this.loopStack.length - 1];
        return [codeLine(Operator.Jump, jumpToStart ? loopLabel.start : loopLabel.end)];
    }

    public parseFunction(input: ArrayValue)
    {
        if (!isValueArray(input[1]))
        {
            throw new Error('Function needs parameter array');
        }

        const parameters = input[1].map(e => valueToString(e));
        const tempCodeLines = input.slice(2).map(v => this.parse(v)).flat(1);

        return this.processTempFunction(parameters, tempCodeLines);
    }

    public parseGlobalFunction(input: ArrayValue)
    {
        const tempCodeLines = input.map(v => this.parse(v)).flat(1);
        return this.processTempFunction([], tempCodeLines);
    }

    public processTempFunction(parameters: string[], tempCodeLines: TempCodeLine[]) : VMFunction
    {
        const labels: { [label: string]: number } = {}
        const code: CodeLine[] = [];

        for (const tempLine of tempCodeLines)
        {
            if (tempLine.label != null && tempLine.label != '')
            {
                labels[tempLine.label] = code.length;
            }
            else if (tempLine.operator != null)
            {
                code.push({ operator: tempLine.operator, value: tempLine.value });
            }
        }

        return { code, parameters, labels, name: 'anonymous' };
    }

    public parseKeyword(input: string, arrayValue: ArrayValue): TempCodeLine[]
    {
        switch (input)
        {
            case FunctionKeyword:
                {
                    const func = this.parseFunction(arrayValue);
                    const funcValue: FunctionValue = { funcValue: func };
                    return [codeLine(Operator.Push, funcValue)];
                }
            case ContinueKeyword: return this.parseLoopJump(ContinueKeyword, true);
            case BreakKeyword: return this.parseLoopJump(BreakKeyword, false);
            case SetKeyword: return this.parseSet(arrayValue);
            case DefineKeyword: return this.parseDefine(arrayValue);
            case LoopKeyword: return this.parseLoop(arrayValue);
            case IfKeyword: return this.parseCond(arrayValue, true);
            case UnlessKeyword: return this.parseCond(arrayValue, false);
        }

        return [];
    }

    public optimiseCallSymbolValue(input: Symbol, numArgs: number)
    {
        const getSymbol = this.getSymbolValue(input);
        const foundValue = this.builtinScope.get(getSymbol);
        if (foundValue != null)
        {
            const callArgs: ArrayValue = [ foundValue, numArgs ];
            return [ codeLine(Operator.CallDirect, callArgs) ];
        }

        return [
            this.parseGet(getSymbol),
            codeLine(Operator.Call, numArgs)
        ];
    }

    public optimiseGetSymbolValue(input: Symbol)
    {
        const foundValue = this.builtinScope.get(input);
        if (foundValue != null)
        {
            return codeLine(Operator.Push, foundValue);
        }

        return this.parseGet(input);
    }

    public parseGet(input: Value)
    {
        const opCode = isValueArray(input) ? Operator.GetProperty : Operator.Get;
        return codeLine(opCode, input);
    }

    public getSymbolValue(input: Symbol): Value
    {
        const str = input.description || '';
        if (str.includes('.'))
        {
            return str.split('.').map(Symbol);
        }

        return str;
    }

    public parseOperator(input: string): Operator
    {
        input = input.toLowerCase();
        switch (input)
        {
            case 'push': return Operator.Push;
            case 'call': return Operator.Call;
            case 'calldirect': return Operator.CallDirect;
            case 'return': return Operator.Return;
            case 'getproperty': return Operator.GetProperty;
            case 'get': return Operator.Get;
            case 'set': return Operator.Set;
            case 'define': return Operator.Define;
            case 'jump': return Operator.Jump;
            case 'jumptrue': return Operator.JumpTrue;
            case 'jumpfalse': return Operator.JumpFalse;
        }

        return Operator.Unknown;
    }

    public isJumpCall(input: Operator)
    {
        return input === Operator.Call || input === Operator.Jump ||
            input === Operator.JumpTrue || input === Operator.JumpFalse;
    }
}