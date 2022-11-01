#pragma once

#include <memory>

namespace stack_vm
{
    class ivalue;
    class array_value;

    bool try_get_property(std::shared_ptr<ivalue> current, const array_value &properties, std::shared_ptr<ivalue> &result);
    bool try_parse_index(std::shared_ptr<ivalue> input, int &result);
} // namespace stack_vm
