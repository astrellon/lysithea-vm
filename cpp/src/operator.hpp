#pragma once

namespace stack_vm
{
    enum class Operator {
        Unknown, Push, Pop, Call, Run, Return, Jump, JumpTrue, JumpFalse
    };
} // namespace stack_vm
