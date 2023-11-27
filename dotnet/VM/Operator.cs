namespace LysitheaVM
{
    public enum Operator : byte
    {
        Unknown,

        // General
        Push, ToArgument,
        Call, CallDirect, Return, ResetStackSize,
        GetProperty, Get, Set, Define,
        Jump, JumpTrue, JumpFalse,

        // Misc
        StringConcat,

        // Comparison
        GreaterThan, GreaterThanEquals,
        Equals, NotEquals,
        LessThan, LessThanEquals,

        // Boolean
        Not, And, Or,

        // Math
        Add, Sub, Multiply, Divide,
        Inc, Dec, UnaryNegative
    }
}