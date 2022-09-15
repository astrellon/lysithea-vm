#pragma once

#include <string>
#include <cctype>

#include "operator.hpp"

namespace stack_vm
{
    vm_operator parse_operator(const std::string &input)
    {
        auto upper_case = input;
        std::transform(upper_case.begin(), upper_case.end(), upper_case.begin(), ::toupper);

        if (upper_case == "PUSH") return vm_operator::push;
        if (upper_case == "RUN") return vm_operator::run;
        if (upper_case == "CALL") return vm_operator::call;
        if (upper_case == "JUMP") return vm_operator::jump;
        if (upper_case == "JUMPTRUE") return vm_operator::jump_true;
        if (upper_case == "JUMPFALSE") return vm_operator::jump_false;
        if (upper_case == "RETURN") return vm_operator::call_return;

        return vm_operator::unknown;
    }
} // namespace stack_vm