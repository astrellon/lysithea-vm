#pragma once

#include "./ivalue.hpp"
#include <string>

namespace stack_vm
{
    class null_value : public ivalue
    {
        public:
            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const null_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return 0;
            }

            virtual std::string to_string() const
            {
                return "null";
            }

            virtual std::string type_name() const
            {
                return "null";
            }
    };
} // stack_vm