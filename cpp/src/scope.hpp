#pragma once

#include <memory>
#include <string>
#include <unordered_map>

#include "./values/value.hpp"
#include "./values/builtin_function_value.hpp"

namespace stack_vm
{
    class complex_value;

    class scope
    {
        public:
            // Fields
            std::unordered_map<std::string, value> values;
            std::shared_ptr<scope> parent;

            // Constructor
            scope();
            scope(std::shared_ptr<scope> parent);

            // Methods
            void clear();
            void combine_scope(const scope &input);

            void define(const std::string &key, value value);
            void define(const std::string &key, builtin_function_callback callback);
            bool try_set(const std::string &key, value value);
            bool try_get_key(const std::string &key, value &result) const;
    };
} // stack_vm