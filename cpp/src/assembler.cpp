#include "assembler.hpp"

#include "utils.hpp"

namespace stack_vm
{
    std::vector<std::shared_ptr<scope>> assembler::parse_scopes(const json &j)
    {
        std::vector<std::shared_ptr<scope>> result;

        for (auto &scope_json : j)
        {
            result.push_back(parse_scope(scope_json));
        }

        return result;
    }

    std::shared_ptr<scope> assembler::parse_scope(const json &j)
    {
        auto name = j.at("name").get<std::string>();
        auto data_json = j.at("data");
        std::vector<temp_code_line> temp_code;

        for (auto &data_line_json : data_json)
        {
            for (auto &line : parse_code_line(data_line_json))
            {
                temp_code.push_back(line);
            }
        }

        std::vector<code_line> code;
        std::map<std::string, int> labels;
        for (auto &temp_code_line : temp_code)
        {
            if (temp_code_line.is_label())
            {
                labels[temp_code_line.jump_label] = code.size();
            }
            else
            {
                code.emplace_back(temp_code_line.op, temp_code_line.argument);
            }
        }

        return std::make_shared<scope>(name, code, labels);
    }

    std::vector<assembler::temp_code_line> assembler::parse_code_line(const json &j)
    {
        auto string_input = false;
        std::string first;
        if (j.type() == json::value_t::string)
        {
            string_input = true;
            first = j.get_to(first);
        }
        else
        {
            j.begin()->get_to(first);
        }

        if (first[0] == ':')
        {
            return std::vector<temp_code_line>{ first };
        }

        std::vector<temp_code_line> result;
        auto op_code = vm_operator::unknown;
        std::optional<value> code_line_input;
        if (first[0] == '$')
        {
            op_code = vm_operator::run;
            code_line_input = value(first.substr(1));
        }
        else
        {
            op_code = parse_operator(first);
            if (op_code == vm_operator::unknown)
            {
                throw std::runtime_error("Unknown operator");
            }
        }

        if (!string_input)
        {
            for (auto iter = j.begin() + 1; iter != j.end(); ++iter)
            {
                auto parsed_value = parse_json_value(iter.value());
                if (!parsed_value.has_value())
                {
                    throw std::runtime_error("Error parsing code line");
                }

                result.emplace_back(vm_operator::push, parsed_value.value());
            }
        }

        if (op_code != vm_operator::push)
        {
            if (code_line_input.has_value())
            {
                result.emplace_back(vm_operator::push, code_line_input.value());
            }
            result.emplace_back(op_code);
        }

        return result;
    }

    std::optional<value> assembler::parse_json_value(const json &j)
    {
        auto type = j.type();
        switch (type)
        {
            case json::value_t::string:
            {
                return value(j.get<std::string>());
            }
            case json::value_t::boolean:
            {
                return value(j.get<bool>());
            }
            case json::value_t::number_float:
            {
                return value(j.get<double>());
            }
            case json::value_t::number_integer:
            {
                return value(j.get<int>());
            }
            case json::value_t::number_unsigned:
            {
                return value(j.get<unsigned int>());
            }
            case json::value_t::object:
            {
                auto obj = std::make_shared<object_value>();
                for (auto iter = j.begin(); iter != j.end(); ++iter)
                {
                    auto parsed_value = parse_json_value(iter.value());
                    if (!parsed_value.has_value())
                    {
                        throw std::runtime_error("Error parsing object value");
                    }
                    obj->emplace(iter.key(), parsed_value.value());
                }
                return value(obj);
            }
            case json::value_t::array:
            {
                auto arr = std::make_shared<array_value>();
                for (auto &item : j)
                {
                    auto parsed_value = parse_json_value(item);
                    if (!parsed_value.has_value())
                    {
                        throw std::runtime_error("Error parsing array value");
                    }
                    arr->push_back(parsed_value.value());
                }
                return value(arr);
            }
            default: return nullptr;
        }

        return nullptr;
    }

} // stack_vm