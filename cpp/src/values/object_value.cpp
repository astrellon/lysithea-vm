#include "object_value.hpp"

#include <sstream>

namespace stack_vm
{
    int object_value::compare_to(const complex_value *input) const
    {
        auto other = dynamic_cast<const object_value *>(input);
        if (!other)
        {
            return 1;
        }

        const auto &this_object = *value.get();
        const auto &other_object = *other->value.get();

        auto compare_length = value::compare(this_object.size(), other_object.size());
        if (compare_length != 0)
        {
            return compare_length;
        }

        for (auto iter = this_object.begin(); iter != this_object.end(); ++iter)
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
        for (const auto &iter : *value.get())
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
} // stack_vm