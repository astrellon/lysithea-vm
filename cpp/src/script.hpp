#pragma once

#include <memory>

namespace stack_vm
{
    class scope;
    class function;

    class script
    {
        public:
            // Fields
            std::shared_ptr<const scope> builtin_scope;
            std::shared_ptr<function> code;

            // Constructor
            script(std::shared_ptr<const scope> builtin_scope, std::shared_ptr<function> code): builtin_scope(builtin_scope), code(code) { }

            // Methods
    };
} // stack_vm