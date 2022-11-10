#pragma once

#include <string>
#include <memory>
#include "../values/value.hpp"

namespace lysithea_vm
{
    class scope;

    class standard_operator_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

            inline static bool greater(const value &left, const value &right)
            {
                return left.compare_to(right) > 0;
            }

            inline static bool greater_equals(const value &left, const value &right)
            {
                return left.compare_to(right) >= 0;
            }

            inline static bool equals(const value &left, const value &right)
            {
                return left.compare_to(right) == 0;
            }

            inline static bool not_equals(const value &left, const value &right)
            {
                return left.compare_to(right) != 0;
            }

            inline static bool less(const value &left, const value &right)
            {
                return left.compare_to(right) < 0;
            }

            inline static bool less_equals(const value &left, const value &right)
            {
                return left.compare_to(right) <= 0;
            }

            inline static bool not_bool(const value &input)
            {
                return !input.get_bool();
            }

        private:
            // Constructor
            standard_operator_library() { };
    };
} // lysithea_vm