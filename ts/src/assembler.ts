import { readAllTokens, tokenize } from "./parser";
import Scope from "./scope";
import Script from "./script";
import ArrayValue from "./values/arrayValue";
import BuiltinFunctionValue from "./values/builtinFunctionValue";
import FunctionValue from "./values/functionValue";
import { IFunctionValue, isIArrayValue, isIFunctionValue, IValue } from "./values/ivalues";
import NumberValue from "./values/numberValue";
import StringValue from "./values/stringValue";
import { getProperty } from "./values/valuePropertyAccess";
import VariableValue from "./values/variableValue";
import { CodeLine, Operator } from "./virtualMachine";
import VMFunction from "./vmFunction";

interface TempCodeLine
{
    readonly label?: string;
    readonly operator?: Operator;
    readonly value?: IValue;
}

interface LoopLabels
{
    readonly start: string;
    readonly end: string;
}

interface PropertyRequestInfo
{
    readonly isPropertyRequest: boolean;
    readonly parentKey: string;
    readonly property: ArrayValue;
}

const FunctionKeyword = 'function';
const LoopKeyword = 'loop';
const ContinueKeyword = 'continue';
const BreakKeyword = 'break';
const IfKeyword = 'if';
const UnlessKeyword = 'unless';
const SetKeyword = 'set';
const DefineKeyword = 'define';
const IncKeyword = 'inc';
const DecKeyword = 'dec';
const JumpKeyword = 'jump';
const ReturnKeyword = 'return';

const incNumber = new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(args.getNumber(0) + 1);
});
const decNumber = new BuiltinFunctionValue((vm, args) =>
{
    vm.pushStackNumber(args.getNumber(0) - 1);
});

function codeLine(operator: Operator, value?: IValue): TempCodeLine
{
    return { operator, value };
}

