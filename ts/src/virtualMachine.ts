import { operatorToString } from "./assembler";
import Scope, { IReadOnlyScope } from "./scope";
import Script from "./script";
import { Operator, ScopeFrame } from "./types";
import ArgumentsValue from "./values/argumentsValue";
import BoolValue from "./values/boolValue";
import { IFunctionValue, isIArrayValue, isIFunctionValue, IValue } from "./values/ivalues";
import NumberValue from "./values/numberValue";
import StringValue from "./values/stringValue";
import { getProperty } from "./values/valuePropertyAccess";
import VMFunction from "./vmFunction";

export default class VirtualMachine
{
    public builtinScope: IReadOnlyScope | undefined = undefined;

    private _globalScope: Scope;
    public get globalScope() { return this._globalScope; }
    private _currentScope: Scope;

    private _lineCounter: number = 0;
    public get lineCounter() { return this._lineCounter; }

    public running: boolean = false;
    public paused: boolean = false;
    public currentCode: VMFunction = VMFunction.Empty;

    private _stack: IValue[] = [];
    public get stack(): ReadonlyArray<IValue> { return this._stack; }

    private _stackTrace: ScopeFrame[] = [];
    public get stackTrace(): ReadonlyArray<ScopeFrame> { return this._stackTrace; }

    private readonly _stackSize: number;

    constructor (stackSize: number)
    {
        this._stackSize = stackSize;
        this._globalScope = new Scope();
        this._currentScope = this._globalScope;
    }

    public reset()
    {
        this._globalScope = new Scope();
        this._currentScope = this._globalScope;
        this._lineCounter = 0;
        this._stack = [];
        this._stackTrace = [];
        this.running = false;
        this.paused = false;
    }

    public changeToScript(script: Script)
    {
        this._lineCounter = 0;
        this._stack = [];
        this._stackTrace = [];

        this.builtinScope = script.builtinScope;
        this.currentCode = script.code;

    }

