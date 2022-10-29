#pragma once

#include <string>

namespace stack_vm
{
    class ivalue
    {
        public:
            // Constructor
            virtual ~ivalue() { }

            // Methods
            virtual int compare_to(const ivalue *input) const = 0;
            virtual std::string to_string() const = 0;
            virtual std::string type_name() const = 0;
    };
} // stack_vm