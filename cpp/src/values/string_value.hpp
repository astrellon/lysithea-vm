#pragma once

#include <memory>
#include <string>
#include <cstring>

#include "./complex_value.hpp"

namespace stack_vm
{
    using string_ptr = std::shared_ptr<std::string>;

    class string_value : public complex_value
    {
        public:
            // Fields
            string_ptr value;

            // Constructor
            string_value(const std::string value) : value(std::make_shared<std::string>(value)) { }
            string_value(const char *value) : value(std::make_shared<std::string>(value)) { }
            string_value(string_ptr value) : value(value) { }

            // Methods
            virtual int compare_to(const complex_value *input) const
            {
                auto other = dynamic_cast<const string_value *>(input);
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
                return "string";
            }
    };
} // stack_vm