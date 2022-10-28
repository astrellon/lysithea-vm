#pragma once

#include <memory>
#include <unordered_map>
#include <string>

#include "./ivalue.hpp"
#include "./number_value.hpp"

namespace stack_vm
{
    using object_map = std::unordered_map<std::string, ivalue>;
    using object_ptr = std::shared_ptr<object_map>;

    class object_value : public ivalue
    {
        public:
            // Fields
            object_ptr value;

            // Constructor
            object_value(const object_map &value) : value(std::make_shared<object_map>(value)) { }
            object_value(object_ptr value) : value(value) { }

            // Methods
            virtual int compare_to(const ivalue *input) const
            {
                auto other = dynamic_cast<const object_value *>(input);
                if (!other)
                {
                    return 1;
                }

                const auto &this_object = *value.get();
                const auto &other_object = *other->value.get();

                auto compare_length = number_value::compare(this_object.size(), other_object.size());
                if (compare_length != 0)
                {
                    return compare_length;
                }

                for (auto iter = this_object.begin(); iter != this_object.end(); ++iter)
                {
                    auto find_other = other_object.find(iter->first);
                    if (find_other == other_object.end())
                    {
                        return 1;
                    }

                    auto compare_value = iter->second.compare_to(&find_other->second);
                    if (compare_value != 0)
                    {
                        return 0;
                    }
                }

                return 0;
            }

            virtual std::string to_string() const
            {
                return "";
            }

            virtual std::string type_name() const
            {
                return "object";
            }
    };
} // stack_vm