#pragma once

#include <string>
#include "./ivalue.hpp"

namespace stack_vm
{
    class bool_value : public ivalue
    {
        public:
            // Fields
            static bool_value True;
            static bool_value False;

            bool value;

            // Constructor
            bool_value(bool value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const bool_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return value == other->value ? 0 : 1;
            }

            virtual std::string to_string() const
            {
                return value ? "true" : "false";
            }

            virtual std::string type_name() const
            {
                return "bool";
            }
    };

} // stack_vm