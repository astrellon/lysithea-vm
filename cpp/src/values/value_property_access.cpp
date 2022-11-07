#include "value_property_access.hpp"

#include "./value.hpp"
#include "./array_value.hpp"
#include "./object_value.hpp"
#include "./string_value.hpp"

namespace stack_vm
{
    bool try_get_property(value current, const array_value &properties, value &result)
    {
        for (const auto &iter : properties.data)
        {
            int index;
            if (current.is_array() && try_parse_index(iter, index))
            {
                if (!current.get_complex()->try_get(index, current))
                {
                    return false;
                }
            }
            else if (current.is_object())
            {
                if (!current.get_complex()->try_get(iter.to_string(), current))
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

    bool try_parse_index(value input, int &result)
    {
        if (input.is_number())
        {
            result = input.get_int();
            return result >= 0;
        }

        auto is_string = input.get_complex<const string_value>();
        if (is_string)
        {
            try
            {
                result = std::stoi(is_string->data);
                return result >= 0;
            }
            catch (std::exception &exp)
            {
            }
        }

        return false;
    }
} // stack_vm