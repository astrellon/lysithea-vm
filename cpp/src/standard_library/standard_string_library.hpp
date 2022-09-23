#pragma once

#include <string>

namespace stack_vm
{
    class virtual_machine;
    class value;

    class standard_string_library
    {
        public:
            // Fields
            static const std::string &handle_name;

            // Methods
            static void add_handler(virtual_machine &vm);
            static void handler(const std::string &command, virtual_machine &vm);

            static value append(const value &target, const std::string &input);
            static value prepend(const value &target, const std::string &input);
            static value get(const value &target, int index);
            static value set(const value &target, int index, const std::string &input);
            static value insert(const value &target, int index, const std::string &input);
            static value substring(const value &target, int index, int length);

        private:
            // Constructor
            standard_string_library() { };
    };
} // stack_vm