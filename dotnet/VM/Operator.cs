namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Pop, Swap, Copy,
        Call, Return,
        Get, Set, CreateProcedure,
        Jump, JumpTrue, JumpFalse
    }
}