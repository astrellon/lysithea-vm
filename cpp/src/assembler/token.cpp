#include "token.hpp"

#include <sstream>

#include "../errors/assembler_error.hpp"

namespace lysithea_vm
{
    std::string token::to_string(int indent) const
    {
        std::stringstream ss;
        if (indent > 0)
        {
            ss << std::string(indent - 1, ' ') << '-';
        }

        ss << '[' << location.start_line_number << ", " << location.end_column_number << ']';
        if (location.end_line_number > location.start_line_number || location.end_column_number > location.start_column_number)
        {
            ss << " -> [" << location.end_line_number << ", " << location.end_column_number << ']';
        }

        switch (type)
        {
            case token_type::empty:
            {
                ss << " (empty)";
                break;
            }
            case token_type::expression:
            {
                ss << " (expression): " << list_data.size() << '\n';
                for (auto iter : list_data)
                {
                    ss << iter->to_string(indent + 2);
                }
                break;
            }
            case token_type::value:
            {
                ss << " (value): " << token_value.to_string();
                break;
            }
            case token_type::list:
            {
                ss << " (list): " << list_data.size() << '\n';
                for (auto iter : list_data)
                {
                    ss << iter->to_string(indent + 2);
                }
                break;
            }
            case token_type::map:
            {
                ss << " (map): " << map_data.size() << '\n';
                for (auto iter : map_data)
                {
                    ss << std::string(indent, ' ') << iter.first << ":\n" << iter.second->to_string(indent + 2) << '\n';
                }
                break;
            }
        }

        return ss.str();
    }

    value token::get_value() const
    {
        if (type == token_type::value)
        {
            return token_value;
        }

        throw assembler_error::create(*this, "Cannot get value of token");
    }

    value token::get_value_can_be_empty() const
    {
        if (type == token_type::empty)
        {
            return value();
        }

        return get_value();
    }

    token token::keep_location(value new_token_value) const
    {
        return token(location, new_token_value);
    }

    token token::keep_location(complex_ptr new_token_value) const
    {
        return token(location, value(new_token_value));
    }

    token token::to_empty() const
    {
        return token(location);
    }
} // lysithea_vm