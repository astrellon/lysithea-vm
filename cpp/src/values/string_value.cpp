#include "string_value.hpp"

#include "./values.hpp"
#include "../virtual_machine.hpp"

namespace stack_vm
{
    bool string_value::try_get(const std::string &key, stack_vm::value &result) const
    {
        if (key == "length")
        {
            result = value(data.size());
            return true;
        }

        return false;
    }
} // stack_vm