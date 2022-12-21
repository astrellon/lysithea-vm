import { AssemblerError } from "../errors/errors";
import { IValue, ArrayValue, NumberValue, isNumberValue, FunctionValue, VariableValue, ObjectValue, ObjectValueMap } from "../index";
import { Scope } from "../scope";
import { Script } from "../script";
import { Editable } from "../standardLibrary/standardObjectLibrary";
import { StringValue } from "../values/stringValue";
import { getProperty } from "../values/valuePropertyAccess";
import { CodeLine, CodeLocation, EmptyCodeLocation, Operator } from "../virtualMachine";
import { VMFunction } from "../vmFunction";
import { Lexer } from "./lexer";
import { Token } from "./token";

interface TempCodeLine
{
    readonly label?: string;
    readonly operator?: Operator;
    readonly value?: Token;
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
const ConstKeyword = 'const';
const JumpKeyword = 'jump';
const ReturnKeyword = 'return';

function codeLine(operator: Operator, value: Token): TempCodeLine
{
    return { operator, value };
}

function labelLine(label: string): TempCodeLine
{
    return { label };
}

const regexSplit = /(\r\n|\n|\r)/g;
function splitInput(input: string)
{
    return input.split(regexSplit);
}

function isTokenExpression(input: Token)
{
    return input.type === 'expression';
}

function isNoOperator(input: TempCodeLine[])
{
    if (input.length === 1)
    {
        return input[0].operator === 'unknown';
    }

    return false;
}

export class Assembler
{
    public builtinScope: Scope = new Scope();

    private labelCount: number = 0;
    private loopStack: LoopLabels[] = [];
    private keywordParsingStack: string[] = [];
    private constScope = new Scope();

    public parseFromText(input: string)
    {
        const split = splitInput(input);
        const parsed = Lexer.readFromLines(split);

        const code = this.parseGlobalFunction(parsed);
        const scriptScope = new Scope();
        scriptScope.combineScope(this.builtinScope);
        scriptScope.combineScope(this.constScope);

        return new Script(scriptScope, code);
    }

    public parse(input: Token): TempCodeLine[]
    {
        if (input.type === 'expression')
        {
            if (input.tokenList.length === 0)
            {
                return [];
            }

            const first = input.tokenList[0];
            // If the first item in an array is a variable we assume that it is a function call or a label
            if (first.value instanceof VariableValue)
            {
                const firstString = first.toString();
                if (first.value.isLabel)
                {
                    return [ labelLine(firstString) ];
                }

                // Check for keywords
                const keywordParse = this.parseKeyword(firstString, input);
                if (keywordParse.length > 0)
                {
                    if (isNoOperator(keywordParse))
                    {
                        return [];
                    }

                    return keywordParse;
                }

                this.keywordParsingStack.push('func-call');

                // Handle general opcode or function call.
                let result = input.tokenList.slice(1).map(v => this.parse(v)).flat(1);
                result = result.concat(this.optimiseCallSymbolValue(first, input.tokenList.length - 1));

                this.keywordParsingStack.pop();

                return result;
            }
            else
            {
                throw new AssemblerError(input, 'Expression needs to start with a function variable');
            }
        }
        else if (input.type === 'list')
        {
            const result: IValue[] = [];

            for (const item of input.tokenList)
            {
                const parsed = this.parse(item);
                if (parsed.length === 0)
                {
                    continue;
                }

                if (parsed.length === 1 && parsed[0].operator === 'push' && parsed[0].value !== undefined)
                {
                    result.push(parsed[0].value.getValue());
                }
                else
                {
                    throw new AssemblerError(parsed[0].value as Token, 'Unexpected token in list literal');
                }
            }

            return [codeLine('push', input.keepLocation(new ArrayValue(result, false)))];
        }
        else if (input.type === 'map')
        {
            const result: Editable<ObjectValueMap> = {};
            for (const key in input.tokenMap)
            {
                const item = input.tokenMap[key];
                const parsed = this.parse(item);
                if (parsed.length === 0)
                {
                    continue;
                }

                if (parsed.length === 1 && parsed[0].operator === 'push' && parsed[0].value !== undefined)
                {
                    result[key] = parsed[0].value.getValue();
                }
                else
                {
                    throw new AssemblerError(parsed[0].value as Token, 'Unexpected token in map literal');
                }
            }

            return [codeLine('push', input.keepLocation(new ObjectValue(result)))];
        }
        else if (input.value instanceof VariableValue)
        {
            if (!input.value.isLabel)
            {
                return this.optimiseGetSymbolValue(input);
            }
        }

        return [ codeLine('push', input) ];
    }

