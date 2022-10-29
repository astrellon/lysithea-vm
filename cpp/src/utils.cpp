#include "utils.hpp"

#include <algorithm>

namespace stack_vm
{

    vm_operator parse_operator(const std::string &input)
    {
        auto upper_case = input;
        std::transform(upper_case.begin(), upper_case.end(), upper_case.begin(), ::toupper);

        if (upper_case == "PUSH") return vm_operator::push;
        if (upper_case == "TOARGUMENT") return vm_operator::to_argument;
        if (upper_case == "CALL") return vm_operator::call;
        if (upper_case == "CALLDIRECT") return vm_operator::call_direct;
        if (upper_case == "RETURN") return vm_operator::call_return;
        if (upper_case == "GETPROPERTY") return vm_operator::get_property;
        if (upper_case == "GET") return vm_operator::get;
        if (upper_case == "SET") return vm_operator::set;
        if (upper_case == "DEFINE") return vm_operator::define;
        if (upper_case == "JUMP") return vm_operator::jump;
        if (upper_case == "JUMPTRUE") return vm_operator::jump_true;
        if (upper_case == "JUMPFALSE") return vm_operator::jump_false;

        return vm_operator::unknown;
    }

    int compare(double v1, double v2)
    {
        auto diff = v1 - v2;
        if (std::abs(diff) < 0.0001)
        {
            return 0;
        }
        if (diff < 0.0)
        {
            return -1;
        }
        return 1;
    }

    int compare(int v1, int v2)
    {
        auto diff = v1 - v2;
        if (diff == 0)
        {
            return 0;
        }

        if (diff < 0)
        {
            return -1;
        }
        return 1;
    }

    int compare(std::size_t v1, std::size_t v2)
    {
        long diff = (long)(v1 - v2);
        if (diff == 0)
        {
            return 0;
        }

        if (diff < 0)
        {
            return -1;
        }
        return 1;
    }
} // stack_vm