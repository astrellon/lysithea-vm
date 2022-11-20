namespace LysitheaVM
{
    public enum Operator : byte
    {
        Unknown,

        // General
        Push, ToArgument,
        Call, CallDirect, Return,
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
        AddTo, SubFrom, MultiplyBy, DivideBy,
        Inc, Dec, UnaryNegative
    }
}