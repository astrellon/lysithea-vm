import { valueCompareTo, valueToString, valueTypeof } from "../types";
import VirtualMachine from "../virtualMachine";

export default class StandardStringLibrary
{
    public static readonly HandleName = "string";

    public static AddHandler(vm: VirtualMachine)
    {
        vm.addRunHandler(StandardStringLibrary.HandleName, StandardStringLibrary.Handler)
    }

    public static Handler(command: string, vm: VirtualMachine)
    {
        switch (command)
        {
            case "append":
                {
                    const right = vm.popStack();
                    const left = vm.popStack();
                    vm.pushStack(valueToString(left) + valueToString(right));
                    break;
                }
            case "prepend":
                {
                    const right = vm.popStack();
                    const left = vm.popStack();
                    vm.pushStack(valueToString(right) + valueToString(left));
                    break;
                }
            case "length":
                {
                    const top = vm.popStackCast<string>();
                    vm.pushStack(top.length);
                    break;
                }
            case "get":
                {
                    const index = vm.popStackCast<number>();
                    const top = vm.popStackCast<string>();
                    vm.pushStack(top[index]);
                    break;
                }
            case "set":
                {
                    const value = vm.popStack();
                    const index = vm.popStackCast<number>();
                    const top = vm.popStackCast<string>();
                    vm.pushStack(StandardStringLibrary.set(top, index, valueToString(value)));
                    break;
                }
            case "insert":
                {
                    const value = vm.popStack();
                    const index = vm.popStackCast<number>();
                    const top = vm.popStackCast<string>();
                    vm.pushStack(StandardStringLibrary.insert(top, index, valueToString(value)));
                    break;
                }
            case "substring":
                {
                    const length = vm.popStackCast<number>();
                    const index = vm.popStackCast<number>();
                    const top = vm.popStackCast<string>();
                    vm.pushStack(top.substring(index, index + length));
                    break;
                }
        }
    }

    public static set(input: string, index: number, value: string): string
    {
        return `${input.substring(0, index)}${value}${input.substring(index + 1)}`;
    }

    public static insert(input: string, index: number, value: string): string
    {
        return `${input.substring(0, index)}${value}${input.substring(index)}`;
    }
}