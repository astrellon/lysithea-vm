#pragma once

#include <string>
#include <memory>
#include "../value.hpp"

namespace stack_vm
{
    class virtual_machine;

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
            static value remove_at(const value &target, int index);
            static value remove_all(const value &target, const std::string &values);

            inline static int get_index(const string_ptr &input, int index)
            {
                if (index < 0)
                {
                    return input->size() + index;
                }

                return index;
            }

            inline static int get_index(const value &input, int index)
            {
                if (index < 0)
                {
                    return input.get_string()->size() + index;
                }

                return index;
            }

        private:
            // Constructor
            standard_string_library() { };
    };
} // stack_vm