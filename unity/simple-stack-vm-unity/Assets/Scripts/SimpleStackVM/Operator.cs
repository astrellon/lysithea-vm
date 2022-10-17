namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push,
        Call, CallDirect, Return,
        GetProperty, Get, Set, Define,
        Jump, JumpTrue, JumpFalse
    }
}