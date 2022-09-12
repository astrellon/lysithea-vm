#pragma once

#include <string>
#include <cctype>

#include "operator.hpp"

namespace stack_vm
{

    bool equals_ignore_case(const std::string &str1, const std::string &str2)
    {
        return ((str1.size() == str2.size()) &&
            std::equal(str1.begin(), str1.end(), str2.begin(), [](char c1, char c2)
            { return (c1 == c2 || std::toupper(c1) == std::toupper(c2)); }));
    }

    vm_operator parse_operator(const std::string &input)
    {
        if (equals_ignore_case(input, "PUSH")) return vm_operator::push;
        if (equals_ignore_case(input, "RUN")) return vm_operator::run;
        if (equals_ignore_case(input, "CALL")) return vm_operator::call;
        if (equals_ignore_case(input, "JUMP")) return vm_operator::jump;
        if (equals_ignore_case(input, "JUMPTRUE")) return vm_operator::jump_true;
        if (equals_ignore_case(input, "JUMPFALSE")) return vm_operator::jump_false;
        if (equals_ignore_case(input, "RETURN")) return vm_operator::call_return;

        return vm_operator::unknown;
    }
} // namespace stack_vm