#pragma once

#include <string>

namespace stack_vm
{
    class virtual_machine;

    class standard_comparison_library
    {
        public:
            // Fields
            static const std::string &handle_name;

            // Methods
            static void add_handler(virtual_machine &vm);
            static void handler(const std::string &command, virtual_machine &vm);

            inline static bool greater(const value &left, const value &right)
            {
                return left.compare(right) > 0;
            }

            inline static bool greater_equals(const value &left, const value &right)
            {
                return left.compare(right) >= 0;
            }

            inline static bool equals(const value &left, const value &right)
            {
                return left.compare(right) == 0;
            }

            inline static bool not_equals(const value &left, const value &right)
            {
                return left.compare(right) != 0;
            }

            inline static bool less(const value &left, const value &right)
            {
                return left.compare(right) < 0;
            }

            inline static bool less_equals(const value &left, const value &right)
            {
                return left.compare(right) <= 0;
            }

        private:
            // Constructor
            standard_comparison_library() { };
    };
} // stack_vm