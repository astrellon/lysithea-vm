namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push,
        Call, Return,
        Get, Set, Define,
        Jump, JumpTrue, JumpFalse
    }
}