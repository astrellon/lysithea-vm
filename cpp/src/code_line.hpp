#pragma once

#include <memory>
#include <sstream>

#include "operator.hpp"

namespace stack_vm
{
    class ivalue;

    class code_line
    {
        public:
            // Fields
            vm_operator op;
            std::shared_ptr<ivalue> value;

            // Constructor
            code_line(vm_operator op) : op(op), value(nullptr) { }
            code_line(vm_operator op, std::shared_ptr<ivalue> value) : op(op), value(value) { }

            // Methods
            std::string to_string() const;
    };
} // stack_vm