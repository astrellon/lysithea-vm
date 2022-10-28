import Scope from "./scope";
import { IValue } from "./values/ivalues";
import VMFunction from "./vmFunction";

export enum Operator
{
    Unknown,
    Push, ToArgument,
    Call, CallDirect, Return,
    GetProperty, Get, Set, Define,
    Jump, JumpTrue, JumpFalse
}

export type Editable<T> =
{
    -readonly [P in keyof T]: T[P];
};

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