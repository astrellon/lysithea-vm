#include "array_value.hpp"

#include <sstream>

#include "./builtin_function_value.hpp"
#include "./value.hpp"

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace lysithea_vm
{
    value array_value::empty(std::make_shared<array_value>(false));

    int array_value::compare_to(const complex_value *input) const
    {
        auto other = dynamic_cast<const array_value *>(input);
        if (!other)
        {
            return 1;
        }

        const auto &other_array = other->data;

        auto compare_length = compare(data.size(), other_array.size());
        if (compare_length != 0)
        {
            return compare_length;
        }

        for (auto i = 0; i < data.size(); i++)
        {
            auto compare_value = data[i].compare_to(other_array[i]);
            if (compare_value != 0)
            {
                return compare_value;
            }
        }

        return 0;
    }

    bool array_value::try_get(const std::string &key, lysithea_vm::value &result) const
    {
        if (key == "length")
        {
            result = value(data.size());
            return true;
        }

        return false;
    }

    std::string array_value::to_string() const
    {
        std::stringstream ss;
        ss << '(';
        auto first = true;
        for (const auto &iter : data)
        {
            if (!first)
            {
                ss << ' ';
            }
            first = false;

            ss << iter.to_string();
        }
        ss << ')';
        return ss.str();
    }
} // lysithea_vm