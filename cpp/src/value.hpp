#pragma once

#include <map>
#include <vector>
#include <variant>
#include <any>
#include <sstream>
#include <memory>

namespace stack_vm
{
    class value;

    using object_value = std::map<std::string, value>;
    using array_value = std::vector<value>;

    class value
    {
        public:
            // Fields
            std::variant<bool, double, std::string, std::shared_ptr<object_value>, std::shared_ptr<array_value>> data;

            // Constructor
            value(bool input) : data(input) { }
            value(int input) : data((double)input) { }
            value(unsigned int input) : data((double)input) { }
            value(double input) : data(input) { }
            value(const char * input) : value(std::string(input)) { }
            value(const std::string &input) : data(input) { }
            value(std::shared_ptr<object_value> input) : data(input) { }
            value(std::shared_ptr<array_value> input) : data(input) { }

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
                        for (const auto &iter : *std::get<std::shared_ptr<object_value>>(data))
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
                        for (const auto &iter : *std::get<std::shared_ptr<array_value>>(data))
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