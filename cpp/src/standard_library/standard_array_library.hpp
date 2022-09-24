#pragma once

#include <string>
#include "../value.hpp"

namespace stack_vm
{
    class virtual_machine;

    class standard_array_library
    {
        public:
            // Fields
            static const std::string &handle_name;

            // Methods
            static void add_handler(virtual_machine &vm);
            static void handler(const std::string &command, virtual_machine &vm);

            static value append(const value &target, const value &input);
            static value prepend(const value &target, const value &input);
            static value concat(const value &target, const array_value &input);
            static value set(const value &target, int index, const value &input);
            static value insert(const value &target, int index, const value &input);
            static value insert_flatten(const value &target, int index, const array_value &input);
            static value remove_at(const value &target, int index);
            static value remove(const value &target, const value &value);
            static value remove_all(const value &target, const value &value);
            static value contains(const value &target, const value &value);
            static value index_of(const value &target, const value &value);
            static value sublist(const value &target, int index, int length);

            inline static array_ptr copy(const value &target)
            {
                return std::make_shared<array_value>(*target.get_array());
            }

        private:
            // Constructor
            standard_array_library() { };
    };
} // stack_vm