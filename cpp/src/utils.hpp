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

    constexpr uint32_t hash(const std::string_view &input) noexcept
    {
        uint32_t hash = 5381;

        for(const char *c = input.data(); c < input.data() + input.size(); ++c)
            hash = ((hash << 5) + hash) + (unsigned char) *c;

        return hash;
    }

} // namespace stack_vm