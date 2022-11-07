import Scope from "../scope";
import { operatorScope } from "./standardOperators";
import { mathScope } from "./standardMathLibrary";
import { stringScope } from "./standardStringLibrary";
import { arrayScope } from "./standardArrayLibrary";
import { objectScope } from "./standardObjectLibrary";
import { miscScope } from "./standardMiscLibrary";

export enum LibraryType {
    operator = 1 << 0,
    math = 1 << 1,
    string = 1 << 2,
    array = 1 << 3,
    object = 1 << 4,
    misc = 1 << 5,
    all = (1 << 6) - 1
}

export function addToScope(scope: Scope, libraries: LibraryType)
{
    if (libraries & LibraryType.operator)
    {
        scope.combineScope(operatorScope);
    }
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