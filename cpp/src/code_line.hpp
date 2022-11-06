#pragma once

#include <memory>
#include <sstream>

#include "operator.hpp"
#include "./values/value.hpp"

namespace stack_vm
{
    class complex_value;

    class code_line
    {
        public:
            // Fields
            vm_operator op;
            stack_vm::value value;

            // Constructor
            code_line(vm_operator op) : op(op), value(nullptr) { }
            code_line(vm_operator op, stack_vm::value input) : op(op), value(input) { }

            // Methods
            std::string to_string() const;

            inline bool has_value() const
            {
                return !value.is_null();
            }
    };
} // stack_vm