    public step()
    {
        if (this._lineCounter >= this.currentCode.code.length)
        {
            if (!this.tryCallReturn())
            {
                this.running = false;
            }

            return;
        }

        const codeLine = this.currentCode.code[this._lineCounter++];

        switch (codeLine.operator)
        {
            default:
                {
                    throw new Error(`Unknown operator: ${codeLine.operator}`);
                }
            case Operator.Push:
                {
                    if (codeLine.value !== undefined)
                    {
                        this.pushStack(codeLine.value);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Push needs an input`);
                    }
                    break;
                }
            case Operator.ToArgument:
                {
                    const top = codeLine.value ?? this.popStack();
                    if (!(isIArrayValue(top)))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to convert argument value onto stack: ${top.toString()}`);
                    }

                    this.pushStack(new ArgumentsValue(top.arrayValues()));
                    break;
                }
            case Operator.Get:
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!(key instanceof StringValue))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get, input must be a string not: ${key.toString()}`);
                    }

                    const foundValue = this._currentScope.get(key.value);
                    if (foundValue !== undefined)
                    {
                        this.pushStack(foundValue);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get variable: ${key.toString()}`);
                    }
                    break;
                }
            case Operator.GetProperty:
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!isIArrayValue(key))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get property, input needs to be an array: ${key.toString()}`)
                    }

                    const top = this.popStack();
                    const found = getProperty(top, key);
                    if (found !== undefined)
                    {
                        this.pushStack(found);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get property: ${key.toString()}`);
                    }
                    break;
                }
            case Operator.Define:
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    this._currentScope.define(key.toString(), value);
                    break;
                }
            case Operator.Set:
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    if (!this._currentScope.set(key.toString(), value))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to set variable that has not been defined: ${key.toString()} = ${value.toString()}`);
                    }
                    break;
                }
            case Operator.JumpFalse:
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top.compareTo(BoolValue.False) === 0)
                    {
                        this.jump(label.toString());
                    }
                    break;
                }
            case Operator.JumpTrue:
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top.compareTo(BoolValue.True) === 0)
                    {
                        this.jump(label.toString());
                    }
                    break;
                }
            case Operator.Jump:
                {
                    const label = codeLine.value ?? this.popStack();
                    this.jump(label.toString());
                    break;
                }
            case Operator.Return:
                {
                    this.callReturn();
                    break;
                }
            case Operator.Call:
                {
                    if (!(codeLine.value instanceof NumberValue))
                    {
                        throw new Error(`${this.getScopeLine()}: Call needs a num args code line input`);
                    }

                    const numArgs = codeLine.value;
                    const top = this.popStack();
                    if (isIFunctionValue(top))
                    {
                        this.callFunction(top, numArgs.value, true);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Call needs a function to run: ${top.toString()}`);
                    }
                    break;
                }
            case Operator.CallDirect:
                {
                    const funcCall = codeLine.value;
                    if (funcCall == null || !isIArrayValue(funcCall) ||
                        !isIFunctionValue(funcCall.get(0)) ||
                        !(funcCall.get(1) instanceof NumberValue))
                    {
                        throw new Error(`${this.getScopeLine()}: Call direct needs an array of the function and num args code line input`);
                    }

                    this.callFunction(funcCall.get(0) as IFunctionValue, (funcCall.get(1) as NumberValue).value, true);
                    break;
                }
        }
    }

    public tryCallReturn(): boolean
    {
        const scopeFrame = this._stackTrace.pop();
        if (scopeFrame == undefined)
        {
            return false;
        }

        this._currentScope = scopeFrame.scope;
        this._lineCounter = scopeFrame.lineNumber;
        this.currentCode = scopeFrame.function;
        return true;
    }

    public callReturn()
    {
        if (!this.tryCallReturn())
        {
            throw new Error(`${this.getScopeLine()}: Unable to return, call stack empty`);
        }
    }

    public getArgs(numArgs: number): ArgumentsValue
    {
        if (numArgs === 0)
        {
            return ArgumentsValue.Empty;
        }

        let hasArguments = false;
        const args: IValue[] = new Array(numArgs);
        for (let i = 0; i < numArgs; i++)
        {
            const arg = this.popStack();
            if (arg instanceof ArgumentsValue)
            {
                hasArguments = true;
            }
            args[numArgs - i - 1] = arg;
        }

        if (hasArguments)
        {
            let combined: IValue[] = [];
            for (const arg of args)
            {
                if (arg instanceof ArgumentsValue)
                {
                    combined = combined.concat(arg.value);
                }
                else
                {
                    combined.push(arg);
                }
            }
            return new ArgumentsValue(combined);
        }
        return new ArgumentsValue(args);
    }

    public jump(label: string)
    {
        const line = this.currentCode.labels[label];
        if (line == null)
        {
            throw new Error(`${this.getScopeLine()}: Unable to jump to label: ${label}`);
        }

        this._lineCounter = line;
    }

    public callFunction(value: IFunctionValue, numArgs: number, pushToStackTrace: boolean)
    {
        const args = this.getArgs(numArgs);
        value.invoke(this, args, pushToStackTrace);
    }

    public executeFunction(func: VMFunction, args: ArgumentsValue, pushToStackTrace = false)
    {
        if (pushToStackTrace)
        {
            this.pushCurrentToStackTrace();
        }

        this.currentCode = func;
        this._currentScope = new Scope(this._currentScope);
        this._lineCounter = 0;

        const numCalledArgs = Math.min(args.value.length, func.parameters.length);
        for (let i = 0; i < numCalledArgs; i++)
        {
            const argName = func.parameters[i];
            if (argName.startsWith('...'))
            {
                args = args.sublist(i);
                this._currentScope.define(argName.substring(3), args);
                break;
            }
            this._currentScope.define(argName, args.value[i]);
        }
    }

    public pushCurrentToStackTrace()
    {
        this.pushToStackTrace({
            lineNumber: this._lineCounter,
            scope: this._currentScope,
            function: this.currentCode
        });
    }

    public pushToStackTrace(scopeFrame: ScopeFrame)
    {
        if (this._stackTrace.length >= this._stackSize)
        {
            throw new Error(`${this.getScopeLine()}: Unable to push to stack trace, stack is full`);
        }

        this._stackTrace.push(scopeFrame);
    }

    public pushStack(value: IValue)
    {
        if (this._stack.length >= this._stackSize)
        {
            throw new Error(`${this.getScopeLine()}: Unable to push, stack full`);
        }
        this._stack.push(value);
    }

    public pushStackNumber(value: number)
    {
        this.pushStack(new NumberValue(value));
    }

    public pushStackString(value: string)
    {
        this.pushStack(new StringValue(value));
    }

    public pushStackBool(value: boolean)
    {
        this.pushStack(new BoolValue(value));
    }

    public popStack(): IValue
    {
        const result = this._stack.pop();
        if (result === undefined)
        {
            throw new Error(`${this.getScopeLine()}: Popped empty stack`);
        }
        return result;
    }

    public popStackCast<T>(guardCheck: (v: any) => v is T): T
    {
        const top = this.popStack();
        if (guardCheck(top))
        {
            return top as T;
        }

        throw new Error(`${this.getScopeLine()}: Pop stack cast error`);
    }

    public peekStack(): IValue
    {
        if (this._stack.length === 0)
        {
            throw new Error(`${this.getScopeLine()}: Peek empty stack`);
        }

        return this._stack[this._stack.length - 1];
    }

    public peekStackCast<T>(guardCheck: (v: any) => v is T): T
    {
        const top = this.peekStack();
        if (guardCheck(top))
        {
            return top as T;
        }

        throw new Error(`${this.getScopeLine()}: Peek stack cast error`);
    }

    public createStackTrace(): string[]
    {
        const result = [ this.debugScopeLine(this.currentCode, this._lineCounter - 1) ];

        for (let i = this.stackTrace.length - 1; i >= 0; i--)
        {
            const stackFrame = this._stackTrace[i];
            result.push(this.debugScopeLine(stackFrame.function, stackFrame.lineNumber));
        }

        return result;
    }

    public debugScopeLine(func: VMFunction, line: number)
    {
        if (line >= func.code.length)
        {
            return `[${func.name}]:${line - 1}: end of code`;
        }
        if (line < 0)
        {
            return `[${func.name}]:${line - 1}: before start of code`;
        }

        const codeLine = func.code[line];
        const codeLineInput = codeLine.value != null ? codeLine.value.toString() : '<empty>';
        const opString = operatorToString(codeLine.operator);
        return `[${func.name}]:${line - 1}:${opString}: [${codeLineInput}]`;
    }

    private getScopeLine()
    {
        return `${this.currentCode.name}:${this._lineCounter}`;
    }
}