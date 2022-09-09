#pragma once

namespace stack_vm
{
    enum class vm_operator {
        unknown, push, call, run, call_return, jump, jump_true, jump_false
    };
} // namespace stack_vm
