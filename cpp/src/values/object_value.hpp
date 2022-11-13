#pragma once

#include <memory>
#include <map>
#include <string>

#include "./complex_value.hpp"
#include "./value.hpp"

namespace lysithea_vm
{
    using object_map = std::map<std::string, value>;

    class object_value : public complex_value
    {
        public:
            // Fields
            static value empty;
            object_map data;

            // Constructor
            object_value() { }
            object_value(const object_map &data) : data(data) { }

            // Methods
            virtual int compare_to(const complex_value *input) const;
            virtual std::string to_string() const;

            virtual std::string type_name() const { return "object"; }
            virtual bool is_object() const { return true; }

            virtual std::vector<std::string> object_keys() const
            {
                std::vector<std::string> result;

                for (const auto &iter : data)
                {
                    result.push_back(iter.first);
                }

                return result;
            }

            virtual bool try_get(const std::string &key, lysithea_vm::value &result) const
            {
                auto find = data.find(key);
                if (find == data.end())
                {
                    return false;
                }

                result = find->second;
                return true;
            }

            static inline lysithea_vm::value make_value(const object_map &input)
            {
                return lysithea_vm::value(std::make_shared<object_value>(input));
            }
    };
} // lysithea_vm