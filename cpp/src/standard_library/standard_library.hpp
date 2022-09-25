#pragma once

#include "standard_comparison_library.hpp"
#include "standard_math_library.hpp"
#include "standard_string_library.hpp"
#include "standard_array_library.hpp"
#include "standard_object_library.hpp"
#include "standard_value_library.hpp"

namespace stack_vm
{
    class standard_library
    {
        public:
            enum library_type
            {
                none = 0,
                comparison = 1 << 0,
                math = 1 << 1,
                string = 1 << 2,
                array = 1 << 3,
                object = 1 << 4,
                value = 1 << 5,
                all = (1 << 6) - 1
            };

            // Fields

            // Methods
            static void add_to_virtual_machine(virtual_machine &vm, library_type libraries = all);

        private:
            // Constructor
            standard_library() { }

    };
} // stack_vm
