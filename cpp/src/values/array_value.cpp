#include "array_value.hpp"

#include <sstream>

#include "./builtin_function_value.hpp"
#include "../virtual_machine.hpp"
#include "./value.hpp"

namespace stack_vm
{
    int array_value::compare_to(const complex_value *input) const
    {
        auto other = dynamic_cast<const array_value *>(input);
        if (!other)
        {
            return 1;
        }

        const auto &this_array = *value.get();
        const auto &other_array = *other->value.get();

        auto compare_length = value::compare(this_array.size(), other_array.size());
        if (compare_length != 0)
        {
            return compare_length;
        }

        for (auto i = 0; i < this_array.size(); i++)
        {
            auto compare_value = this_array[i]->compare_to(other_array[i].get());
            if (compare_value != 0)
            {
                return compare_value;
            }
        }

        return 0;
    }

    bool array_value::try_get(const std::string &key, stack_vm::value &result) const
    {
        if (key == "length")
        {
            result = value(std::make_shared<builtin_function_value>([this](virtual_machine &vm, const array_value &args)
            {
                vm.push_stack(std::make_shared<number_value>(this->value->size()));
            }));
            return true;
        }

        return false;
    }

    std::string array_value::to_string() const
    {
        std::stringstream ss;
        ss << '(';
        auto first = true;
        for (const auto &iter : *value.get())
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
} // stack_vm