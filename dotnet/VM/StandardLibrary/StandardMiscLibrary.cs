using System;

namespace SimpleStackVM
{
    public static class StandardMiscLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("toString", (vm, numArgs) =>
            {
                var top = vm.PeekStack();
                if (top is StringValue || top is SymbolValue)
                {
                    return;
                }

                top = vm.PopStack();
                vm.PushStack(new StringValue(top.ToString()));
            });

            result.Define("typeof", (vm, numArgs) =>
            {
                var top = vm.PopStack();
                vm.PushStack(new StringValue(GetTypeOf(top)));
            });

            result.Define("compareTo", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(new NumberValue(left.CompareTo(right)));
            });

            result.Define("print", (vm, numArgs) =>
            {
                var args = vm.GetArgs(numArgs);
                Console.WriteLine(string.Join("", args));
            });

            return result;
        }

        public static string GetTypeOf(IValue input)
        {
            if (input is StringValue)
            {
                return "string";
            }
            if (input is NumberValue)
            {
                return "number";
            }
            if (input is BoolValue)
            {
                return "bool";
            }
            if (input is ArrayValue)
            {
                return "array";
            }
            if (input is ObjectValue)
            {
                return "object";
            }
            if (input is NullValue)
            {
                return "null";
            }
            if (input is AnyValue)
            {
                return "any";
            }
            if (input is BuiltinFunctionValue)
            {
                return "builtin-proc";
            }
            if (input is FunctionValue)
            {
                return "proc";
            }

            return "unknown";
        }
        #endregion
    }
}
