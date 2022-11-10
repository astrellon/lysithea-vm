#pragma once

#include "standard_operator_library.hpp"
#include "standard_math_library.hpp"
#include "standard_string_library.hpp"
#include "standard_array_library.hpp"
#include "standard_object_library.hpp"
#include "standard_misc_library.hpp"

namespace lysithea_vm
{
    class scope;

    class standard_library
    {
        public:
            enum library_type
            {
                none = 0,
                vm_operator = 1 << 0,
                math = 1 << 1,
                string = 1 << 2,
                array = 1 << 3,
                object = 1 << 4,
                misc = 1 << 5,
                all = (1 << 6) - 1
            };

            // Fields

            // Methods
            static void add_to_scope(scope &input, library_type libraries = all);

        private:
            // Constructor
            standard_library() { }

    };
} // lysithea_vm
