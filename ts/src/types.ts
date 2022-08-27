export enum Operator
{
    Unknown, Push, Pop, Call, Return, Jump, JumpTrue, JumpFalse, Run
}

export type Value = string | boolean | number | ArrayValue | ObjectValue | null;
export type ArrayValue = ReadonlyArray<Value>;
export interface ObjectValue
{
    readonly [key: string]: Value
}

export interface CodeLine
{
    readonly operator: Operator;
    readonly value?: Value;
}

export interface Scope
{
    readonly name: string;
    readonly code: ReadonlyArray<CodeLine>;
    readonly labels: { readonly [label: string]: number };
}

export interface ScopeFrame
{
    readonly lineNumber: number;
    readonly scope: Scope;
}