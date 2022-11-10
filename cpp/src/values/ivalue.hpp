#pragma once

#include <string>
#include <vector>
#include <memory>
#include <stdexcept>
#include <unordered_map>

namespace lysithea_vm
{
    class virtual_machine;
    class array_value;

    class ivalue
    {
        public:
            // Constructor
            virtual ~ivalue() { }

            // Methods
            virtual int compare_to(const ivalue *input) const = 0;
            virtual std::string to_string() const = 0;
            virtual std::string type_name() const = 0;

            // Boolean methods
            virtual bool is_true() const { return false; }
            virtual bool is_false() const { return false; }

            // Object methods
            virtual bool is_object() const { return false; }
            virtual std::vector<std::string> object_keys() const
            {
                std::vector<std::string> result;
                return result;
            }
            virtual bool try_get(const std::string &key, std::shared_ptr<ivalue> &result) const { return false; }

            // Array methods
            virtual bool is_array() const { return false; }
            virtual int array_length() const { return 0; }
            virtual bool try_get(int index, std::shared_ptr<ivalue> &result) const { return false; }

            // Function methods
            virtual bool is_function() const { return false; }
            virtual void invoke(virtual_machine &vm, std::shared_ptr<const array_value> args, bool push_to_stack_trace) const
            {
                throw std::runtime_error("Attempting to invoke a function that does not override the invoke method");
            }

        private:
            // Fields
            static const std::vector<std::string> empty_object_keys;
    };
} // lysithea_vm