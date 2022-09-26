import { Scope, ScopeFrame, Value } from "./types";

export type RunHandler = (value: Value, vm: VirtualMachine) => void;
type ScopeMap = { [key: string]: Scope }

const EmptyScope: Scope = {
    name: '',
    code: [],
    labels: {}
}

export default class VirtualMachine
{
    private _currentScope: Scope = EmptyScope;
    public get currentScope() { return this._currentScope; }

    private _programCounter: number = 0;
    public get programCounter() { return this._programCounter; }

    public running: boolean = false;
    public paused: boolean = false;

    private _stack: Value[] = [];
    public get stack(): ReadonlyArray<Value> { return this._stack; }

    private _stackTrace: ScopeFrame[] = [];
    public get stackTrace(): ReadonlyArray<ScopeFrame> { return this._stackTrace; }

    private _scopes: ScopeMap = {};

    private readonly _runHandler: RunHandler;
    private readonly _stackSize: number;

    constructor (stackSize: number, runHandler: RunHandler)
    {
        this._stackSize = stackSize;
        this._runHandler = runHandler;
    }

    public addScope(scope: Scope)
    {
        this._scopes[scope.name] = scope;
    }

    public addScopes(scopes: Scope[])
    {
        for (const scope of scopes)
        {
            this.addScope(scope);
        }
    }

    public clearScopes()
    {
        this._scopes = {};
    }

    public reset()
    {
        this._programCounter = 0;
        this._stack = [];
        this._stackTrace = [];
        this.running = false;
        this.paused = false;
    }

    public setCurrentScope(scopeName: string)
    {
        const startScope = this._scopes[scopeName];
        if (startScope == null)
        {
            throw new Error(`Unable to find scope: ${scopeName}`);
        }

        this._currentScope = startScope;
    }

    public step()
    {
        if (this._programCounter >= this._currentScope.code.length)
        {
            console.log('VM hits end!');
            this.running = false;
            return;
        }

        const codeLine = this._currentScope.code[this._programCounter++];

        switch (codeLine.operator)
        {
            case 'push':
                {
                    if (codeLine.value == null)
                    {
                        throw new Error(`${this.getScopeLine()}: Push requires input`);
                    }

                    this._stack.push(codeLine.value);
                    break;
                }
            case 'pop':
                {
                    this._stack.pop();
                    break;
                }
            case 'swap':
                {
                    const value = codeLine.value ?? this.popObject();
                    if (typeof(value) !== 'number')
                    {
                        throw new Error(`${this.getScopeLine()}: Swap needs value to swap`);
                    }

                    if (!this.swapStack(value))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to swap, index out of range: ${value}`);
                    }
                    break;
                }
            case 'copy':
                {
                    const value = codeLine.value ?? this.popObject();
                    if (typeof(value) !== 'number')
                    {
                        throw new Error(`${this.getScopeLine()}: Copy needs value to swap`);
                    }

                    if (!this.copyStack(value))
                    {
                        throw new Error(`${this.getScopeLine()}: Unable to copy, index out of range: ${value}`);
                    }
                }
            case 'jump':
                {
                    const label = codeLine.value ?? this.popObject();
                    this.jumpValue(label);
                    break;
                }
            case 'jumpTrue':
                {
                    const label = codeLine.value ?? this.popObject();
                    const top = this.popObject();
                    if (top == true)
                    {
                        this.jumpValue(label);
                    }
                    break;
                }
            case 'jumpFalse':
                {
                    const label = codeLine.value ?? this.popObject();
                    const top = this.popObject();
                    if (top == false)
                    {
                        this.jumpValue(label);
                    }
                    break;
                }
            case 'call':
                {
                    const label = codeLine.value ?? this.popObject();
                    this.call(label);
                    break;
                }
            case 'return':
                {
                    this.callReturn();
                    break;
                }
            case 'run':
                {
                    const top = codeLine.value ?? this.popObject();
                    this._runHandler(top, this);
                    break;
                }
            default:
                {
                    throw new Error(`${this.getScopeLine()}: Unknown operator: ${codeLine.operator}`);
                }
        }
    }

    public call(value: Value)
    {
        if (this._stackTrace.length >= this._stackSize)
        {
            throw new Error(`${this.getScopeLine()}: Unable to call, stack trace full`);
        }
        this._stackTrace.push({ lineNumber: this._programCounter, scope: this._currentScope });
        this.jumpValue(value);
    }

    public callReturn()
    {
        const scopeFrame = this._stackTrace.pop();
        if (scopeFrame == undefined)
        {
            throw new Error(`${this.getScopeLine()}: Unable to return, call stack empty`);
        }

        this._currentScope = scopeFrame.scope;
        this._programCounter = scopeFrame.lineNumber;
    }

    public jumpValue(value: Value)
    {
        if (typeof(value) === 'string')
        {
            this.jumpLabel(value);
        }
        else if (Array.isArray(value))
        {
            if (value.length == 0)
            {
                throw new Error(`${this.getScopeLine()}: Cannot jump to an empty array`);
            }

            const scopeName = value.length > 1 ? value[1] as string : undefined;
            this.jump(value[0], scopeName);
        }
    }

    public jumpLabel(input: string)
    {
        if (input == null || input.length === 0)
        {
            this._programCounter = 0;
        }
        else
        {
            if (input[0] == ':')
            {
                this.jump(input, undefined);
            }
            else
            {
                this.jump(undefined, input);
            }
        }
    }

    public jump(label?: string, scopeName?: string)
    {
        if (scopeName != null)
        {
            this.setCurrentScope(scopeName);
        }

        if (label == null || label.length == 0)
        {
            this._programCounter = 0;
            return;
        }

        const line = this._currentScope.labels[label];
        if (line == null)
        {
            throw new Error(`Unable to jump to label: ${label}`);
        }

        this._programCounter = line;
    }

    public popObject() : Value
    {
        const result = this._stack.pop();
        if (result == undefined)
        {
            throw new Error(`${this.getScopeLine()}: Popped empty stack`);
        }
        return result;
    }

    public popObjectCast<T extends Value>() : T
    {
        const result = this._stack.pop();
        if (result == undefined)
        {
            throw new Error(`${this.getScopeLine()}: Popped empty stack`);
        }
        return result as T;
    }

    public pushObject(value: Value)
    {
        if (this._stack.length >= this._stackSize)
        {
            throw new Error(`${this.getScopeLine()}: Unable to push, stack full`);
        }
        this._stack.push(value);
    }

    public copyStack(topOffset: number)
    {
        const newIndex = this._stack.length - topOffset;
        if (newIndex < 0 || newIndex >= this._stack.length)
        {
            return false;
        }

        this._stack.push(this._stack[newIndex]);

        return true;
    }

    public swapStack(topOffset: number)
    {
        const newIndex = this._stack.length - topOffset;
        if (newIndex < 0 || newIndex >= this._stack.length)
        {
            return false;
        }

        const topIndex = this._stack.length - 1;
        const top = this._stack[topIndex];
        const other = this._stack[newIndex];

        this._stack[topIndex] = other;
        this._stack[newIndex] = top;

        return true;
    }

    private getScopeLine()
    {
        return `${this._currentScope.name}:${this._programCounter}`;
    }
}