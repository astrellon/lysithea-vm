namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Swap,
        Run,
        Call, Return,
        Jump, JumpTrue, JumpFalse
    }
}