#pragma once

#include <memory>
#include <vector>
#include <string>
#include <stdexcept>

#include "./complex_value.hpp"
#include "./value.hpp"

namespace stack_vm
{
    using array_vector = std::vector<value>;
    using array_ptr = std::shared_ptr<array_vector>;

    class array_value : public complex_value
    {
        public:
            // Fields
            array_ptr value;
            bool is_arguments_value;

            // Constructor
            array_value(bool is_arguments_value)
                : value(std::make_shared<array_vector>()), is_arguments_value(is_arguments_value) {
            }
            array_value(const array_vector &value, bool is_arguments_value)
                : value(std::make_shared<array_vector>(value)), is_arguments_value(is_arguments_value) { }

            array_value(array_ptr value, bool is_arguments_value)
                : value(value), is_arguments_value(is_arguments_value) { }

            virtual ~array_value() { }

            // Helper Methods
            template <typename T>
            inline std::shared_ptr<T> get_index(int index) const
            {
                index = calc_index(index);
                if (index < 0 || index >= value->size())
                {
                    throw std::out_of_range("Error getting array at index, out of range");
                }

                auto casted = std::dynamic_pointer_cast<T>(value->at(index).get_complex());
                if (!casted)
                {
                    throw std::bad_cast();
                }

                return casted;
            }

            inline value get_index(int index) const
            {
                index = calc_index(index);
                if (index < 0 || index >= value->size())
                {
                    throw std::out_of_range("Error getting array at index, out of range");
                }

                return value->at(index);
            }

            // Value Methods
            virtual int compare_to(const complex_value *input) const;
            virtual std::string to_string() const;
            virtual std::string type_name() const
            {
                return is_arguments_value ? "arguments" : "array";
            }

            // Array methods
            virtual bool is_array() const { return true; }
            virtual int array_length() const { return static_cast<int>(value->size()); }

            virtual bool try_get(int index, std::shared_ptr<complex_value> &result) const
            {
                index = calc_index(index);
                if (index < 0 || index >= value->size())
                {
                    return false;
                }

                result = value->at(index);
                return true;
            }

            inline int calc_index(int index) const
            {
                if (index < 0)
                {
                    return static_cast<int>(value->size()) + index;
                }

                return index;
            }

            // Object methods
            virtual int object_length() const { return 1; }
            virtual bool is_object() const { return true; }
            virtual std::vector<std::string> object_keys() const
            {
                std::vector<std::string> result;
                result.push_back("length");
                return result;
            }
            virtual bool try_get(const std::string &key, stack_vm::value &result) const;

    };
} // stack_vm