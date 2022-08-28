import { Operator, Scope, ScopeFrame, Value } from "./types";

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

    private _running: boolean = false;
    public get running() { return this._running; }

    private _stack: Value[] = [];
    public get stack(): ReadonlyArray<Value> { return this._stack; }

    private _stackTrace: ScopeFrame[] = [];
    public get stackTrace(): ReadonlyArray<ScopeFrame> { return this._stackTrace; }

    public paused: boolean = false;

    private readonly _scopes: ScopeMap = {};

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

    public run(startScopeName: string | null)
    {
        if (startScopeName != null)
        {
            const startScope = this._scopes[startScopeName];
            if (startScope == null)
            {
                throw new Error(`Unable to find scope: ${startScopeName} to start virtual machine`);
            }

            this._currentScope = startScope;
        }

        this._running = true;
        this.paused = false;
        while (this._running && !this.paused)
        {
            this.step();
        }
    }

    public stop()
    {
        this._running = false;
    }

    public step()
    {
        if (this._programCounter >= this._currentScope.code.length)
        {
            this.stop();
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
                    this.popObject();
                    break;
                }
            case 'jump':
                {
                    const label = this.popObject();
                    this.jumpValue(label);
                    break;
                }
            case 'jumpTrue':
                {
                    const label = this.popObject();
                    const top = this.popObject();
                    if (top == true)
                    {
                        this.jumpValue(label);
                    }
                    break;
                }
            case 'jumpFalse':
                {
                    const label = this.popObject();
                    const top = this.popObject();
                    if (top == false)
                    {
                        this.jumpValue(label);
                    }
                    break;
                }
            case 'call':
                {
                    const label = this.popObject();
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
                    const top = this.popObject();
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
            this.jump(value, undefined);
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

    public jump(label: string, scopeName?: string)
    {
        if (scopeName != null)
        {
            const newScope = this._scopes[scopeName];
            if (newScope == null)
            {
                throw new Error(`${this.getScopeLine()}: Unable to find scope to jump to ${scopeName}`);
            }
            this._currentScope = newScope;
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
        this._stack.push(value);
    }

    private getScopeLine()
    {
        return `${this._currentScope.name}:${this._programCounter}`;
    }
}