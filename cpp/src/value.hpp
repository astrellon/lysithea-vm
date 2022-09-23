#pragma once

#include <iostream>
#include <map>
#include <vector>
#include <variant>
#include <any>
#include <sstream>
#include <memory>

#include "utils.hpp"

namespace stack_vm
{
    class value;

    using object_value = std::map<std::string, value>;
    using array_value = std::vector<value>;
    using string_ptr = std::shared_ptr<std::string>;
    using object_ptr = std::shared_ptr<object_value>;
    using array_ptr = std::shared_ptr<array_value>;

    class value
    {
        public:
            // Fields
            std::variant<bool, double, string_ptr, object_ptr, array_ptr> data;

            // Constructor
            value() : data(false) { }
            value(bool input) : data(input) { }
            value(int input) : data((double)input) { }
            value(unsigned int input) : data((double)input) { }
            value(double input) : data(input) { }
            value(const char * input) : value(std::make_shared<std::string>(input)) { }
            value(const std::string &input) : value(std::make_shared<std::string>(input)) { }
            value(string_ptr input) : data(input) { }
            value(object_ptr input) : data(input) { }
            value(array_ptr input) : data(input) { }

            // Methods
            inline bool is_string() const
            {
                return data.index() == 2;
            }
            inline bool is_number() const
            {
                return data.index() == 1;
            }
            inline bool is_bool() const
            {
                return data.index() == 0;
            }
            inline bool is_object() const
            {
                return data.index() == 3;
            }
            inline bool is_array() const
            {
                return data.index() == 4;
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

            inline string_ptr get_string() const
            {
                return std::get<string_ptr>(data);
            }

            inline object_ptr get_object() const
            {
                return std::get<object_ptr>(data);
            }

            inline array_ptr get_array() const
            {
                return std::get<array_ptr>(data);
            }

            int compare(const value &other) const
            {
                if (other.data.index() != data.index())
                {
                    return 1;
                }

                switch (data.index())
                {
                    case 0: return get_bool() - other.get_bool();
                    case 1: return stack_vm::compare(get_number(), other.get_number());
                    case 2: return get_string()->compare(*other.get_string());
                    case 3:
                    {
                        auto this_object = get_object();
                        auto other_object = other.get_object();
                        auto compare_length = stack_vm::compare(this_object->size(), other_object->size());
                        if (compare_length != 0)
                        {
                            return compare_length;
                        }

                        // for (auto i = 0; i < this_object->size(); i++)
                        for (auto iter = this_object->begin(); iter != this_object->end(); ++iter)
                        {
                            auto find_other = other_object->find(iter->first);
                            if (find_other == other_object->end())
                            {
                                return 1;
                            }

                            auto compare_value = iter->second.compare(find_other->second);
                            if (compare_value != 0)
                            {
                                return 0;
                            }
                        }

                        return 0;
                    }
                    case 4:
                    {
                        auto this_array = get_array();
                        auto other_array = other.get_array();
                        auto compare_length = stack_vm::compare(this_array->size(), other_array->size());
                        if (compare_length != 0)
                        {
                            return compare_length;
                        }

                        for (auto i = 0; i < this_array->size(); i++)
                        {
                            auto compare_value = this_array->at(i).compare(other_array->at(i));
                            if (compare_value != 0)
                            {
                                return 0;
                            }
                        }

                        return 0;
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
                    case 2: return *get_string();
                    case 3:
                    {
                        std::stringstream ss;
                        ss << '{';
                        auto first = true;
                        auto object = get_object();
                        for (const auto &iter : *object)
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
                        auto array = get_array();
                        for (const auto &iter : *array)
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

            std::string type() const
            {
                switch (data.index())
                {
                    case 0: return "bool";
                    case 1: return "number";
                    case 2: return "string";
                    case 3: return "object";
                    case 4: return "array";
                }

                return "unknown";
            }
    };
} // stack_vm