    public parseDefineSet(input: Token, isDefine: boolean)
    {
        const opCode: Operator = isDefine ? 'define' : 'set';
        // Parse the last value as the definable/set-able value.
        const result = this.parse(input.tokenList[input.tokenList.length - 1]);

        // Loop over all the middle inputs as the values to set.
        // Multiple variables can be set when a function returns multiple results.
        for (let i = input.tokenList.length - 2; i >= 1; i--)
        {
            const key = input.tokenList[i].getValue().toString();
            if (this.constScope.get(key) !== undefined)
            {
                throw new AssemblerError(input.tokenList[i], `Attempting to ${opCode} a constant: ${key}`);
            }

            result.push(codeLine(opCode, input.tokenList[i]));
        }
        return result;
    }

    public parseConst(input: Token)
    {
        if (input.tokenList.length != 2)
        {
            throw new AssemblerError(input, 'Const requires 2 inputs');
        }

        const result = this.parse(input.tokenList[input.tokenList.length - 1]);
        if (result.length !== 1 || result[0].operator !== 'push' || result[0].value === undefined)
        {
            throw new AssemblerError(input, 'Const value is not a compile time constant');
        }

        const key = input.tokenList[1].getValue().toString();
        if (!this.constScope.trySetConstant(key, result[0].value.getValue()))
        {
            throw new AssemblerError(input, 'Cannot redefine a constant');
        }

        return result;
    }

    public parseLoop(input: Token)
    {
        if (input.tokenList.length < 3)
        {
            throw new AssemblerError(input, 'Loop input has too few arguments');
        }

        const loopLabelNum = this.labelCount++;
        const labelStart = `:LoopStart${loopLabelNum}`;
        const labelEnd = `:LoopEnd${loopLabelNum}`;

        this.loopStack.push({ start: labelStart, end: labelEnd });

        const comparisonCall = input.tokenList[1];
        let result = [ labelLine(labelStart), ...this.parse(comparisonCall) ];
        result.push(codeLine('jumpFalse', comparisonCall.keepLocation(new StringValue(labelEnd))));
        for (let i = 2; i < input.tokenList.length; i++)
        {
            result = result.concat(this.parse(input.tokenList[i]));
        }

        result.push(codeLine('jump', comparisonCall.keepLocation(new StringValue(labelStart))));
        result.push(labelLine(labelEnd));

        this.loopStack.pop();

        return result;
    }

