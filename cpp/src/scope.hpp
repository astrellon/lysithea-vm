#pragma once

#include <memory>
#include <string>
#include <unordered_map>

#include "./values/builtin_function_value.hpp"

namespace stack_vm
{
    class ivalue;

    class scope
    {
        public:
            // Fields
            std::unordered_map<std::string, std::shared_ptr<ivalue>> values;
            std::shared_ptr<scope> parent;

            // Constructor
            scope();
            scope(std::shared_ptr<scope> parent);

            // Methods
            void clear();
            void combine_scope(const scope &input);

            void define(const std::string &key, std::shared_ptr<ivalue> value);
            void define(const std::string &key, builtin_function_callback &callback);
            bool try_set(const std::string &key, std::shared_ptr<ivalue> value);
            bool try_get_key(const std::string &key, std::shared_ptr<ivalue> &result) const;
    };
} // stack_vm