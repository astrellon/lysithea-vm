#include "temp_code_line.hpp"

namespace lysithea_vm
{
    std::string temp_code_line::to_string() const
    {
        if (is_label())
        {
            return jump_label;
        }

        std::stringstream result;
        result << lysithea_vm::to_string(op);
        // if (!argument.is_undefined())
        // {
        //     result << ": " << argument.to_string();
        // }
        // else
        // {
        //     result << ": <no arg>";
        // }

        return result.str();
    }

} // lysithea_vm