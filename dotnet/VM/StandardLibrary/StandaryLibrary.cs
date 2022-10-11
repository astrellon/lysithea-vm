using System;

namespace SimpleStackVM
{
    public static partial class StandardLibrary
    {
        [Flags]
        public enum LibraryType
        {
            None = 0,
            Comparison = 1 << 0,
            Math = 1 << 1,
            String = 1 << 2,
            Array = 1 << 3,
            Object = 1 << 4,
            Misc = 1 << 5,
            All = (1 << 6) - 1
        }

        #region Methods
        public static void AddToVirtualMachine(VirtualMachine vm, LibraryType libraries = LibraryType.All)
        {
            if (libraries.HasFlag(LibraryType.Comparison))
            {
                vm.AddBuiltinScope(StandardComparisonLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Math))
            {
                vm.AddBuiltinScope(StandardMathLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.String))
            {
                vm.AddBuiltinScope(StandardStringLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Array))
            {
                vm.AddBuiltinScope(StandardArrayLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Object))
            {
                vm.AddBuiltinScope(StandardObjectLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Misc))
            {
                vm.AddBuiltinScope(StandardMiscLibrary.Scope);
            }
        }
        #endregion
    }
}