using System;

namespace LysitheaVM
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

            result.TryConstant("toString", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.ToString());
            });

            result.TryConstant("typeof", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.TypeName);
            });

            result.TryConstant("isDefined", (vm, args) =>
            {
                var top = args.GetIndex(0).ToString();
                var isDefined = vm.CurrentScope.TryGetKey(top, out var temp);
                vm.PushStack(isDefined);
            });

            result.TryConstant("compareTo", (vm, args) =>
            {
                var left = args.GetIndex(0);
                var right = args.GetIndex(1);
                vm.PushStack(left.CompareTo(right));
            });

            result.TryConstant("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            });

            return result;
        }
        #endregion
    }
}
