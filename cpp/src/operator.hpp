#pragma once

namespace lysithea_vm
{
    enum class vm_operator
    {
        unknown,

        // General
        push, to_argument,
        call, call_direct, call_return,
        get_property, get, set, define,
        jump, jump_true, jump_false,

        // Misc
        string_concat,

        // Comparison
        greater_than, greater_than_equals,
        equals, not_equals,
        less_than, less_than_equals,

        // Boolean
        op_not, op_and, op_or,

        // Math
        add, sub, multiply, divide,
        inc, dec, unary_negative
    };
} // namespace lysithea_vm
