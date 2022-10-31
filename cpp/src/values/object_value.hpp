#pragma once

#include <memory>
#include <unordered_map>
#include <string>

#include "./ivalue.hpp"
#include "./number_value.hpp"

namespace stack_vm
{
    using object_map = std::unordered_map<std::string, std::shared_ptr<ivalue>>;
    using object_ptr = std::shared_ptr<object_map>;

    class object_value : public iobject_value
    {
        public:
            // Fields
            object_ptr value;

            // Constructor
            object_value(const object_map &value) : value(std::make_shared<object_map>(value)) { }
            object_value(object_ptr value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const;
            virtual std::string to_string() const;

            virtual std::string type_name() const
            {
                return "object";
            }

            virtual std::vector<std::string> object_keys() const
            {
                std::vector<std::string> result;

                for (const auto iter : *value.get())
                {
                    result.push_back(iter.first);
                }

                return result;
            }

            virtual bool try_get(const std::string &key, std::shared_ptr<ivalue> &result) const
            {
                auto find = value->find(key);
                if (find == value->end())
                {
                    return false;
                }

                result = find->second;
                return true;
            }
    };
} // stack_vm