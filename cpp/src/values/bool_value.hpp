#pragma once

#include <string>
#include "./ivalue.hpp"

namespace lysithea_vm
{
    class bool_value : public ivalue
    {
        public:
            // Fields
            static const bool_value True;
            static const bool_value False;

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

            virtual bool is_true() const { return value == true; }
            virtual bool is_false() const { return value == false; }

            virtual std::string to_string() const
            {
                return value ? "true" : "false";
            }

            virtual std::string type_name() const
            {
                return "bool";
            }
    };

} // lysithea_vm