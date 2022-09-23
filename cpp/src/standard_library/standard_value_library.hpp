#pragma once

#include <string>

namespace stack_vm
{
    class virtual_machine;

    class standard_value_library
    {
        public:
            // Fields
            static const std::string &handle_name;

            // Methods
            static void add_handler(virtual_machine &vm);
            static void handler(const std::string &command, virtual_machine &vm);

        private:
            // Constructor
            standard_value_library() { };
    };
} // stack_vm