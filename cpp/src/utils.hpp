#pragma once

#include <string>
#include <cctype>

#include "operator.hpp"

namespace stack_vm
{
    vm_operator parse_operator(const std::string &input);

    int compare(double v1, double v2);
    int compare(int v1, int v2);
    int compare(std::size_t v1, std::size_t v2);
} // namespace stack_vm