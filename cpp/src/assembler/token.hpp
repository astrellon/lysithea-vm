#pragma once

#include <vector>
#include <unordered_map>
#include <memory>
#include <algorithm>

#include "../code_location.hpp"
#include "../values/array_value.hpp"
#include "../values/object_value.hpp"

namespace lysithea_vm
{
    class token;

    class itoken
    {
        public:
            // Fields
            const code_location location;

            // Constructor
            itoken (const code_location &location) : location(location) { }
            virtual ~itoken() { }

            // Methods
            token copy(value new_token_value) const;
            token to_empty() const;

            virtual value get_value() const = 0;
    };

    class token : public itoken
    {
        public:
            // Fields
            value token_value;

            // Constructor
            token (const code_location &location, value token_value) : itoken(location), token_value(token_value) { }
            token (const code_location &location) : itoken(location), token_value(value()) { }

            // Methods
            virtual value get_value() const
            {
                return token_value;
            }
    };

    class token_list : public itoken
    {
        public:
            // Fields
            std::vector<std::shared_ptr<itoken>> data;

            // Constructor
            token_list (const code_location &location, const std::vector<std::shared_ptr<itoken>> &data) : itoken(location), data(data) { }
            virtual ~token_list() { }

            // Methods
            virtual value get_value() const
            {
                array_vector result(data.size());
                std::transform(data.begin(), data.end(), result.begin(), token_list::convert_token);
                return array_value::make_value(result, false);
            }

        private:
            // Fields

            // Methods
            static value convert_token(std::shared_ptr<itoken> input)
            {
                return input->get_value();
            }
    };

    class token_map : public itoken
    {
        public:
            // Fields
            std::unordered_map<std::string, std::shared_ptr<itoken>> data;

            // Constructor
            token_map (const code_location &location, const std::unordered_map<std::string, std::shared_ptr<itoken>> &data) : itoken(location), data(data) { }
            virtual ~token_map() {}

            // Methods
            virtual value get_value() const
            {
                object_map result;
                for (auto kvp : data)
                {
                    result[kvp.first] = kvp.second->get_value();
                }
                return object_value::make_value(result);
            }
    };
} // lysithea_vm