#include "code_line.hpp"

#include "./values/ivalue.hpp"
#include "./utils.hpp"

namespace stack_vm
{
    std::string code_line::to_string() const
    {
        std::stringstream result;
        result << stack_vm::to_string(op) << ": ";
        if (value)
        {
            result << value->to_string();
        }
        else
        {
            result << "<no arg>";
        }
        return result.str();
    }
} // stack_vm