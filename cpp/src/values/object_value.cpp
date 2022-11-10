#include "object_value.hpp"

#include <sstream>

#include "../utils.hpp"

namespace lysithea_vm
{
    value object_value::empty(std::make_shared<object_value>());

    int object_value::compare_to(const complex_value *input) const
    {
        auto other = dynamic_cast<const object_value *>(input);
        if (!other)
        {
            return 1;
        }

        const auto &other_object = other->data;

        auto compare_length = compare(data.size(), other_object.size());
        if (compare_length != 0)
        {
            return compare_length;
        }

        for (auto iter = data.cbegin(); iter != data.cend(); ++iter)
        {
            auto find_other = other_object.find(iter->first);
            if (find_other == other_object.end())
            {
                return 1;
            }

            auto compare_value = iter->second.compare_to(find_other->second);
            if (compare_value != 0)
            {
                return 0;
            }
        }

        return 0;
    }

    std::string object_value::to_string() const
    {
        std::stringstream ss;
        ss << '{';
        auto first = true;
        for (const auto &iter : data)
        {
            if (!first)
            {
                ss << ' ';
            }
            first = false;

            ss << '"';
            ss << iter.first;
            ss << "\" ";
            ss << iter.second.to_string();
        }
        ss << '}';
        return ss.str();
    }
} // lysithea_vm