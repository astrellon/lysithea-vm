#pragma once

#include <optional>

#include "value.hpp"
#include "operator.hpp"

namespace stack_vm
{
    class code_line
    {
        public:
            // Fields
            const vm_operator op;
            const std::optional<stack_vm::value> value;

            // Constructor
            code_line(vm_operator op) : op(op) { }
            code_line(vm_operator op, const stack_vm::value &value) : op(op), value(value) { }
            code_line(vm_operator op, const std::optional<stack_vm::value> value) : op(op), value(value) { }

            // Methods
    };
} // stack_vm