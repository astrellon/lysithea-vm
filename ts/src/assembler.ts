import { readAllTokens, tokenize } from "./parser";
import { Scope } from "./scope";
import { Script } from "./script";
import { ArrayValue } from "./values/arrayValue";
import { FunctionValue } from "./values/functionValue";
import { IArrayValue, IFunctionValue, isIArrayValue, isIFunctionValue, IValue } from "./values/ivalues";
import { NumberValue, isNumberValue } from "./values/numberValue";
import { StringValue } from "./values/stringValue";
import { getProperty } from "./values/valuePropertyAccess";
import { VariableValue } from "./values/variableValue";
import { CodeLine, Operator } from "./virtualMachine";
import { VMFunction } from "./vmFunction";

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
const JumpKeyword = 'jump';
const ReturnKeyword = 'return';

function codeLine(operator: Operator, value?: IValue): TempCodeLine
{
    return { operator, value };
}

function labelLine(label: string): TempCodeLine
{
    return { label };
}

export class VirtualMachineAssembler
{
    public builtinScope: Scope = new Scope();

    private labelCount: number = 0;
    private loopStack: LoopLabels[] = [];
    private keywordParsingStack: string[] = [];

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

                this.keywordParsingStack.push('func-call');

                // Handle general opcode or function call.
                let result = input.value.slice(1).map(v => this.parse(v)).flat(1);
                result = result.concat(this.optimiseCallSymbolValue(first.value, input.value.length - 1));

                this.keywordParsingStack.pop();

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

    public parseDefineSet(input: ArrayValue, isDefine: boolean)
    {
        const opCode: Operator = isDefine ? 'define' : 'set';
        // Parse the last value as the definable/set-able value.
        const result = this.parse(input.value[input.value.length - 1]);

        // Loop over all the middle inputs as the values to set.
        // Multiple variables can be set when a function returns multiple results.
        for (let i = input.value.length - 2; i >= 1; i--)
        {
            result.push(codeLine(opCode, input.value[i]));
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
        let name = '';
        let offset = 0;
        if (input.value[1] instanceof VariableValue || input.value[1] instanceof StringValue)
        {
            name = input.value[1].toString();
            offset = 1;
        }

        if (!isIArrayValue(input.value[1 + offset]))
        {
            throw new Error('Function needs parameter array');
        }

        const parameters = (input.value[1 + offset] as IArrayValue).arrayValues().map(e => e.toString());
        const tempCodeLines = input.value.slice(2 + offset).map(v => this.parse(v)).flat(1);

        return this.processTempFunction(parameters, tempCodeLines, name);
    }

    public parseGlobalFunction(input: ArrayValue)
    {
        const tempCodeLines = input.value.map(v => this.parse(v)).flat(1);
        return this.processTempFunction([], tempCodeLines, 'global');
    }

    public processTempFunction(parameters: string[], tempCodeLines: TempCodeLine[], name: string) : VMFunction
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

        return new VMFunction(code, parameters, labels, name);
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
        const result = input.value.slice(1).map(v => this.parse(v)).flat(1);
        result.push(codeLine('return'));
        return result;
    }

    public parseFunctionKeyword(arrayValue: ArrayValue): TempCodeLine[]
    {
        const func = this.parseFunction(arrayValue);
        const funcValue = new FunctionValue(func);
        const result = [codeLine('push', funcValue)];

        const currentKeyword = this.keywordParsingStack.length > 1 ? this.keywordParsingStack[this.keywordParsingStack.length - 1] : FunctionKeyword;
        if (func.hasName && currentKeyword === FunctionKeyword)
        {
            result.push(codeLine('define', new StringValue(func.name)));
        }

        return result;
    }

    public parseNegative(input: ArrayValue): TempCodeLine[]
    {
        if (input.value.length >= 3)
        {
            return this.parseOperator('-', input);
        }
        else if (input.value.length === 2)
        {
            if (isNumberValue(input.value[1]))
            {
                return [codeLine('push', new NumberValue(-input.value[1].value))];
            }

            const result = this.parse(input.value[1]);
            result.push(codeLine('unaryNegative'));
            return result;
        }
        else
        {
            throw new Error('Negative/Sub operator expects at least 1 input');
        }
    }