function labelLine(label: string): TempCodeLine
{
    return { label };
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

        const code = this.parseGlobalFunction(parsed);
        const scriptScope = new Scope();
        scriptScope.combineScope(this.builtinScope);

        return new Script(scriptScope, code);
    }

    public parse(input: IValue): TempCodeLine[]
    {
        if (input instanceof ArrayValue)
        {
            if (input.value.length === 0)
            {
                return [];
            }

            const first = input.value[0];
            // If the first item in an array is a variable we assume that it is a function call or a label
            if (first instanceof VariableValue)
            {
                const firstString = first.toString();
                if (first.isLabel)
                {
                    return [ labelLine(firstString) ];
                }

                // Check for keywords
                const keywordParse = this.parseKeyword(firstString, input);
                if (keywordParse.length > 0)
                {
                    return keywordParse;
                }

                // Handle general opcode or function call.
                let result = input.value.slice(1).map(v => this.parse(v)).flat(1);
                result = result.concat(this.optimiseCallSymbolValue(first.value, input.value.length - 1));

                return result;
            }

            // Any array that doesn't start with a variable we assume it's a data array.
        }
        else if (input instanceof VariableValue)
        {
            if (!input.isLabel)
            {
                return this.optimiseGetSymbolValue(input.value);
            }
        }

        return [ codeLine('push', input) ];
    }

    public parseSet(input: ArrayValue)
    {
        let result = this.parse(input.value[2]);
        result.push(codeLine('set', input.value[1]));
        return result;
    }

    public parseDefine(input: ArrayValue)
    {
        let result = this.parse(input.value[2]);
        result.push(codeLine('define', input.value[1]));

        if (result[0].value !== undefined && result[0].value instanceof FunctionValue)
        {
            result[0].value.value.name = input.value[1].toString();
        }

        return result;
    }

    public parseLoop(input: ArrayValue)
    {
        if (input.value.length < 3)
        {
            throw new Error('Loop input has too few arguments');
        }

        const loopLabelNum = this.labelCount++;
        const labelStart = `:LoopStart${loopLabelNum}`;
        const labelEnd = `:LoopEnd${loopLabelNum}`;

        this.loopStack.push({ start: labelStart, end: labelEnd });

        const comparisonCall = input.value[1];
        let result = [ labelLine(labelStart), ...this.parse(comparisonCall) ];
        result.push(codeLine('jumpFalse', new StringValue(labelEnd)));
        for (let i = 2; i < input.value.length; i++)
        {
            result = result.concat(this.parse(input.value[i]));
        }

        result.push(codeLine('jump', new StringValue(labelStart)));
        result.push(labelLine(labelEnd));

        this.loopStack.pop();

        return result;
    }

    public parseCond(input: ArrayValue, isIfStatement: boolean)
    {
        if (input.value.length < 3)
        {
            throw new Error('Condition input has too few inputs');
        }
        if (input.value.length > 4)
        {
            throw new Error('Condition input has too many inputs!');
        }

        const ifLabelNum = this.labelCount++;
        const labelElse = `:CondElse${ifLabelNum}`;
        const labelEnd = `:CondEnd${ifLabelNum}`;

        const hasElseCall = input.value.length === 4;
        const jumpOperator = isIfStatement ? 'jumpFalse' : 'jumpTrue';

        const comparisonCall = input.value[1];
        const firstBlock = input.value[2] as ArrayValue;

        let result = this.parse(comparisonCall);

        if (hasElseCall)
        {
            // Jump to else if the condition doesn't match
            result.push(codeLine(jumpOperator, new StringValue(labelElse)));

            // First block of code
            result = result.concat(this.parseFlatten(firstBlock));
            // Jump after the condition, skipping second block of code.
            result.push(codeLine('jump', new StringValue(labelEnd)));

            // Jump target for else
            result.push(labelLine(labelElse));

            // Second 'else' block of code
            const secondBlock = input.value[3] as ArrayValue;
            result = result.concat(this.parseFlatten(secondBlock));
        }
        else
        {
            // We only have one block, so jump to the end of the block if the condition doesn't match
            result.push(codeLine(jumpOperator, new StringValue(labelEnd)));

            result = result.concat(this.parseFlatten(firstBlock));
        }

        result.push(labelLine(labelEnd));

        return result;
    }

    public parseFlatten(input: ArrayValue)
    {
        if (input.value.every(isIArrayValue))
        {
            return input.value.map(v => this.parse(v)).flat(1);
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
        return [codeLine('jump', new StringValue(jumpToStart ? loopLabel.start : loopLabel.end))];
    }

    public parseFunction(input: ArrayValue)
    {
        if (!isIArrayValue(input.value[1]))
        {
            throw new Error('Function needs parameter array');
        }

        const parameters = input.value[1].arrayValues().map(e => e.toString());
        const tempCodeLines = input.value.slice(2).map(v => this.parse(v)).flat(1);

        return this.processTempFunction(parameters, tempCodeLines);
    }

    public parseGlobalFunction(input: ArrayValue)
    {
        const tempCodeLines = input.value.map(v => this.parse(v)).flat(1);
        var result = this.processTempFunction([], tempCodeLines);
        result.name = 'global';
        return result;
    }

    public processTempFunction(parameters: string[], tempCodeLines: TempCodeLine[]) : VMFunction
    {
        const labels: { [label: string]: number } = {}
        const code: CodeLine[] = [];

        for (const tempLine of tempCodeLines)
        {
            if (tempLine.label !== undefined && tempLine.label != '')
            {
                labels[tempLine.label] = code.length;
            }
            else if (tempLine.operator !== undefined)
            {
                code.push({ operator: tempLine.operator, value: tempLine.value });
            }
        }

        return new VMFunction(code, parameters, labels);
    }

    public parseChangeVariable(input: IValue, changeFunc: IFunctionValue)
    {
        const varName = new StringValue(input.toString());
        return [
            codeLine('get', varName),
            codeLine('callDirect', new ArrayValue([changeFunc, new NumberValue(1)])),
            codeLine('set', varName)
        ];
    }

    public parseJump(input: ArrayValue)
    {
        let parse = this.parse(input.value[1]);
        if (parse.length === 1 && parse[0].operator === 'push' && parse[0].value !== undefined)
        {
            return [codeLine('jump', parse[0].value)];
        }
        parse.push(codeLine('push'));
        return parse;
    }

    public parseReturn(input: ArrayValue)
    {
        let result = input.value.slice(1).map(v => this.parse(v)).flat(1);
        result.push(codeLine('return'));
        return result;
    }

    public parseKeyword(input: string, arrayValue: ArrayValue): TempCodeLine[]
    {
        switch (input)
        {
            case FunctionKeyword:
                {
                    const func = this.parseFunction(arrayValue);
                    const funcValue = new FunctionValue(func);
                    return [codeLine('push', funcValue)];
                }
            case ContinueKeyword: return this.parseLoopJump(ContinueKeyword, true);
            case BreakKeyword: return this.parseLoopJump(BreakKeyword, false);
            case SetKeyword: return this.parseSet(arrayValue);
            case DefineKeyword: return this.parseDefine(arrayValue);
            case LoopKeyword: return this.parseLoop(arrayValue);
            case IfKeyword: return this.parseCond(arrayValue, true);
            case UnlessKeyword: return this.parseCond(arrayValue, false);
            case IncKeyword: return this.parseChangeVariable(arrayValue.value[1], incNumber);
            case DecKeyword: return this.parseChangeVariable(arrayValue.value[1], decNumber);
            case JumpKeyword: return this.parseJump(arrayValue);
            case ReturnKeyword: return this.parseReturn(arrayValue);
        }

        return [];
    }

    private optimiseCallSymbolValue(input: string, numArgs: number): TempCodeLine[]
    {
        const numArgsValue = new NumberValue(numArgs);
        const propertyRequestInfo = VirtualMachineAssembler.isGetPropertyRequest(input);

        // Check if we know about the parent object? (eg: string.length, the parent is the string object)
        const foundParent = this.builtinScope.get(propertyRequestInfo.parentKey);
        if (foundParent !== undefined)
        {
            // If the get is for a property? (eg: string.length, length is the property)
            let foundProperty: IValue | undefined = undefined;
            if (propertyRequestInfo.isPropertyRequest && (foundProperty = getProperty(foundParent, propertyRequestInfo.property)) !== undefined)
            {
                if (isIFunctionValue(foundProperty))
                {
                    // If we found the property then we're done and we can just push that known value onto the stack.
                    const callValue = new ArrayValue([foundProperty, numArgsValue]);
                    return [codeLine('callDirect', callValue)];
                }

                throw new Error(`Attempting to call a value that is not a function: ${input} = ${foundProperty.toString()}`);
            }
            else if (!propertyRequestInfo.isPropertyRequest)
            {
                // This was not a property request but we found the parent so just push onto the stack.
                if (isIFunctionValue(foundParent))
                {
                    const callValue = new ArrayValue([foundParent, numArgsValue]);
                    return [codeLine('callDirect', callValue)];
                }

                throw new Error(`Attempting to call a value that is not a function: ${input} = ${foundParent.toString()}`);
            }
        }

        // Could not find the parent right now, so look for the parent at runtime.
        const result: TempCodeLine[] = [codeLine('get', new StringValue(propertyRequestInfo.parentKey))];

        // If this was also a property check also look up the property at runtime.
        if (propertyRequestInfo.isPropertyRequest)
        {
            result.push(codeLine('getProperty', propertyRequestInfo.property));
        }

        result.push(codeLine('call', numArgsValue));
        return result;
    }

    private optimiseGetSymbolValue(input: string): TempCodeLine[]
    {
        let isArgumentUnpack = false;
        if (input.startsWith('...'))
        {
            isArgumentUnpack = true;
            input = input.substring(3);
        }

        const result: TempCodeLine[] = [];

        const propertyRequestInfo = VirtualMachineAssembler.isGetPropertyRequest(input);
        const foundParent = this.builtinScope.get(propertyRequestInfo.parentKey);
        if (foundParent !== undefined)
        {
            if (propertyRequestInfo.isPropertyRequest)
            {
                const foundProperty = getProperty(foundParent, propertyRequestInfo.property);
                if (foundProperty !== undefined)
                {
                    result.push(codeLine('push', foundProperty));
                }
                else
                {
                    result.push(codeLine('push', foundParent));
                    result.push(codeLine('getProperty', propertyRequestInfo.property));
                }
            }
            else if (!propertyRequestInfo.isPropertyRequest)
            {
                result.push(codeLine('push', foundParent));
            }
        }
        else
        {
            result.push(codeLine('get', new StringValue(propertyRequestInfo.parentKey)));

            if (propertyRequestInfo.isPropertyRequest)
            {
                result.push(codeLine('getProperty', propertyRequestInfo.property));
            }
        }

        if (isArgumentUnpack)
        {
            result.push(codeLine('toArgument', undefined));
        }

        return result;
    }

    private static isGetPropertyRequest(input: string): PropertyRequestInfo
    {
        if (input.includes('.'))
        {
            const split = input.split('.');
            const parentKey = split[0];
            const property = new ArrayValue(split.slice(1).map(c => new VariableValue(c)));
            return {
                isPropertyRequest: true, parentKey, property
            }
        }

        return {
            isPropertyRequest: false, parentKey: input, property: ArrayValue.Empty
        }
    }
}