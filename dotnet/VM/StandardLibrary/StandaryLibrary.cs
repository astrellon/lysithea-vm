using System;

namespace SimpleStackVM
{
    public static partial class StandardLibrary
    {
        [Flags]
        public enum LibraryType
        {
            None = 0,
            Operators = 1 << 0,
            Math = 1 << 1,
            String = 1 << 2,
            Array = 1 << 3,
            Object = 1 << 4,
            Misc = 1 << 5,
            All = (1 << 6) - 1
        }

        public static IReadOnlyScope AllLibraries = CreateAllLibrariesScope();

        #region Methods
        public static Scope CreateAllLibrariesScope()
        {
            var result = new Scope();
            AddToScope(result);
            return result;
        }

        public static void AddToScope(Scope scope, LibraryType libraries = LibraryType.All)
        {
            if (libraries.HasFlag(LibraryType.All))
            {
                scope.CombineScope(AllLibraries);
                return;
            }

            if (libraries.HasFlag(LibraryType.Operators))
            {
                scope.CombineScope(StandardOperators.Scope);
            }
            if (libraries.HasFlag(LibraryType.Math))
            {
                scope.CombineScope(StandardMathLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.String))
            {
                scope.CombineScope(StandardStringLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Array))
            {
                scope.CombineScope(StandardArrayLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Object))
            {
                scope.CombineScope(StandardObjectLibrary.Scope);
            }
            if (libraries.HasFlag(LibraryType.Misc))
            {
                scope.CombineScope(StandardMiscLibrary.Scope);
            }
        }
        #endregion
    }
}