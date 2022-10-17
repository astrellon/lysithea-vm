import { operatorToString } from "./assembler";
import Scope from "./scope";
import { ArrayValue, BuiltinFunctionValue, EmptyArrayValue, EmptyFunction, VMFunction, FunctionValue, isValueArray, isValueBuiltinFunction, isValueFunction, isValueNumber, Operator, ScopeFrame, Value, valueToString, isValueString } from "./types";

export default class VirtualMachine
{
    private readonly _builtinScope: Scope = new Scope();
    public get builtinScope() { return this._builtinScope; }

    private _globalScope: Scope;
    public get globalScope() { return this._globalScope; }
    private _currentScope: Scope;

    private _lineCounter: number = 0;
    public get programCounter() { return this._lineCounter; }

    public running: boolean = false;
    public paused: boolean = false;
    public currentCode: VMFunction = EmptyFunction;

    private _stack: Value[] = [];
    public get stack(): ReadonlyArray<Value> { return this._stack; }

    private _stackTrace: ScopeFrame[] = [];
    public get stackTrace(): ReadonlyArray<ScopeFrame> { return this._stackTrace; }

    private readonly _stackSize: number;

    constructor (stackSize: number)
    {
        this._stackSize = stackSize;
        this._globalScope = new Scope(this._builtinScope);
        this._currentScope = this._globalScope;
    }

    public reset()
    {
        this._globalScope = new Scope(this._builtinScope);
        this._currentScope = this._globalScope;
        this._lineCounter = 0;
        this._stack = [];
        this._stackTrace = [];
        this.running = false;
        this.paused = false;
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
            case Operator.Get:
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!isValueString(key))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get, input must be a string not: ${valueToString(key)}`);
                    }

                    const foundValue = this._currentScope.getKey(key);
                    if (foundValue !== undefined)
                    {
                        this.pushStack(foundValue);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get variable: ${valueToString(key)}`);
                    }
                    break;
                }
            case Operator.GetProperty:
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!isValueArray(key))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get property, input needs to be an array: ${valueToString(key)}`)
                    }

                    const foundValue = this._currentScope.getProperty(key);
                    if (foundValue !== undefined)
                    {
                        this.pushStack(foundValue);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to get property: ${valueToString(key)}`);
                    }
                    break;
                }
            case Operator.Define:
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    this._currentScope.define(valueToString(key), value);
                    break;
                }
            case Operator.Set:
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    if (!this._currentScope.set(valueToString(key), value))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to set variable that has not been defined: ${valueToString(key)} = ${valueToString(value)}`);
                    }
                    break;
                }
            case Operator.JumpFalse:
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top == false)
                    {
                        this.jump(valueToString(label));
                    }
                    break;
                }
            case Operator.JumpTrue:
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top == true)
                    {
                        this.jump(valueToString(label));
                    }
                    break;
                }
            case Operator.Jump:
                {
                    const label = codeLine.value ?? this.popStack();
                    this.jump(valueToString(label));
                    break;
                }
            case Operator.Return:
                {
                    this.callReturn();
                    break;
                }
            case Operator.Call:
                {
                    if (!isValueNumber(codeLine.value))
                    {
                        throw new Error(`${this.getScopeLine()}: Call needs a num args code line input`);
                    }

                    const numArgs = codeLine.value;
                    const top = this.popStack();
                    if (isValueFunction(top) || isValueBuiltinFunction(top))
                    {
                        this.callFunction(top, numArgs, true);
                    }
                    else
                    {
                        throw new Error(`${this.getScopeLine()}: Call needs a function to run: ${valueToString(top)}`);
                    }
                    break;
                }
            case Operator.CallDirect:
                {
                    const funcCall = codeLine.value;
                    if (funcCall == null || !isValueArray(funcCall) ||
                        !(isValueFunction(funcCall[0]) || isValueBuiltinFunction(funcCall[0])) ||
                        !isValueNumber(funcCall[1]))
                    {
                        throw new Error(`${this.getScopeLine()}: Call direct needs an array of the function and num args code line input`);
                    }

                    this.callFunction(funcCall[0], funcCall[1], true);
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

    public getArgs(numArgs: number): ArrayValue
    {
        if (numArgs === 0)
        {
            return EmptyArrayValue;
        }

        const args: Value[] = new Array(numArgs);
        for (let i = 0; i < numArgs; i++)
        {
            args[numArgs - i - 1] = this.popStack();
        }
        return args as ArrayValue;
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

    public callFunction(value: FunctionValue | BuiltinFunctionValue, numArgs: number, pushToStackTrace: boolean)
    {
        if (isValueFunction(value))
        {
            if (pushToStackTrace)
            {
                this.pushCurrentToStackTrace();
            }
            this.executeFunction(value, numArgs);
        }
        else
        {
            this.executeBuiltinFunction(value, numArgs);
        }
    }

    public executeFunction(value: FunctionValue, numArgs: number = -1)
    {
        this.currentCode = value.funcValue;
        this._currentScope = new Scope(this._currentScope);
        this._lineCounter = 0;

        const params = value.funcValue.parameters;

        const args = this.getArgs(numArgs >= 0 ? Math.min(numArgs, params.length) : params.length);
        for (let i = 0; i < args.length; i++)
        {
            this._currentScope.define(params[i], args[i]);
        }
    }

    public executeBuiltinFunction(value: BuiltinFunctionValue, numArgs: number)
    {
        value(this, numArgs);
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

    public pushStack(value: Value)
    {
        if (this._stack.length >= this._stackSize)
        {
            throw new Error(`${this.getScopeLine()}: Unable to push, stack full`);
        }
        this._stack.push(value);
    }

    public popStack(): Value
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

    public peekStack(): Value
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
        const codeLineInput = codeLine.value != null ? valueToString(codeLine.value) : '<empty>';
        const opString = operatorToString(codeLine.operator);
        return `[${func.name}]:${line - 1}:${opString}: [${codeLineInput}]`;
    }

    private getScopeLine()
    {
        return `${this.currentCode.name}:${this._lineCounter}`;
    }
}