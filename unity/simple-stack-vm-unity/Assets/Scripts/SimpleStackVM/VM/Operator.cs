namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Pop, Swap, Copy,
        Run,
        Call, Return,
        Jump, JumpTrue, JumpFalse
    }
}