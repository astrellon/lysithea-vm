#include "standard_library.hpp"

#include "../scope.hpp"

namespace stack_vm
{
    void standard_library::add_to_scope(scope &input, standard_library::library_type libraries)
    {
        if (libraries & standard_library::vm_operator)
        {
            input.combine_scope(*standard_operator_library::library_scope);
        }
        if (libraries & standard_library::math)
        {
            input.combine_scope(*standard_math_library::library_scope);
        }
        if (libraries & standard_library::string)
        {
            input.combine_scope(*standard_string_library::library_scope);
        }
        if (libraries & standard_library::array)
        {
            input.combine_scope(*standard_array_library::library_scope);
        }
        if (libraries & standard_library::object)
        {
            input.combine_scope(*standard_object_library::library_scope);
        }
        if (libraries & standard_library::misc)
        {
            input.combine_scope(*standard_misc_library::library_scope);
        }
    }
} // stack_vm