#include "utils.hpp"

#include <algorithm>

namespace lysithea_vm
{
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

            case vm_operator::string_concat: return "$";

            case vm_operator::add: return "+";
            case vm_operator::unary_negative:
            case vm_operator::sub: return "-";
            case vm_operator::multiply: return "*";
            case vm_operator::divide: return "/";

            case vm_operator::less_than: return "<";
            case vm_operator::less_than_equals: return "<=";
            case vm_operator::equals: return "==";
            case vm_operator::not_equals: return "!=";
            case vm_operator::greater_than: return ">";
            case vm_operator::greater_than_equals: return ">=";
            case vm_operator::inc: return "++";
            case vm_operator::dec: return "--";
            case vm_operator::op_and: return "&&";
            case vm_operator::op_or: return "||";
            case vm_operator::op_not: return "!";
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