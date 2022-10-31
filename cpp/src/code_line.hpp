#pragma once

#include <memory>

#include "operator.hpp"

namespace stack_vm
{
    class ivalue;

    class code_line
    {
        public:
            // Fields
            const vm_operator op;
            std::shared_ptr<const ivalue> value;

            // Constructor
            code_line(vm_operator op) : op(op), value(nullptr) { }
            code_line(vm_operator op, std::shared_ptr<const ivalue> value) : op(op), value(value) { }

            // Methods
    };
} // stack_vm