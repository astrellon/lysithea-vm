import VirtualMachine from "../virtualMachine";
import { addComparisonHandler } from "./standardComparisonLibrary";
import { addMathHandler } from "./standardMathLibrary";
import { addStringHandler } from "./standardStringLibrary";
import { addArrayHandler } from "./standardArrayLibrary";
import { addObjectHandler } from "./standardObjectLibrary";
import { addValueHandler } from "./standardValueLibrary";

export enum LibraryType {
    comparison = 1 << 0,
    math = 1 << 1,
    string = 1 << 2,
    array = 1 << 3,
    object = 1 << 4,
    value = 1 << 5,
    all = (1 << 6) - 1
}

export function addToVirtualMachine(vm: VirtualMachine, libraries: LibraryType)
{
    if (libraries & LibraryType.comparison)
    {
        addComparisonHandler(vm);
    }
    if (libraries & LibraryType.math)
    {
        addMathHandler(vm);
    }
    if (libraries & LibraryType.string)
    {
        addStringHandler(vm);
    }
    if (libraries & LibraryType.array)
    {
        addArrayHandler(vm);
    }
    if (libraries & LibraryType.object)
    {
        addObjectHandler(vm);
    }
    if (libraries & LibraryType.value)
    {
        addValueHandler(vm);
    }
}