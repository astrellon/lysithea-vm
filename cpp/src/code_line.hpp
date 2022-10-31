#pragma once

// #include <optional>
#include <memory>

#include "values/ivalue.hpp"
#include "values/undefined_value.hpp"
#include "operator.hpp"

namespace stack_vm
{
    class code_line
    {
        public:
            // Fields
            const vm_operator op;
            std::shared_ptr<ivalue> value;

            // Constructor
            code_line(vm_operator op) : op(op), value(nullptr) { }
            code_line(vm_operator op, const ivalue &value) : op(op), value(std::make_shared<ivalue>(value)) { }

            // Methods
    };
} // stack_vm