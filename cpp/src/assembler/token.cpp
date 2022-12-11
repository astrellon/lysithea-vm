#include "token.hpp"

#include <sstream>

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
        switch (type)
        {
            case token_type::value:
            {
                return token_value;
            }
            case token_type::list:
            {
                array_vector result(list_data.size());
                std::transform(list_data.begin(), list_data.end(), result.begin(), token::convert_token);
                return array_value::make_value(result, false);
            }
            case token_type::map:
            {
                object_map result;
                for (auto kvp : map_data)
                {
                    result[kvp.first] = kvp.second->get_value();
                }
                return object_value::make_value(result);
            }
            default:
            {
                return token_value;
            }
        }
    }

    token token::copy(value new_token_value) const
    {
        return token(location, new_token_value);
    }

    token token::copy(complex_ptr new_token_value) const
    {
        return token(location, value(new_token_value));
    }

    token token::to_empty() const
    {
        return token(location);
    }
} // lysithea_vm