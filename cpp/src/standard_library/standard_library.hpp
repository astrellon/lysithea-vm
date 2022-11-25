#pragma once

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
                math = 1 << 0,
                string = 1 << 1,
                array = 1 << 2,
                object = 1 << 3,
                misc = 1 << 4,
                all = (1 << 5) - 1
            };

            // Fields

            // Methods
            static void add_to_scope(scope &input, library_type libraries = all);

        private:
            // Constructor
            standard_library() { }

    };
} // lysithea_vm
