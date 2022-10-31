#pragma once

#include <functional>
#include <string>

#include "./ivalue.hpp"

namespace stack_vm
{
    class virtual_machine;

    using builtin_function_callback = std::function<void (virtual_machine &, const array_value &)>;

    class builtin_function_value : public ivalue
    {
        public:
            // Fields
            builtin_function_callback value;

            // Constructor
            builtin_function_value(builtin_function_callback value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const builtin_function_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return &value == &(other->value) ? 0 : 1;
            }

            virtual std::string to_string() const { return "builtin-function"; }
            virtual std::string type_name() const { return "builtin-function"; }
            virtual bool is_function() const { return true; }

            // virtual void invoke(virtual_machine &vm, const array_value &args, bool push_to_stack_trace)
            // {
            //     // vm
            // }
    };
} // stack_vm