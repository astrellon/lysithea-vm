#pragma once

#include <string>
#include <memory>

namespace stack_vm
{
    class scope;

    class standard_value_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

        private:
            // Constructor
            standard_value_library() { };
    };
} // stack_vm