    public parseCond(input: Token, isIfStatement: boolean)
    {
        if (input.tokenList.length < 3)
        {
            throw new AssemblerError(input, 'Condition input has too few inputs');
        }
        if (input.tokenList.length > 4)
        {
            throw new AssemblerError(input, 'Condition input has too many inputs!');
        }

        const ifLabelNum = this.labelCount++;
        const labelElse = `:CondElse${ifLabelNum}`;
        const labelEnd = `:CondEnd${ifLabelNum}`;

        const hasElseCall = input.tokenList.length === 4;
        const jumpOperator = isIfStatement ? 'jumpFalse' : 'jumpTrue';

        const comparisonCall = input.tokenList[1];
        const firstBlock = input.tokenList[2];

        let result = this.parse(comparisonCall);

        if (hasElseCall)
        {
            // Jump to else if the condition doesn't match
            result.push(codeLine(jumpOperator, comparisonCall.keepLocation(new StringValue(labelElse))));

            // First block of code
            result = result.concat(this.parseFlatten(firstBlock));
            // Jump after the condition, skipping second block of code.
            result.push(codeLine('jump', firstBlock.keepLocation(new StringValue(labelEnd))));

            // Jump target for else
            result.push(labelLine(labelElse));

            // Second 'else' block of code
            const secondBlock = input.tokenList[3];
            result = result.concat(this.parseFlatten(secondBlock));
        }
        else
        {
            // We only have one block, so jump to the end of the block if the condition doesn't match
            result.push(codeLine(jumpOperator, comparisonCall.keepLocation(new StringValue(labelEnd))));

            result = result.concat(this.parseFlatten(firstBlock));
        }

        result.push(labelLine(labelEnd));

        return result;
    }

    public parseFlatten(input: Token)
    {
        if (input.tokenList.every(isTokenExpression))
        {
            return input.tokenList.map(v => this.parse(v)).flat(1);
        }

        return this.parse(input);
    }

    public parseLoopJump(token: Token, keyword: string, jumpToStart: boolean)
    {
        if (this.loopStack.length === 0)
        {
            throw new AssemblerError(token, `Unexpected ${keyword} outside of loop`);
        }

        const loopLabel = this.loopStack[this.loopStack.length - 1];
        return [codeLine('jump', token.keepLocation(new StringValue(jumpToStart ? loopLabel.start : loopLabel.end)))];
    }

    public parseFunction(input: Token)
    {
        this.constScope = new Scope(this.constScope);

        let name = '';
        let offset = 0;
        if (input.tokenList[1].value instanceof VariableValue || input.tokenList[1].value instanceof StringValue)
        {
            name = input.tokenList[1].toString();
            offset = 1;
        }

        const parameters = input.tokenList[1 + offset].tokenList.map(e => e.toString());
        const tempCodeLines = input.tokenList.slice(2 + offset).map(v => this.parse(v)).flat(1);

        const result = this.processTempFunction(parameters, tempCodeLines, name);

        if (this.constScope.parent === undefined)
        {
            throw new AssemblerError(input, 'Internal error, const scope parent lost');
        }
        this.constScope = this.constScope.parent;

        return result;
    }

    public parseGlobalFunction(input: Token)
    {
        const tempCodeLines = input.tokenList.map(v => this.parse(v)).flat(1);
        return this.processTempFunction([], tempCodeLines, 'global');
    }

    public processTempFunction(parameters: string[], tempCodeLines: TempCodeLine[], name: string) : VMFunction
    {
        const labels: { [label: string]: number } = {}
        const code: CodeLine[] = [];
        const locations: CodeLocation[] = [];

        for (const tempLine of tempCodeLines)
        {
            if (tempLine.label !== undefined && tempLine.label !== '')
            {
                labels[tempLine.label] = code.length;
            }
            else if (tempLine.operator !== undefined && tempLine.value !== undefined)
            {
                locations.push(tempLine.value.location);
                code.push({ operator: tempLine.operator, value: tempLine.value.getValueCanBeEmpty() });
            }
        }

        return new VMFunction(code, parameters, labels, name);
    }

    public parseJump(input: Token)
    {
        let parse = this.parse(input.tokenList[1]);
        if (parse.length === 1 && parse[0].operator === 'push' && parse[0].value !== undefined)
        {
            return [codeLine('jump', parse[0].value)];
        }
        parse.push(codeLine('push', input.toEmpty()));
        return parse;
    }

    public parseReturn(input: Token)
    {
        const result = input.tokenList.slice(1).map(v => this.parse(v)).flat(1);
        result.push(codeLine('return', input.toEmpty()));
        return result;
    }

