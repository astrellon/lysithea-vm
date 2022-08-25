#pragma once

#include <string>
#include <vector>
#include <map>

namespace stack_vm
{
    enum class ValueType
    {
        String, Number, Bool, Object, Array
    };

    union ValueData
    {
        double number;
        bool boolean;
        std::string string;

        ValueData(double value) : number(value) { }
        ValueData(bool value) : boolean(value) { }
        ValueData(const std::string &value) : string(value) { }
        ~ValueData() { }
    };

    class Value
    {
        public:
            // Fields
            const ValueType type;
            const ValueData data;

            // Constructor
            Value(char const *s) : Value(std::string(s)) { }
            Value(const std::string &value) : type(ValueType::String), data(value) { }
            Value(double value) : type(ValueType::Number), data(value) { }
            Value(bool value) : type(ValueType::Bool), data(value) { }
            ~Value()
            {
                if (type == ValueType::String)
                {
                    data.string.~basic_string();
                }
            }

            // Methods
            std::string toString() const
            {
                switch (type)
                {
                    case ValueType::String: return data.string;
                    case ValueType::Bool: return data.boolean ? "true" : "false";
                    case ValueType::Number: return std::to_string(data.number);
                }

                return "<<Unknown>>";
            }
    };
} // stack_vm