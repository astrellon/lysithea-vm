#pragma once

#include <memory>
#include <string>
#include "./ivalue.hpp"
#include "../function.hpp"

namespace stack_vm
{
    using function_ptr = std::shared_ptr<function>;

    class function_value : public ivalue
    {
        public:
            // Fields
            function_ptr value;

            // Constructor
            function_value(function_ptr value) : value(value) { }
            function_value(function value) : value(std::make_shared<function>(value)) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const function_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return value.get() == other->value.get() ? 0 : 1;
            }

            virtual std::string to_string() const
            {
                return "function:" + value->name;
            }

            virtual std::string type_name() const
            {
                return "function";
            }
    };

} // stack_vm