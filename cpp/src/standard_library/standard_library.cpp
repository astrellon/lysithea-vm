#include "standard_library.hpp"

#include "../virtual_machine.hpp"

namespace stack_vm
{
    void standard_library::add_to_virtual_machine(virtual_machine &vm, standard_library::library_type libraries)
    {
        if (libraries & standard_library::comparison)
        {
            standard_comparison_library::add_handler(vm);
        }
        if (libraries & standard_library::math)
        {
            standard_math_library::add_handler(vm);
        }
        if (libraries & standard_library::string)
        {
            standard_string_library::add_handler(vm);
        }
        if (libraries & standard_library::array)
        {
            standard_array_library::add_handler(vm);
        }
        if (libraries & standard_library::object)
        {
            standard_object_library::add_handler(vm);
        }
        if (libraries & standard_library::value)
        {
            standard_value_library::add_handler(vm);
        }
    }
} // stack_vm