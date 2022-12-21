#pragma once

#include <memory>
#include <string>
#include <unordered_map>

#include "./values/value.hpp"
#include "./values/builtin_function_value.hpp"

namespace lysithea_vm
{
    class scope
    {
        public:
            // Fields
            std::unordered_map<std::string, value> values;
            std::unordered_map<std::string, bool> constants;
            std::shared_ptr<scope> parent;

            // Constructor
            scope();
            scope(std::shared_ptr<scope> parent);

            // Methods
            void clear();
            void combine_scope(const scope &input);

            bool has_key(const std::string &key) const;
            bool try_define(const std::string &key, value input);
            bool try_set_constant(const std::string &key, value input);
            bool try_set_constant(const std::string &key, builtin_function_callback callback);
            bool try_set(const std::string &key, value input);
            bool try_get_key(const std::string &key, value &result) const;
            bool try_get_number(const std::string &key, double &result) const;
            bool try_get_bool(const std::string &key, bool &result) const;

            bool is_constant(const std::string &key) const;
            void set_constant(const std::string &key);
    };
} // lysithea_vm