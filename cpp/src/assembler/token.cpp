#include "token.hpp"

namespace lysithea_vm
{
    value token::get_value() const
    {
        if (type == token_type::value)
        {
            return token_value;
        }
        else if (type == token_type::list)
        {
            array_vector result(list_data.size());
            std::transform(list_data.begin(), list_data.end(), result.begin(), token::convert_token);
            return array_value::make_value(result, false);
        }
        else if (type == token_type::map)
        {
            object_map result;
            for (auto kvp : map_data)
            {
                result[kvp.first] = kvp.second->get_value();
            }
            return object_value::make_value(result);
        }
        return token_value;
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