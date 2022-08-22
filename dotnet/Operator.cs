namespace SimpleStackVM
{
    public enum Operator : byte
    {
        Unknown,
        Push, Pop,
        Run, Call, Return,
        Jump, JumpTrue, JumpFalse,
        Log,

/*
        StartLine, EndLine,
        StartChoice, EndChoice,
        Text
        */
    }

}