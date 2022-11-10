#pragma once

#include <functional>
#include <string>

#include "./complex_value.hpp"

namespace lysithea_vm
{
    class virtual_machine;

    using builtin_function_callback = std::function<void (virtual_machine &, const array_value &)>;

    class builtin_function_value : public complex_value
    {
        public:
            // Fields
            builtin_function_callback data;

            // Constructor
            builtin_function_value(builtin_function_callback data) : data(data) { }

            // Methods
            virtual int compare_to(const complex_value *input) const
            {
                auto other = dynamic_cast<const builtin_function_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return &data == &(other->data) ? 0 : 1;
            }

            virtual std::string to_string() const { return "builtin-function"; }
            virtual std::string type_name() const { return "builtin-function"; }
            virtual bool is_function() const { return true; }

            virtual void invoke(virtual_machine &vm, std::shared_ptr<const array_value> args, bool push_to_stack_trace) const
            {
                data(vm, *args);
            }
    };
} // lysithea_vm