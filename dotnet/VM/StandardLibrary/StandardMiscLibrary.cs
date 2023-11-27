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

            result.TryDefine("toString", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.ToString());
            });

            result.TryDefine("typeof", (vm, args) =>
            {
                var top = args.GetIndex(0);
                vm.PushStack(top.TypeName);
            });

            result.TryDefine("isDefined", (vm, args) =>
            {
                var top = args.GetIndex(0).ToString();
                var isDefined = vm.CurrentScope.TryGetKey(top, out var temp);
                vm.PushStack(isDefined);
            });

            result.TryDefine("isBuiltin", (vm, args) =>
            {
                var top = args.GetIndex(0).ToString();
                var isBuiltin = vm.BuiltinScope != null ? vm.BuiltinScope.TryGetKey(top, out var temp) : false;
                vm.PushStack(isBuiltin);
            });

            result.TryDefine("compareTo", (vm, args) =>
            {
                var left = args.GetIndex(0);
                var right = args.GetIndex(1);
                vm.PushStack(left.CompareTo(right));
            });

            result.TryDefine("print", (vm, args) =>
            {
                Console.WriteLine(string.Join("", args.Value));
            }, hasReturn: false);

            return result;
        }
        #endregion
    }
}
