#include "utils.hpp"

#include <algorithm>

namespace lysithea_vm
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

    std::string to_string(vm_operator input)
    {
        switch (input)
        {
            case vm_operator::call: return "call";
            case vm_operator::call_direct: return "callDirect";
            case vm_operator::call_return: return "return";
            case vm_operator::define: return "define";
            case vm_operator::get: return "get";
            case vm_operator::get_property: return "getProperty";
            case vm_operator::jump: return "jump";
            case vm_operator::jump_false: return "jumpFalse";
            case vm_operator::jump_true: return "jumpTrue";
            case vm_operator::push: return "push";
            case vm_operator::set: return "set";
            case vm_operator::to_argument: return "toArgument";
            default: break;
        }

        return "unknown";
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

    bool starts_with_unpack(const std::string &input)
    {
        return input.length() > 3 && input[0] == '.' && input[1] == '.' && input[2] == '.';
    }

    std::vector<std::string> string_split(const std::string &input, const std::string &delimiter)
    {
        std::vector<std::string> result;
        int start = 0;
        int end = input.find(delimiter);
        while (end != input.npos)
        {
            result.emplace_back(input.substr(start, end - start));
            start = end + delimiter.size();
            end = input.find(delimiter, start);
        }

        if (start != end)
        {
            result.emplace_back(input.substr(start));
        }

        return result;
    }
} // lysithea_vm