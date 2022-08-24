#pragma once

#include <string>
#include "ivalue.hpp"

namespace stack_vm
{
    class StringValue : public IValue
    {
        public:
            // Fields
            const std::string value;

            // Constructor
            StringValue(const std::string value) : value(value) { }
            ~StringValue() { }

            // Methods
    };
} // stack_vm