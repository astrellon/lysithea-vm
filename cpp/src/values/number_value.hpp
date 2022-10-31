#pragma once

#include <string>
#include <cmath>
#include "./ivalue.hpp"

namespace stack_vm
{
    class number_value : public ivalue
    {
        public:
            // Fields
            double value;

            // Constructor
            number_value(double value) : value(value) { }
            number_value(float value) : value(static_cast<double>(value)) { }
            number_value(int value) : value(static_cast<double>(value)) { }
            number_value(long value) : value(static_cast<double>(value)) { }
            number_value(unsigned int value) : value(static_cast<double>(value)) { }
            number_value(std::size_t value) : value(static_cast<double>(value)) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const number_value *>(input);
                if (!other)
                {
                    return 1;
                }

                return compare(value, other->value);
            }

            virtual std::string to_string() const
            {
                return std::to_string(value);
            }

            virtual std::string type_name() const
            {
                return "number";
            }

            static int compare(double left, double right)
            {
                auto diff = left - right;
                if (fabs(diff) < 0.0001)
                {
                    return 0;
                }

                return diff < 0 ? -1 : 1;
            }

            static int compare(std::size_t left, std::size_t right)
            {
                if (left == right)
                {
                    return 0;
                }

                return left < right ? -1 : 1;
            }

            static int compare(int left, int right)
            {
                auto diff = left - right;
                if (diff == 0)
                {
                    return 0;
                }

                return diff < 0 ? -1 : 1;
            }
    };
} // stack_vm