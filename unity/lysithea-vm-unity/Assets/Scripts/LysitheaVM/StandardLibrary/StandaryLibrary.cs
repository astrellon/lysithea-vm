using System;

namespace LysitheaVM
{
    public static partial class StandardLibrary
    {
        [Flags]
        public enum LibraryType
        {
            None = 0,
            Math = 1 << 0,
            String = 1 << 1,
            Array = 1 << 2,
            Object = 1 << 3,
            Misc = 1 << 4,
            All = (1 << 5) - 1
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
            if (AllLibraries != null && libraries.HasFlag(LibraryType.All))
            {
                scope.CombineScope(AllLibraries);
                return;
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