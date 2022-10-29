#pragma once

namespace stack_vm
{
    enum class vm_operator
    {
        unknown,
        push, to_argument,
        call, call_direct, call_return,
        get_property, get, set, define,
        jump, jump_true, jump_false
    };
} // namespace stack_vm
