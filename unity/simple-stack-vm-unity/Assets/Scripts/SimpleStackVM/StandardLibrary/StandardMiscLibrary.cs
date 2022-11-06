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

            result.Define("toString", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.ToString());
            });

            result.Define("typeof", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.TypeName);
            });

            result.Define("compareTo", (vm, args) =>
            {
                var left = args.GetIndex(0);
                var right = args.GetIndex(1);
                vm.PushStack(left.CompareTo(right));
            });

            result.Define("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}
