#pragma once

#include <string>
#include <memory>

namespace stack_vm
{
    class scope;

    class standard_assert_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

        private:
            // Constructor
            standard_assert_library() { };
    };
} // stack_vm