#pragma once

#include <string>
#include <memory>

#include "../values/value.hpp"
#include "../values/array_value.hpp"

namespace lysithea_vm
{
    class scope;

    class standard_array_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

            static value concat(const array_vector &target, const array_vector &input);
            static value get(const array_vector &target, int index);
            static value set(const array_vector &target, int index, const value &input);
            static value insert(const array_vector &target, int index, const value &input);
            static value insert_flatten(const array_vector &target, int index, const array_vector &input);
            static value remove_at(const array_vector &target, int index);
            static value remove(const value &target, const value &value);
            static value remove_all(const value &target, const value &value);
            static value contains(const array_vector &target, const value &value);
            static value index_of(const array_vector &target, const value &value);
            static value sublist(const array_vector &target, int index, int length);

            inline static array_vector::const_iterator get_iter(const array_vector &value, int index)
            {
                if (index < 0)
                {
                    return value.begin() + (value.size() + index);
                }
                return value.begin() + index;
            }

        private:
            // Constructor
            standard_array_library() { };
    };
} // lysithea_vm