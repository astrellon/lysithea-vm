#pragma once

#include <cmath>
#include <variant>
#include <memory>

#include "./string_value.hpp"

namespace stack_vm
{
    class complex_value;

    using complex_ptr = std::shared_ptr<complex_value>;

    class value
    {
        public:
            // Fields
            std::variant<bool, double, std::nullptr_t, complex_ptr> data;

            // Constructor
            value() : data(nullptr) { }
            value(bool input) : data(input) { }
            value(int input) : data(static_cast<double>(input)) { }
            value(unsigned int input) : data(static_cast<double>(input)) { }
            value(double input) : data(input) { }
            value(std::size_t input) : data(static_cast<double>(input)) { }
            value(const char * input) : value(std::make_shared<string_value>(input)) { }
            value(const std::string &input) : value(std::make_shared<string_value>(input)) { }
            value(complex_ptr input) : data(input) { }

            // Methods
            inline bool is_bool() const
            {
                return data.index() == 0;
            }
            inline bool is_number() const
            {
                return data.index() == 1;
            }
            inline bool is_null() const
            {
                return data.index() == 2;
            }
            inline bool is_complex() const
            {
                return data.index() == 3;
            }

            inline bool is_function() const
            {
                if (is_complex())
                {
                    return get_complex()->is_function();
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
                if (is_bool())
                {
                    return get_bool() == true;
                }

                return false;
            }

            inline bool is_false() const
            {
                if (is_bool())
                {
                    return get_bool() == false;
                }

                return false;
            }

            inline bool get_bool() const
            {
                return std::get<bool>(data);
            }

            inline double get_number() const
            {
                return std::get<double>(data);
            }

            inline int get_int() const
            {
                return static_cast<int>(get_number());
            }

            inline complex_ptr get_complex() const
            {
                if (is_complex())
                {
                    return std::get<complex_ptr>(data);
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
                if (other.data.index() != data.index())
                {
                    return 1;
                }

                switch (data.index())
                {
                    case 0: return get_bool() - other.get_bool();
                    case 1: return compare(get_number(), other.get_number());
                    case 2: return 0;
                    case 3:
                    {
                        return get_complex()->compare_to(other.get_complex().get());
                    }
                }

                return 1;
            }

            std::string to_string() const
            {
                switch (data.index())
                {
                    case 0: return get_bool() ? "true" : "false";
                    case 1: return std::to_string(get_number());
                    case 2: return "null";
                    case 3: return get_complex()->to_string();
                }

                return "unknown";
            }

            std::string type_name() const
            {
                switch (data.index())
                {
                    case 0: return "bool";
                    case 1: return "number";
                    case 2: return "null";
                    case 3: return get_complex()->type_name();
                }

                return "unknown";
            }

            static int compare(double left, double right)
            {
                auto diff = left - right;
                if (fabs(diff) < 0.0001)
                {
                    return 0;
                }

                return diff < 0 ? -1 : 1;
            }

            static int compare(std::size_t left, std::size_t right)
            {
                if (left == right)
                {
                    return 0;
                }

                return left < right ? -1 : 1;
            }

            static int compare(int left, int right)
            {
                auto diff = left - right;
                if (diff == 0)
                {
                    return 0;
                }

                return diff < 0 ? -1 : 1;
            }
    };
} // stack_vm