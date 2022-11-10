namespace LysitheaVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, ToArgument,
        Call, CallDirect, Return,
        GetProperty, Get, Set, Define,
        Jump, JumpTrue, JumpFalse
    }
}