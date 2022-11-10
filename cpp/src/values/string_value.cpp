#include "string_value.hpp"

#include "./values.hpp"
#include "../virtual_machine.hpp"

namespace lysithea_vm
{
    bool string_value::try_get(const std::string &key, lysithea_vm::value &result) const
    {
        if (key == "length")
        {
            result = value(data.size());
            return true;
        }

        return false;
    }
} // lysithea_vm