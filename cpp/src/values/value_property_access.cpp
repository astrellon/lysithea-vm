#include "value_property_access.hpp"

#include "./array_value.hpp"
#include "./number_value.hpp"
#include "./string_value.hpp"

namespace stack_vm
{
    bool try_get_property(std::shared_ptr<complex_value> current, const array_value &properties, std::shared_ptr<complex_value> &result)
    {
        for (const auto &iter : *properties.value)
        {
            int index;
            if (current->is_array() && try_parse_index(iter, index))
            {
                if (!current->try_get(index, current))
                {
                    return false;
                }
            }
            else if (current->is_object())
            {
                if (!current->try_get(iter->to_string(), current))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        result = current;
        return true;
    }

    bool try_parse_index(std::shared_ptr<complex_value> input, int &result)
    {
        auto is_number = dynamic_cast<const number_value *>(input.get());
        if (is_number)
        {
            result = is_number->int_value();
            return result >= 0;
        }

        auto is_string = dynamic_cast<const string_value *>(input.get());
        if (is_string)
        {
            result = std::stoi(*is_string->value);
            return result >= 0;
        }

        return false;
    }
} // stack_vm