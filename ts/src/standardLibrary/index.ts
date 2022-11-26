import { Scope } from "../scope";
import { mathScope } from "./standardMathLibrary";
import { stringScope } from "./standardStringLibrary";
import { arrayScope } from "./standardArrayLibrary";
import { objectScope } from "./standardObjectLibrary";
import { miscScope } from "./standardMiscLibrary";

export { mathScope } from "./standardMathLibrary";
export { stringScope } from "./standardStringLibrary";
export { arrayScope } from "./standardArrayLibrary";
export { objectScope } from "./standardObjectLibrary";
export { miscScope } from "./standardMiscLibrary";

export enum LibraryType {
    math = 1 << 0,
    string = 1 << 1,
    array = 1 << 2,
    object = 1 << 3,
    misc = 1 << 4,
    all = (1 << 5) - 1
}

export function addToScope(scope: Scope, libraries: LibraryType)
{
    if (libraries & LibraryType.math)
    {
        scope.combineScope(mathScope);
    }
    if (libraries & LibraryType.string)
    {
        scope.combineScope(stringScope);
    }
    if (libraries & LibraryType.array)
    {
        scope.combineScope(arrayScope);
    }
    if (libraries & LibraryType.object)
    {
        scope.combineScope(objectScope);
    }
    if (libraries & LibraryType.misc)
    {
        scope.combineScope(miscScope);
    }
}