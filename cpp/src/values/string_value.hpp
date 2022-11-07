#pragma once

#include <memory>
#include <string>
#include <cstring>

#include "./complex_value.hpp"

namespace stack_vm
{
    class string_value : public complex_value
    {
        public:
            // Fields
            std::string data;

            // Constructor
            string_value(const std::string &data) : data(data) { }
            string_value(const char *data) : data(data) { }

            // Methods
            virtual int compare_to(const complex_value *input) const
            {
                auto other = dynamic_cast<const string_value *>(input);
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
                return "string";
            }

            virtual int object_length() const { return 1; }
            virtual bool is_object() const { return true; }
            virtual std::vector<std::string> object_keys() const
            {
                std::vector<std::string> result;
                result.push_back("length");
                return result;
            }
            virtual bool try_get(const std::string &key, stack_vm::value &result) const;
    };
} // stack_vm