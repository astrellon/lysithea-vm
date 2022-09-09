namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Run, Call, Return,
        Jump, JumpTrue, JumpFalse
    }
}