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
            code_line(vm_operator op) : op(op)
            {
                if (op == vm_operator::push)
                {
                    throw std::runtime_error("Cannot create code line of push without arg");
                }
            }
            code_line(vm_operator op, stack_vm::value input) : op(op), value(input)
            {
                if (op == vm_operator::push && input.is_undefined())
                {
                    throw std::runtime_error("Cannot create code line of push without arg");
                }
            }

            // Methods
            std::string to_string() const;

            inline bool has_value() const
            {
                return !value.is_undefined();
            }
    };
} // stack_vm