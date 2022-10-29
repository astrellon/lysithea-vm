#pragma once

#include <string>
#include <vector>
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
    };

    class iobject_value : public ivalue
    {
        public:
            // Constructor
            virtual ~iobject_value() { }

            // Methods
            virtual std::vector<std::string> object_keys() const = 0;
            virtual bool try_get(const std::string &key, ivalue &result) const = 0;
    };

    class iarray_value : public ivalue
    {
        public:
            // Constructor
            virtual ~iarray_value() { }

            // Methods
            virtual std::vector<std::string> object_keys() const = 0;
            virtual bool try_get(int index, ivalue &result) const = 0;
    };

    class ifunction_value : public ivalue
    {
        public:
            // Constructor
            virtual ~ifunction_value() { }

            // Methods
            virtual void invoke(virtual_machine &vm, const array_value &args, bool push_to_stack_trace);
    };
} // stack_vm