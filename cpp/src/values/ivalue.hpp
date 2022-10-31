#pragma once

#include <string>
#include <vector>
#include <memory>
#include <unordered_map>

namespace stack_vm
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

            virtual bool is_true() const { return false; }
            virtual bool is_false() const { return false; }

            virtual bool is_object() const { return false; }
            virtual std::vector<std::string> object_keys() const { return std::vector<std::string>(); }
            virtual bool try_get(const std::string &key, std::shared_ptr<ivalue> &result) const { return false; }

            virtual bool is_array() const { return false; }
            virtual std::vector<std::shared_ptr<ivalue>> array_values() const { return std::vector<std::shared_ptr<ivalue>>(); }
            virtual bool try_get(int index, std::shared_ptr<ivalue> &result) const { return false; }

            virtual bool is_function() const { return false; }
            virtual void invoke(virtual_machine &vm, std::shared_ptr<array_value> args, bool push_to_stack_trace) const { }
    };
} // stack_vm