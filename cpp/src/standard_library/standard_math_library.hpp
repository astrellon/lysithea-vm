#pragma once

#include <string>
#include <memory>

#include "../values/builtin_function_value.hpp"

namespace lysithea_vm
{
    class scope;

    class standard_math_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();


        private:
            // Constructor
            standard_math_library() { };
    };
} // lysithea_vm