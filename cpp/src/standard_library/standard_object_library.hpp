#pragma once

#include <string>
#include <memory>

#include "../values/value.hpp"
#include "../values/object_value.hpp"

namespace stack_vm
{
    class scope;

    class standard_object_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

            static value set(const object_map &target, const std::string &key, const value &input);
            static value get(const object_map &target, const std::string &key);
            static value keys(const object_map &target);
            static value values(const object_map &target);
            static value removeKey(const value &target, const std::string &key);
            static value removeValues(const value &target, const value &input);

        private:
            // Constructor
            standard_object_library() { };

    };
}