#include "code_line.hpp"

#include "./values/complex_value.hpp"
#include "./utils.hpp"

namespace lysithea_vm
{
    std::string code_line::to_string() const
    {
        std::stringstream result;
        result << lysithea_vm::to_string(op) << ": ";
        if (!value.is_undefined())
        {
            result << value.to_string();
        }
        else
        {
            result << "<no arg>";
        }
        return result.str();
    }
} // lysithea_vm