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
            All = (1 << 5) - 1
        }

        #region Methods
        public static void AddToVirtualMachine(VirtualMachine vm, LibraryType libraries = LibraryType.All)
        {
            if (libraries.HasFlag(LibraryType.Comparison))
            {
                StandardComparisonLibrary.AddHandler(vm);
            }
            if (libraries.HasFlag(LibraryType.Math))
            {
                StandardMathLibrary.AddHandler(vm);
            }
            if (libraries.HasFlag(LibraryType.String))
            {
                StandardStringLibrary.AddHandler(vm);
            }
            if (libraries.HasFlag(LibraryType.Array))
            {
                StandardArrayLibrary.AddHandler(vm);
            }
            if (libraries.HasFlag(LibraryType.Object))
            {
                StandardObjectLibrary.AddHandler(vm);
            }
        }
        #endregion
    }
}