#pragma once

#include <vector>
#include <string>
#include <unordered_map>
#include <memory>
#include <algorithm>

#include "../code_location.hpp"
#include "../values/value.hpp"

namespace lysithea_vm
{
    enum class token_type
    {
        empty, value, expression, list, map
    };

    class token;

    using token_ptr = std::shared_ptr<token>;

    class token
    {
        public:
            // Fields
            code_location location;
            token_type type;
            value token_value;
            std::vector<token_ptr> list_data;
            std::unordered_map<std::string, token_ptr> map_data;

            // Constructor
            token() = default;
            token(const token &copy) = default;
            token(const code_location &location) : location(location), type(token_type::empty) { }
            token(const code_location &location, value token_value) : location(location), type(token_type::value), token_value(token_value) { }
            token(const code_location &location, complex_ptr token_value) : location(location), type(token_type::value), token_value(token_value) { }
            token(const code_location &location, token_type type, const std::vector<token_ptr> &data) : location(location), type(type), list_data(data) { }
            token(const code_location &location, const std::unordered_map<std::string, token_ptr> &data) : location(location), type(token_type::map), map_data(data) { }

            // Methods
            std::string to_string(int indent) const;

            value get_value() const;
            value get_value_can_be_empty() const;
            token keep_location(value new_token_value) const;
            token keep_location(complex_ptr new_token_value) const;
            token to_empty() const;
            bool is_nested_expression() const;
    };
} // lysithea_vm