#pragma once

#include <memory>
#include <string>
#include "./complex_value.hpp"
#include "../function.hpp"

namespace stack_vm
{
    using function_ptr = std::shared_ptr<function>;

    class function_value : public complex_value
    {
        public:
            // Fields
            function_ptr data;

            // Constructor
            function_value(function_ptr data) : data(data) { }
            function_value(function data) : data(std::make_shared<function>(data)) { }

            // Methods
            virtual int compare_to(const complex_value *input) const
            {
                auto other = dynamic_cast<const function_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return data.get() == other->data.get() ? 0 : 1;
            }

            virtual std::string to_string() const { return "function:" + data->name; }
            virtual std::string type_name() const { return "function"; }
            virtual bool is_function() const { return true; }

            virtual void invoke(virtual_machine &vm, std::shared_ptr<const array_value> args, bool push_to_stack_trace) const;
    };

} // stack_vm