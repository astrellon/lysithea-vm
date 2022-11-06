#pragma once

#include <memory>
#include <string>
#include <cstring>

#include "./complex_value.hpp"

namespace stack_vm
{
    class variable_value : public complex_value
    {
        public:
            // Fields
            std::string data;

            // Constructor
            variable_value(const std::string data) : data(data) { }
            variable_value(const char *data) : data(data) { }

            // Methods
            bool is_label() const
            {
                return data.size() > 0 && data.at(0) == ':';
            }

            virtual int compare_to(const complex_value *input) const
            {
                auto other = dynamic_cast<const variable_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return strcmp(data.c_str(), other->data.c_str());
            }

            virtual std::string to_string() const
            {
                return data;
            }

            virtual std::string type_name() const
            {
                return "variable";
            }
    };
} // stack_vm