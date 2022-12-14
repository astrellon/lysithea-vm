#pragma once

#include <cmath>
#include <memory>
#include <sstream>

#include "./string_value.hpp"
#include "./builtin_function_value.hpp"
#include "../utils.hpp"

namespace lysithea_vm
{
    class complex_value;

    using complex_ptr = std::shared_ptr<complex_value>;

    enum class value_type
    {
        undefined, null, is_true, is_false, number, complex
    };

    class value
    {
        public:
            // Fields
            value_type type;
            double number;
            complex_ptr data;

            // Constructor
            value() : type(value_type::undefined) { }
            value(bool input) : type(input == true ? value_type::is_true : value_type::is_false) { }
            value(int input) : type(value_type::number), number(static_cast<double>(input)) { }
            value(unsigned int input) : type(value_type::number), number(static_cast<double>(input)) { }
            value(double input) : type(value_type::number), number(input) { }
            value(std::size_t input) : type(value_type::number), number(static_cast<double>(input)) { }
            value(const char * input) : type(value_type::complex), data(std::make_shared<string_value>(input)) { }
            value(const std::string &input) : type(value_type::complex), data(std::make_shared<string_value>(input)) { }
            value(complex_ptr input) : type(value_type::complex), data(input) { }

            // Methods
            inline bool is_bool() const
            {
                return type == value_type::is_true || type == value_type::is_false;
            }
            inline bool is_number() const
            {
                return type == value_type::number;
            }
            inline bool is_undefined() const
            {
                return type == value_type::undefined;
            }
            inline bool is_null() const
            {
                return type == value_type::null;
            }
            inline bool is_complex() const
            {
                return type == value_type::complex;
            }

            inline bool is_function() const
            {
                if (is_complex())
                {
                    return get_complex()->is_function();
                }
                return false;
            }

            inline bool is_string() const
            {
                if (is_complex())
                {
                    return get_complex()->is_string();
                }
                return false;
            }

            inline bool is_array() const
            {
                if (is_complex())
                {
                    return get_complex()->is_array();
                }
                return false;
            }

            inline bool is_object() const
            {
                if (is_complex())
                {
                    return get_complex()->is_object();
                }
                return false;
            }

            inline bool is_true() const
            {
                return type == value_type::is_true;
            }

            inline bool is_false() const
            {
                return type == value_type::is_false;
            }

            inline bool get_bool() const
            {
                return type == value_type::is_true;
            }

            inline double get_number() const
            {
                return number;
            }

            inline int get_int() const
            {
                return static_cast<int>(number);
            }

            inline complex_ptr get_complex() const
            {
                if (is_complex())
                {
                    return data;
                }
                return nullptr;
            }

            template <typename T>
            inline std::shared_ptr<T> get_complex() const
            {
                return std::dynamic_pointer_cast<T>(get_complex());
            }

            int compare_to(const value &other) const
            {
                if (other.type != type)
                {
                    return 1;
                }

                switch (type)
                {
                    case value_type::is_false:
                    case value_type::is_true:
                    case value_type::undefined:
                    case value_type::null:
                        return 0;
                    case value_type::number:
                        return compare(get_number(), other.get_number());
                    case value_type::complex:
                        return get_complex()->compare_to(other.get_complex().get());
                    default: break;
                }

                return 1;
            }

            std::string to_string() const
            {
                switch (type)
                {
                    case value_type::is_true: return "true";
                    case value_type::is_false: return "false";
                    case value_type::null: return "null";
                    case value_type::number:
                    {
                        std::stringstream ss;
                        ss << std::noshowpoint << get_number();
                        return ss.str();
                    }
                    case value_type::complex: return get_complex()->to_string();
                    default: break;
                }

                return "undefined";
            }

            std::string type_name() const
            {
                switch (type)
                {
                    case value_type::is_true:
                    case value_type::is_false:
                        return "bool";
                    case value_type::number: return "number";
                    case value_type::null: return "null";
                    case value_type::complex: return get_complex()->type_name();
                    default: break;
                }

                return "unknown";
            }

            inline static value make_builtin(builtin_function_callback input)
            {
                return value(std::make_shared<builtin_function_value>(input));
            }

            inline static value make_null()
            {
                return value(value_type::null);
            }

            inline static value make_undefined()
            {
                return value(value_type::undefined);
            }

        private:
            // Constructor
            value(value_type type) : type(type) { }
    };
} // lysithea_vm