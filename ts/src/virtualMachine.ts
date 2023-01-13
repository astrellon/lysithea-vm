import { createErrorLogAt, VirtualMachineError } from "./errors/errors";
import { Scope, IReadOnlyScope } from "./scope";
import { Script } from "./script";
import { sublist } from "./standardLibrary/standardArrayLibrary";
import { ArrayValue } from "./values/arrayValue";
import { BoolValue, isBoolValue } from "./values/boolValue";
import { IFunctionValue, isIArrayValue, isIFunctionValue, IValue } from "./values/ivalues";
import { NumberValue, isNumberValue } from "./values/numberValue";
import { StringValue } from "./values/stringValue";
import { getProperty } from "./values/valuePropertyAccess";
import { VMFunction } from "./vmFunction";

export type Operator = 'unknown' |

    // General
    'push' | 'toArgument' |
    'call' | 'callDirect' | 'return' |
    'getProperty' | 'get' | 'set' | 'define' |
    'jump' | 'jumpTrue' | 'jumpFalse' |

    // Misc
    'stringConcat' |

    // Comparison
    '>' | '>=' |
    '==' | '!=' |
    '<' | '<=' |

    // Boolean
    '!' | '&&' | '||' |

    // Math
    '+' | '-' | '*' | '/' |
    '++' | '--' | 'unaryNegative'
    ;

export interface CodeLine
{
    readonly operator: Operator;
    readonly value?: IValue;
}

export interface ScopeFrame
{
    readonly lineNumber: number;
    readonly function: VMFunction;
    readonly scope: Scope;
}

export interface CodeLocation
{
    readonly startLineNumber: number;
    readonly startColumnNumber: number;
    readonly endLineNumber: number;
    readonly endColumnNumber: number;
}
export const EmptyCodeLocation: CodeLocation = {
    startLineNumber: 0,
    startColumnNumber: 0,
    endLineNumber: 0,
    endColumnNumber: 0
}
export function toStringCodeLocation(input: CodeLocation)
{
    return `${input.startLineNumber}:${input.startColumnNumber} -> ${input.endLineNumber}:${input.endColumnNumber}`;
}