    public parseOnePushInput(opCode: Operator, input: ArrayValue): TempCodeLine[]
    {
        if (input.value.length < 2)
        {
            throw new Error(`Expecting at least 1 input for: ${opCode}`);
        }

        let result: TempCodeLine[] = [];
        for (let i = 1; i < input.value.length; i++)
        {
            result = result.concat(this.parse(input.value[i]));
            result.push(codeLine(opCode));
        }
        return result;
    }

    public parseOperator(opCode: Operator, input: ArrayValue): TempCodeLine[]
    {
        if (input.value.length < 3)
        {
            throw new Error(`Expecting at least 3 inputs for: ${opCode}`);
        }

        let result = this.parse(input.value[1]);
        for (let i = 2; i < input.value.length; i++)
        {
            const item = input.value[i];
            if (isNumberValue(item))
            {
                result.push(codeLine(opCode, item));
            }
            else
            {
                result = result.concat(this.parse(item));
                result.push(codeLine(opCode));
            }
        }

        return result;
    }

    public parseOneVariableUpdate(opCode: Operator, input: ArrayValue): TempCodeLine[]
    {
        if (input.value.length < 2)
        {
            throw new Error(`Expecting at least 1 input for: ${opCode}`);
        }

        let result: TempCodeLine[] = [];
        for (let i = 1; i < input.value.length; i++)
        {
            const varName = new StringValue(input.value[i].toString());
            result.push(codeLine(opCode, varName));
        }
        return result;
    }

    public parseStringConcat(input: ArrayValue): TempCodeLine[]
    {
        const result = input.value.slice(1).map(v => this.parse(v)).flat(1);
        result.push(codeLine('stringConcat', new NumberValue(input.value.length - 1)));
        return result;
    }

    public transformAssignmentOperator(arrayValue: ArrayValue): TempCodeLine[]
    {
        let opCode = arrayValue.value[0].toString();
        opCode = opCode.substring(0, opCode.length - 1);

        const varName = arrayValue.value[1].toString();
        const newCode = [...arrayValue.value];
        newCode[0] = new VariableValue(opCode);

        const wrappedCode = [
            new VariableValue('set'),
            new VariableValue(varName),
            new ArrayValue(newCode)
        ];

        return this.parse(new ArrayValue(wrappedCode));
    }

    public parseKeyword(input: string, arrayValue: ArrayValue): TempCodeLine[]
    {
        let result: TempCodeLine[] | null = null;

        this.keywordParsingStack.push(input);
        switch (input)
        {
            // General Operators
            case FunctionKeyword: result = this.parseFunctionKeyword(arrayValue); break;
            case ContinueKeyword: result = this.parseLoopJump(ContinueKeyword, true); break;
            case BreakKeyword: result = this.parseLoopJump(BreakKeyword, false); break;
            case SetKeyword: result = this.parseDefineSet(arrayValue, false); break;
            case DefineKeyword: result = this.parseDefineSet(arrayValue, true); break;
            case LoopKeyword: result = this.parseLoop(arrayValue); break;
            case IfKeyword: result = this.parseCond(arrayValue, true); break;
            case UnlessKeyword: result = this.parseCond(arrayValue, false); break;
            case JumpKeyword: result = this.parseJump(arrayValue); break;
            case ReturnKeyword: result = this.parseReturn(arrayValue); break;

            // Math Operators
            case '+':
            case '*':
            case '/': result = this.parseOperator(input as Operator, arrayValue); break;
            case '-': result = this.parseNegative(arrayValue); break;

            case '++':
            case '--': result = this.parseOneVariableUpdate(input as Operator, arrayValue); break;

            // Comparison Operators
            case '<':
            case '<=':
            case '==':
            case '!=':
            case '>':
            case '>=': result = this.parseOperator(input as Operator, arrayValue); break;

            // Boolean Operators
            case '&&':
            case '||': result = this.parseOperator(input as Operator, arrayValue); break;
            case '!':  result = this.parseOnePushInput('!', arrayValue); break;

            // Misc Operators
            case '$': result = this.parseStringConcat(arrayValue); break;

            // Conjoined Operators
            case '+=':
            case '-=':
            case '*=':
            case '/=':
            case '&&=':
            case '||=':
            case '$=': result = this.transformAssignmentOperator(arrayValue); break;
        }

        this.keywordParsingStack.pop();

        return result != null ? result : [];
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