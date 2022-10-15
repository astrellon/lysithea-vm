import Scope from "./scope";
import { ArrayValue, CodeLine, isValueArray, isValueFunction, isValueString, Operator, Value, valueToString } from "./types";

export type InputArrayValue = ReadonlyArray<InputDataArg>;
export interface InputObjectValue
{
    readonly [key: string]: InputDataArg;
}
export type InputDataArg = string | boolean | number | InputArrayValue | InputObjectValue;
export type InputDataLine = InputDataArg[];
export interface InputScope
{
    readonly name: string;
    readonly data: ReadonlyArray<InputDataLine>;
}

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

export default class VirtualMachineAssembler
{
    public builtinScope: Scope = new Scope();

    private labelCount: number = 0;
    private loopStack: LoopLabels[] = [];

    public parse(input: Value): TempCodeLine[]
    {
        if (isValueArray(input))
        {

        }


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
            // We only have one block, so jump to the end of the block if the condition doesn't matchc
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
            return input.map(this.parse).flat(1);
        }

        return this.parse(input);
    }
}