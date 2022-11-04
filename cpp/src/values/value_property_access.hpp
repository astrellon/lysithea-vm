#pragma once

#include <memory>

namespace stack_vm
{
    class complex_value;
    class array_value;

    bool try_get_property(std::shared_ptr<complex_value> current, const array_value &properties, std::shared_ptr<complex_value> &result);
    bool try_parse_index(std::shared_ptr<complex_value> input, int &result);
} // namespace stack_vm
