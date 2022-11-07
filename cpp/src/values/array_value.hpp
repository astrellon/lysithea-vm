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

    class array_value : public complex_value
    {
        public:
            // Fields
            static value empty;

            array_vector data;
            bool is_arguments_value;

            // Constructor
            array_value(bool is_arguments_value)
                : is_arguments_value(is_arguments_value) {
            }
            array_value(const array_vector &value, bool is_arguments_value)
                : data(value), is_arguments_value(is_arguments_value) { }

            virtual ~array_value() { }

            // Helper Methods
            template <typename T>
            inline std::shared_ptr<T> get_index(int index) const
            {
                index = calc_index(index);
                if (index < 0 || index >= data.size())
                {
                    throw std::out_of_range("Error getting array at index, out of range");
                }

                auto casted = std::dynamic_pointer_cast<T>(data[index].get_complex());
                if (!casted)
                {
                    throw std::bad_cast();
                }

                return casted;
            }

            inline bool get_bool(int index) const
            {
                auto result = get_index(index);
                if (result.is_bool())
                {
                    return result.get_bool();
                }
                throw std::bad_cast();
            }

            inline double get_number(int index) const
            {
                auto result = get_index(index);
                if (result.is_number())
                {
                    return result.get_number();
                }
                throw std::bad_cast();
            }

            inline int get_int(int index) const
            {
                auto result = get_index(index);
                if (result.is_number())
                {
                    return result.get_int();
                }
                throw std::bad_cast();
            }

            inline stack_vm::value get_index(int index) const
            {
                index = calc_index(index);
                if (index < 0 || index >= data.size())
                {
                    throw std::out_of_range("Error getting array at index, out of range");
                }

                return data[index];
            }

            static inline stack_vm::value make_value(const array_vector &input, bool is_argument_value = false)
            {
                return stack_vm::value(std::make_shared<array_value>(input, is_argument_value));
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
            virtual int array_length() const { return static_cast<int>(data.size()); }

            virtual bool try_get(int index, stack_vm::value &result) const
            {
                index = calc_index(index);
                if (index < 0 || index >= data.size())
                {
                    return false;
                }

                result = data[index];
                return true;
            }

            inline int calc_index(int index) const
            {
                if (index < 0)
                {
                    return static_cast<int>(data.size()) + index;
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