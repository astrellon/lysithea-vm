namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Pop, Swap, Copy,
        Call, Return,
        Get, Set, Define,
        Jump, JumpTrue, JumpFalse
    }
}