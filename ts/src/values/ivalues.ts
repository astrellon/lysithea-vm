import VirtualMachine from "../virtualMachine";
import ArrayValue from "./arrayValue";

export type CompareResult = -1 | 0 | 1;
export interface IValue
{
    readonly compareTo: (other: IValue) => CompareResult;
    readonly toString: () => string;
    readonly typename: () => string;
}

export interface IObjectValue extends IValue
{
    readonly objectKeys: () => ReadonlyArray<string>;
    readonly tryGetKey: (key: string) => IValue | undefined;
}

export interface IArrayValue extends IValue
{
    readonly arrayValues: () => ReadonlyArray<IValue>;
    readonly tryGetIndex: (index: number) => IValue | undefined;
}

export interface IFunctionValue extends IValue
{
    readonly invoke: (vm: VirtualMachine, args: ArrayValue, pushToStackTrace: boolean) => void;
}

export function isIArrayValue(input: IValue | undefined): input is IArrayValue
{
    return input !== undefined && typeof((input as any)['arrayValues']) === 'function';
}

export function isIObjectValue(input: IValue | undefined): input is IObjectValue
{
    return input !== undefined && typeof((input as any)['objectKeys']) === 'function';
}

export function isIFunctionValue(input: IValue | undefined): input is IFunctionValue
{
    return input !== undefined && typeof((input as any)['invoke']) === 'function';
}