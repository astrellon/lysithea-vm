#pragma once

#include <optional>

#include "value.hpp"
#include "operator.hpp"

namespace stack_vm
{
    class CodeLine
    {
        public:
            // Fields
            const Operator op;
            const std::optional<Value> value;

            // Constructor
            CodeLine(Operator op) : op(op) { }
            CodeLine(Operator op, const Value &value) : op(op), value(value) { }

            // Methods
    };
} // stack_vm