import Scope, { IReadOnlyScope } from "./scope";
import VMFunction from "./vmFunction";

export default class Script
{
    public static readonly Empty = new Script(Scope.Empty, VMFunction.Empty);

    public readonly builtinScope: IReadOnlyScope;
    public readonly code: VMFunction;

    constructor (builtinScope: IReadOnlyScope, code: VMFunction)
    {
        this.builtinScope = builtinScope;
        this.code = code;
    }
}