export class VirtualMachine
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

    public execute(script: Script)
    {
        this.changeToScript(script);

        this.running = true;
        this.paused = false;
        while (this.running && !this.paused)
        {
            this.step();
        }
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
                    throw new VirtualMachineError(this.createStackTrace(), `Unknown operator: ${codeLine.operator}`);
                }

            // General Operators
            case 'push':
                {
                    if (codeLine.value !== undefined)
                    {
                        this.pushStack(codeLine.value);
                    }
                    else
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Push needs an input`);
                    }
                    break;
                }
            case 'toArgument':
                {
                    const top = codeLine.value ?? this.popStack();
                    if (!(isIArrayValue(top)))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to convert argument value onto stack: ${top.toString()}`);
                    }

                    this.pushStack(new ArrayValue(top.arrayValues(), true));
                    break;
                }
            case 'get':
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!(key instanceof StringValue))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to get, input must be a string not: ${key.toString()}`);
                    }

                    let foundValue = this._currentScope.get(key.value);
                    if (foundValue !== undefined)
                    {
                        this.pushStack(foundValue);
                        break;
                    }
                    else if (this.builtinScope !== undefined)
                    {
                        foundValue = this.builtinScope.get(key.value);
                        if (foundValue !== undefined)
                        {
                            this.pushStack(foundValue);
                            break;
                        }
                    }
                    throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to get variable: ${key.toString()}`);
                }
            case 'getProperty':
                {
                    const key = codeLine.value ?? this.popStack();
                    if (!isIArrayValue(key))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to get property, input needs to be an array: ${key.toString()}`)
                    }

                    const top = this.popStack();
                    const found = getProperty(top, key);
                    if (found !== undefined)
                    {
                        this.pushStack(found);
                    }
                    else
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to get property: ${key.toString()}`);
                    }
                    break;
                }
            case 'define':
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    this._currentScope.tryDefine(key.toString(), value);
                    break;
                }
            case 'set':
                {
                    const key = codeLine.value ?? this.popStack();
                    const value = this.popStack();
                    if (!this._currentScope.trySet(key.toString(), value))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to set variable that has not been defined: ${key.toString()} = ${value.toString()}`);
                    }
                    break;
                }
            case 'jumpFalse':
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top.compareTo(BoolValue.False) === 0)
                    {
                        this.jump(label.toString());
                    }
                    break;
                }
            case 'jumpTrue':
                {
                    const label = codeLine.value ?? this.popStack();
                    const top = this.popStack();
                    if (top.compareTo(BoolValue.True) === 0)
                    {
                        this.jump(label.toString());
                    }
                    break;
                }
            case 'jump':
                {
                    const label = codeLine.value ?? this.popStack();
                    this.jump(label.toString());
                    break;
                }
            case 'return':
                {
                    this.callReturn();
                    break;
                }
            case 'call':
                {
                    if (!(codeLine.value instanceof NumberValue))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Call needs a num args code line input`);
                    }

                    const numArgs = codeLine.value;
                    const top = this.popStack();
                    if (isIFunctionValue(top))
                    {
                        this.callFunction(top, numArgs.value, true);
                    }
                    else
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Call needs a function to run: ${top.toString()}`);
                    }
                    break;
                }
            case 'callDirect':
                {
                    const funcCall = codeLine.value;
                    if (funcCall == null || !isIArrayValue(funcCall) ||
                        !isIFunctionValue(funcCall.tryGetIndex(0)) ||
                        !(funcCall.tryGetIndex(1) instanceof NumberValue))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Call direct needs an array of the function and num args code line input`);
                    }

                    this.callFunction(funcCall.tryGetIndex(0) as IFunctionValue, (funcCall.tryGetIndex(1) as NumberValue).value, true);
                    break;
                }

            // Misc Operators
            case 'stringConcat':
                {
                    if (!isNumberValue(codeLine.value))
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: StringConcat operator needs the number of args to concat`);
                    }

                    const args = this.getArgs(codeLine.value.value);
                    this.pushStackString(args.value.join(''));
                    break;
                }

            // Math Operators
            case '+':
                {
                    this.pushStackNumber(this.getNumArg(codeLine) + this.popStackNumber());
                    break;
                }
            case '-':
                {
                    const right = this.getNumArg(codeLine);
                    const left = this.popStackNumber();
                    this.pushStackNumber(left - right);
                    break;
                }
            case 'unaryNegative':
                {
                    this.pushStackNumber(-this.popStackNumber());
                    break;
                }
            case '*':
                {
                    this.pushStackNumber(this.getNumArg(codeLine) * this.popStackNumber());
                    break;
                }
            case '/':
                {
                    const right = this.getNumArg(codeLine);
                    const left = this.popStackNumber();
                    this.pushStackNumber(left / right);
                    break;
                }
            case '++':
                {
                    if (codeLine.value == undefined)
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Inc operator needs code line variable`);
                    }

                    const key = codeLine.value.toString();
                    const num = this._currentScope.getNumber(key);
                    if (num === undefined)
                    {
                        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Inc operator could not find variable: ${key}`);
                    }
                    this._currentScope.trySet(key, new NumberValue(num + 1));
                    break;
                }
            case '--':
                {
                    if (codeLine.value == undefined)
                    {
                            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Dec operator needs code line variable`);
                        }

                        const key = codeLine.value.toString();
                        const num = this._currentScope.getNumber(key);
                        if (num === undefined)
                        {
                            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Dec operator could not find variable: ${key}`);
                    }
                    this._currentScope.trySet(key, new NumberValue(num - 1));
                    break;
                }

            // Comparison Operators
            case '<':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) < 0);
                    break;
                }
            case '<=':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) <= 0);
                    break;
                }
            case '==':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) == 0);
                    break;
                }
            case '!=':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) != 0);
                    break;
                }
            case '>':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) > 0);
                    break;
                }
            case '>=':
                {
                    const right = codeLine.value ?? this.popStack();
                    const left = this.popStack();
                    this.pushStackBool(left.compareTo(right) >= 0);
                    break;
                }

            // Boolean Operators
            case '&&':
                {
                    this.pushStackBool(this.getBoolArg(codeLine) && this.popStackBool());
                    break;
                }
            case '||':
                {
                    this.pushStackBool(this.getBoolArg(codeLine) || this.popStackBool());
                    break;
                }
            case '!':
                {
                    this.pushStackBool(!this.popStackBool());
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
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to return, call stack empty`);
        }
    }

    public getArgs(numArgs: number): ArrayValue
    {
        if (numArgs === 0)
        {
            return ArrayValue.Empty;
        }

        let hasArguments = false;
        const args: IValue[] = new Array(numArgs);
        for (let i = 0; i < numArgs; i++)
        {
            const arg = this.popStack();
            if (arg instanceof ArrayValue && arg.isArgumentValue)
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
                if (arg instanceof ArrayValue && arg.isArgumentValue)
                {
                    combined = combined.concat(arg.value);
                }
                else
                {
                    combined.push(arg);
                }
            }
            return new ArrayValue(combined, true);
        }
        return new ArrayValue(args, true);
    }

    public getNumArg(codeLine: CodeLine): number
    {
        if (codeLine.value == null)
        {
            return this.popStackNumber();
        }
        if (isNumberValue(codeLine.value))
        {
            return codeLine.value.value;
        }
        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Error attempt to get number argument`);
    }

    public getBoolArg(codeLine: CodeLine): boolean
    {
        if (codeLine.value == null)
        {
            return this.popStackBool();
        }
        if (isBoolValue(codeLine.value))
        {
            return codeLine.value.value;
        }
        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Error attempt to get boolean argument`);
    }

    public jump(label: string)
    {
        const line = this.currentCode.labels[label];
        if (line == null)
        {
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to jump to label: ${label}`);
        }

        this._lineCounter = line;
    }

    public callFunction(value: IFunctionValue, numArgs: number, pushToStackTrace: boolean)
    {
        const args = this.getArgs(numArgs);
        value.invoke(this, args, pushToStackTrace);
    }

    public executeFunction(func: VMFunction, args: ArrayValue, pushToStackTrace = false)
    {
        if (pushToStackTrace)
        {
            this.pushCurrentToStackTrace();
        }

        this.currentCode = func;
        this._currentScope = new Scope(this._currentScope);
        this._lineCounter = 0;

        const numCalledArgs = Math.min(args.value.length, func.parameters.length);
        let i = 0;
        for (; i < numCalledArgs; i++)
        {
            const argName = func.parameters[i];
            if (argName.startsWith('...'))
            {
                args = sublist(args, i, -1);
                this._currentScope.tryDefine(argName.substring(3), args);
                i++;
                break;
            }
            this._currentScope.tryDefine(argName, args.value[i]);
        }

        if (i < func.parameters.length)
        {
            const argName = func.parameters[i];
            if (argName.startsWith('...'))
            {
                this._currentScope.tryDefine(argName.substring(3), ArrayValue.Empty);
            }
            else
            {
                throw new VirtualMachineError(this.createStackTrace(), 'Function called without enough arguments: ' + func.name);
            }
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
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to push to stack trace, stack is full`);
        }

        this._stackTrace.push(scopeFrame);
    }

    public pushStack(value: IValue)
    {
        if (this._stack.length >= this._stackSize)
        {
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Unable to push, stack full`);
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
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Popped empty stack`);
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

        throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Pop stack cast error`);
    }

    public popStackNumber(): number
    {
        const result = this._stack.pop();
        if (!isNumberValue(result))
        {
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Pop stack error, not a number`);
        }
        return result.value;
    }

    public popStackBool(): boolean
    {
        const result = this._stack.pop();
        if (!isBoolValue(result))
        {
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Pop stack error, not a boolean`);
        }
        return result.value;
    }

    public peekStack(): IValue
    {
        if (this._stack.length === 0)
        {
            throw new VirtualMachineError(this.createStackTrace(), `${this.getScopeLine()}: Peek empty stack`);
        }

        return this._stack[this._stack.length - 1];
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
        let text = `  at [${func.name}] in ${func.debugSymbols.sourceName}`;
        if (line >= func.code.length)
        {
            return `${text} end of function`;
        }
        if (line < 0)
        {
            return `${text} before start of function`;
        }

        const codeLocation = func.debugSymbols.getLocation(line);
        return createErrorLogAt(func.debugSymbols.sourceName, codeLocation, func.debugSymbols.fullText);
    }

    private getScopeLine()
    {
        return `${this.currentCode.name}:${this._lineCounter}`;
    }
}