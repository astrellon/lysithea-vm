namespace SimpleStackVM
{
    public class Script
    {
        #region Fields
        public readonly IReadOnlyScope BuiltinScope;
        public readonly Function Code;
        #endregion

        #region Constructor
        public Script(IReadOnlyScope builtinScope, Function code)
        {
            this.BuiltinScope = builtinScope;
            this.Code = code;
        }
        #endregion
    }
}