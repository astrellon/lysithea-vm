#pragma once

#include <memory>
#include <vector>
#include <string>

#include "./ivalue.hpp"
#include "./number_value.hpp"

namespace stack_vm
{
    using array_vector = std::vector<ivalue>;
    using array_ptr = std::shared_ptr<array_vector>;

    class array_value : public ivalue
    {
        public:
            // Fields
            array_ptr value;

            // Constructor
            array_value(const array_vector &value) : value(std::make_shared<array_vector>(value)) { }
            array_value(array_ptr value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const array_value *>(input);
                if (!other)
                {
                    return 1;
                }

                const auto &this_array = *value.get();
                const auto &other_array = *other->value.get();

                auto compare_length = number_value::compare(this_array.size(), other_array.size());
                if (compare_length != 0)
                {
                    return compare_length;
                }

                for (auto i = 0; i < this_array.size(); i++)
                {
                    auto compare_value = this_array[i].compare_to(&other_array[i]);
                    if (compare_value != 0)
                    {
                        return compare_value;
                    }
                }

                return 0;
            }

            virtual std::string to_string() const
            {
                return "";
            }

            virtual std::string type_name() const
            {
                return "array";
            }
    };
} // stack_vm