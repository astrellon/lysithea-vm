#pragma once

#include <memory>
#include <string>
#include <map>

#include "./values/value.hpp"
#include "./values/builtin_function_value.hpp"

namespace lysithea_vm
{
    class scope
    {
        public:
            // Fields
            std::map<std::string, value> values;
            std::shared_ptr<scope> parent;

            // Constructor
            scope();
            scope(std::shared_ptr<scope> parent);

            // Methods
            void clear();
            void combine_scope(const scope &input);

            void define(const std::string &key, value input);
            void define(const std::string &key, builtin_function_callback callback);
            bool try_set(const std::string &key, value input);
            bool try_get_key(const std::string &key, value &result) const;
    };
} // lysithea_vm