    public parseFunctionKeyword(arrayValue: Token): TempCodeLine[]
    {
        const func = this.parseFunction(arrayValue);
        const funcValue = new FunctionValue(func);
        const funcToken = arrayValue.keepLocation(funcValue);

        if (this.keywordParsingStack.length === 1 && func.hasName)
        {
            this.constScope.trySetConstant(func.name, funcValue);
            // Special return case
            return [codeLine('unknown', Token.empty(EmptyCodeLocation))];
        }

        const result = [codeLine('push', funcToken)];
        const currentKeyword = this.keywordParsingStack.length > 1 ? this.keywordParsingStack[this.keywordParsingStack.length - 1] : FunctionKeyword;
        if (func.hasName && currentKeyword === FunctionKeyword)
        {
            result.push(codeLine('define', arrayValue.keepLocation(new StringValue(func.name))));
        }

        return result;
    }

    public parseNegative(input: Token): TempCodeLine[]
    {
        if (input.tokenList.length >= 3)
        {
            return this.parseOperator('-', input);
        }
        else if (input.tokenList.length === 2)
        {
            const firstToken = input.tokenList[1];
            if (isNumberValue(firstToken.value))
            {
                return [codeLine('push', firstToken.keepLocation(new NumberValue(-firstToken.value.value)))];
            }

            const result = this.parse(firstToken);
            result.push(codeLine('unaryNegative', firstToken.toEmpty()));
            return result;
        }
        else
        {
            throw new AssemblerError(input, 'Negative/Sub operator expects at least 1 input');
        }
    }

    public parseOnePushInput(opCode: Operator, input: Token): TempCodeLine[]
    {
        if (input.tokenList.length < 2)
        {
            throw new AssemblerError(input, `Expecting at least 1 input for: ${opCode}`);
        }

        let result: TempCodeLine[] = [];
        for (let i = 1; i < input.tokenList.length; i++)
        {
            result = result.concat(this.parse(input.tokenList[i]));
            result.push(codeLine(opCode, input.tokenList[i].toEmpty()));
        }
        return result;
    }

    public parseOperator(opCode: Operator, input: Token): TempCodeLine[]
    {
        if (input.tokenList.length < 3)
        {
            throw new AssemblerError(input, `Expecting at least 3 inputs for: ${opCode}`);
        }

        let result = this.parse(input.tokenList[1]);
        for (let i = 2; i < input.tokenList.length; i++)
        {
            const item = input.tokenList[i];
            if (isNumberValue(item.value))
            {
                result.push(codeLine(opCode, item));
            }
            else
            {
                result = result.concat(this.parse(item));
                result.push(codeLine(opCode, input.toEmpty()));
            }
        }

        return result;
    }

    public parseOneVariableUpdate(opCode: Operator, input: Token): TempCodeLine[]
    {
        if (input.tokenList.length < 2)
        {
            throw new AssemblerError(input, `Expecting at least 1 input for: ${opCode}`);
        }

        let result: TempCodeLine[] = [];
        for (let i = 1; i < input.tokenList.length; i++)
        {
            const varName = new StringValue(input.tokenList[i].getValue().toString());
            result.push(codeLine(opCode, input.tokenList[i].keepLocation(varName)));
        }
        return result;
    }

    public parseStringConcat(input: Token): TempCodeLine[]
    {
        const result = input.tokenList.slice(1).map(v => this.parse(v)).flat(1);
        result.push(codeLine('stringConcat', input.keepLocation(new NumberValue(input.tokenList.length - 1))));
        return result;
    }

    public transformAssignmentOperator(arrayValue: Token): TempCodeLine[]
    {
        let opCode = arrayValue.tokenList[0].getValue().toString();
        opCode = opCode.substring(0, opCode.length - 1);

        const varName = arrayValue.tokenList[1].getValue().toString();
        const newCode = [...arrayValue.tokenList];
        newCode[0] = arrayValue.tokenList[0].keepLocation(new VariableValue(opCode));

        const wrappedCode = [
            arrayValue.keepLocation(new VariableValue('set')),
            arrayValue.keepLocation(new VariableValue(varName)),
            Token.expression(arrayValue.location, newCode)
        ];

        return this.parse(Token.expression(arrayValue.location, wrappedCode));
    }

