#pragma once

#include <string>
#include "../value.hpp"

namespace stack_vm
{
    class virtual_machine;

    class standard_object_library
    {
        public:
            // Fields
            static const std::string &handle_name;

            // Methods
            static void add_handler(virtual_machine &vm);
            static void handler(const std::string &command, virtual_machine &vm);

            static value set(const value &target, const std::string &key, const value &input);
            static value get(const value &target, const std::string &key);

            inline static object_ptr copy(const value &target)
            {
                return std::make_shared<object_value>(target.get_object());
            }

        private:
            // Constructor
            standard_object_library() { };

    };
}