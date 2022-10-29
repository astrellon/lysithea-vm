#pragma once

#include <optional>

#include "values/ivalue.hpp"
#include "operator.hpp"

namespace stack_vm
{
    class code_line
    {
        public:
            // Fields
            const vm_operator op;
            std::optional<ivalue> value;

            // Constructor
            code_line(vm_operator op) : op(op) { }
            code_line(vm_operator op, const ivalue &value) : op(op), value(value) { }
            code_line(vm_operator op, const std::optional<ivalue> value) : op(op), value(value) { }

            // Methods
    };
} // stack_vm