    public parseKeyword(input: string, arrayValue: Token): TempCodeLine[]
    {
        let result: TempCodeLine[] | null = null;

        this.keywordParsingStack.push(input);
        switch (input)
        {
            // General Operators
            case FunctionKeyword: result = this.parseFunctionKeyword(arrayValue); break;
            case ContinueKeyword: result = this.parseLoopJump(arrayValue, ContinueKeyword, true); break;
            case BreakKeyword: result = this.parseLoopJump(arrayValue, BreakKeyword, false); break;
            case SetKeyword: result = this.parseDefineSet(arrayValue, false); break;
            case DefineKeyword: result = this.parseDefineSet(arrayValue, true); break;
            case ConstKeyword: result = this.parseConst(arrayValue); break;
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

    private optimiseCallSymbolValue(input: Token, numArgs: number): TempCodeLine[]
    {
        if (input.type !== 'value')
        {
            throw new AssemblerError(input, 'Call token must be a value');
        }

        const numArgsValue = new NumberValue(numArgs);

        const result = this.optimiseGet(input, input.getValue().toString());
        if (result.length === 1 && result[0].operator === 'push' && result[0].value !== undefined)
        {
            const callValue = new ArrayValue([result[0].value.getValue(), numArgsValue]);
            return [codeLine('callDirect', input.keepLocation(callValue))];
        }

        result.push(codeLine('call', input.keepLocation(numArgsValue)));
        return result;
    }

    private optimiseGetSymbolValue(input: Token): TempCodeLine[]
    {
        let isArgumentUnpack = false;
        let key = input.getValue().toString();
        if (key.startsWith('...'))
        {
            isArgumentUnpack = true;
            key = key.substring(3);
        }

        const result = this.optimiseGet(input, key)

        if (isArgumentUnpack)
        {
            result.push(codeLine('toArgument', input.toEmpty()));
        }

        return result;
    }

    private optimiseGet(input: Token, key: string)
    {
        if (input.type !== 'value')
        {
            throw new AssemblerError(input, 'Get symbol token must be a value');
        }

        const result: TempCodeLine[] = [];

        const foundConst = this.constScope.get(key);
        if (foundConst !== undefined)
        {
            result.push(codeLine('push', input.keepLocation(foundConst)));
            return result;
        }

        const propertyRequestInfo = Assembler.isGetPropertyRequest(key);
        const foundParent = this.builtinScope.get(propertyRequestInfo.parentKey);

        // Check if we know about the parent object? (eg: string.length, the parent is the string object)
        if (foundParent !== undefined)
        {
            // If the get is for a property? (eg: string.length, length is the property)
            if (propertyRequestInfo.isPropertyRequest)
            {
                const foundProperty = getProperty(foundParent, propertyRequestInfo.property);
                if (foundProperty !== undefined)
                {
                    // If we found the property then we're done and we can just push that known value onto the stack.
                    result.push(codeLine('push', input.keepLocation(foundProperty)));
                }
                else
                {
                    // We didn't find the property at compile time, so look it up at run time.
                    result.push(codeLine('push', input.keepLocation(foundParent)));
                    result.push(codeLine('getProperty', input.keepLocation(propertyRequestInfo.property)));
                }
            }
            else if (!propertyRequestInfo.isPropertyRequest)
            {
                // This was not a property request but we found the parent so just push onto the stack.
                result.push(codeLine('push', input.keepLocation(foundParent)));
            }
        }
        else
        {
            result.push(codeLine('get', input.keepLocation(new StringValue(propertyRequestInfo.parentKey))));

            // If this was also a property check also look up the property at runtime.
            if (propertyRequestInfo.isPropertyRequest)
            {
                result.push(codeLine('getProperty', input.keepLocation(propertyRequestInfo.property)));
            }
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