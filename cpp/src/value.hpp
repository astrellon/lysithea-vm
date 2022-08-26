#pragma once

#include <map>
#include <vector>
#include <variant>
#include <any>
#include <sstream>

namespace stack_vm
{
    class Value;

    using object_value = std::map<std::string, Value>;
    using array_value = std::vector<Value>;

    class Value
    {
        public:
            // Fields
            std::variant<bool, double, std::string, object_value, array_value> data;

            // Constructor
            Value(bool value) : data(value) { }
            Value(int value) : data((double)value) { }
            Value(double value) : data(value) { }
            Value(const char * value) : Value(std::string(value)) { }
            Value(const std::string &value) : data(value) { }
            Value(const object_value &value) : data(value) { }
            Value(const array_value &value) : data(value) { }

            // Methods
            bool is_string() const
            {
                return data.index() == 2;
            }
            bool is_number() const
            {
                return data.index() == 1;
            }
            bool is_bool() const
            {
                return data.index() == 0;
            }
            bool is_object() const
            {
                return data.index() == 3;
            }
            bool is_array() const
            {
                return data.index() == 4;
            }
            bool is_true() const
            {
                switch (data.index())
                {
                    case 0: return std::get<bool>(data) == true;
                    default: return false;
                }
            }

            bool is_false() const
            {
                switch (data.index())
                {
                    case 0: return std::get<bool>(data) == false;
                    default: return false;
                }
            }

            std::string to_string() const
            {
                switch (data.index())
                {
                    case 0: return std::get<bool>(data) ? "true" : "false";
                    case 1: return std::to_string(std::get<double>(data));
                    case 2: return std::get<std::string>(data);
                    case 3:
                    {
                        std::stringstream ss;
                        ss << '{';
                        auto first = true;
                        for (const auto &iter : std::get<object_value>(data))
                        {
                            if (!first)
                            {
                                ss << ',';
                            }
                            first = false;

                            ss << '"';
                            ss << iter.first;
                            ss << "\":";
                            ss << iter.second.to_string();
                        }
                        ss << '}';
                        return ss.str();
                    }
                    case 4:
                    {
                        std::stringstream ss;
                        ss << '[';
                        auto first = true;
                        for (const auto &iter : std::get<array_value>(data))
                        {
                            if (!first)
                            {
                                ss << ',';
                            }
                            first = false;

                            ss << iter.to_string();
                        }
                        ss << ']';
                        return ss.str();
                    }
                    default: return "<<Unknown>>";
                }
            }
    };
} // stack_vm