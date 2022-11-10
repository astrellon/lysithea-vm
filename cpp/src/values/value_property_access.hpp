#pragma once

#include <memory>

namespace lysithea_vm
{
    class value;
    class array_value;

    bool try_get_property(value current, const array_value &properties, value &result);
    bool try_parse_index(value input, int &result);
} // namespace lysithea_vm
