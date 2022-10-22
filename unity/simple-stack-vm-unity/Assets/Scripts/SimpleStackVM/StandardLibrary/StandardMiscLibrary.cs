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
                if (top is StringValue || top is VariableValue)
                {
                    return;
                }

                top = vm.PopStack();
                vm.PushStack(top.ToString());
            });

            result.Define("typeof", (vm, numArgs) =>
            {
                var top = vm.PopStack();
                vm.PushStack(top.TypeName);
            });

            result.Define("compareTo", (vm, numArgs) =>
            {
                var right = vm.PopStack();
                var left = vm.PopStack();
                vm.PushStack(left.CompareTo(right));
            });

            result.Define("print", (vm, numArgs) =>
            {
                var args = vm.GetArgs(numArgs);
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}
