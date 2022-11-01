#pragma once

#include <memory>
#include <string>
#include <cstring>

#include "./ivalue.hpp"

namespace stack_vm
{
    using variable_ptr = std::shared_ptr<std::string>;

    class variable_value : public ivalue
    {
        public:
            // Fields
            variable_ptr value;

            // Constructor
            variable_value(const std::string value) : value(std::make_shared<std::string>(value)) { }
            variable_value(const char *value) : value(std::make_shared<std::string>(value)) { }
            variable_value(variable_ptr value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const variable_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return strcmp(value->c_str(), other->value->c_str());
            }

            virtual std::string to_string() const
            {
                return *value.get();
            }

            virtual std::string type_name() const
            {
                return "variable";
            }
    };
} // stack_vm