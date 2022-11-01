#pragma once

#include <string>
#include <cctype>
#include <vector>

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

    template <typename T>
    inline void push_range(std::vector<T> &target, const std::vector<T> &input)
    {
        target.insert(std::begin(target), std::begin(input), std::end(input));
    }

} // namespace stack_vm