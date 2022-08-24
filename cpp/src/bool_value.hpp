#pragma once

#include "ivalue.hpp"

namespace stack_vm
{
    class BoolValue : public IValue
    {
        public:
            // Fields
            const bool value;

            // Constructor
            BoolValue(bool value) : value(value) { }
            ~BoolValue() { }

            // Methods
    };
} // stack_vm