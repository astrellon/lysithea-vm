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
            if (data_line_json.type() == json::value_t::array)
            {
                for (auto &line : parse_code_line(data_line_json))
                {
                    temp_code.push_back(line);
                }
            }
            else
            {
                auto temp_json_array = json::array();
                temp_json_array.push_back(data_line_json);

                for (auto &line : parse_code_line(temp_json_array))
                {
                    temp_code.push_back(line);
                }
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
        std::vector<temp_code_line> result;

        if (j.size() == 0)
        {
            return result;
        }

        const json &first = j.front();
        std::string first_string_token;

        if (first.type() == json::value_t::string)
        {
            first.get_to(first_string_token);
        }

        if (first_string_token.size() > 0 && first_string_token[0] == ':')
        {
            return std::vector<temp_code_line>{ first_string_token };
        }

        auto op_code = parse_operator(first_string_token);
        auto push_child_offset = 1;
        std::optional<value> code_line_input;
        if (op_code == vm_operator::unknown)
        {
            op_code = vm_operator::run;
            code_line_input = parse_run_command(first);
            if (!code_line_input.has_value())
            {
                throw std::runtime_error("Error parsing code line");
            }
            push_child_offset = 0;
        }
        else if (j.size() > 1)
        {
            if (is_jump_call(op_code))
            {
                code_line_input = parse_jump_label(j.back());
                if (!code_line_input.has_value())
                {
                    throw std::runtime_error("Error parsing jump style opcode line");
                }
            }
            else
            {
                code_line_input = parse_json_value(j.back());
                if (!code_line_input.has_value())
                {
                    throw std::runtime_error("Error parsing code line");
                }
            }
        }

        for (auto i = 1; i < j.size() - push_child_offset; i++)
        {
            const auto &item = j.at(i);
            auto parsed_value = parse_json_value(item);
            if (!parsed_value.has_value())
            {
                throw std::runtime_error("Error parsing code line");
            }

            result.emplace_back(vm_operator::push, parsed_value.value());
        }

        if (code_line_input.has_value())
        {
            result.emplace_back(op_code, code_line_input.value());
        }
        else
        {
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
                return value(std::make_shared<std::string>(j.get<std::string>()));
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

    std::optional<value> assembler::parse_run_command(const json &j)
    {
        return parse_two_string_input(j, '.', false);
    }

    std::optional<value> assembler::parse_jump_label(const json &j)
    {
        return parse_two_string_input(j, ':', true);
    }

    std::optional<value> assembler::parse_two_string_input(const json &j, char delimiter, bool include_delimiter)
    {
        if (j.type() == json::value_t::string)
        {
            auto str = j.get<std::string>();
            auto delimiter_find = str.find(delimiter);
            if (delimiter_find != str.npos && delimiter_find > 0)
            {
                auto arr = std::make_shared<array_value>();
                arr->emplace_back(std::make_shared<std::string>(str.substr(0, delimiter_find)));
                if (include_delimiter)
                {
                    arr->emplace_back(std::make_shared<std::string>(str.substr(delimiter_find)));
                }
                else
                {
                    arr->emplace_back(std::make_shared<std::string>(str.substr(delimiter_find + 1)));
                }

                return value(arr);
            }

            return value(std::make_shared<std::string>(str));
        }
        else if (j.type() == json::value_t::array)
        {
            if (j.size() == 1)
            {
                return parse_two_string_input(j.front(), delimiter, include_delimiter);
            }

            auto parsed = parse_json_value(j);
            if (parsed.has_value())
            {
                auto parsed_array = parsed->get_array();
                if (parsed_array->at(0).is_string() &&
                    parsed_array->at(1).is_string())
                {
                    return parsed_array;
                }
            }
        }

        return nullptr;
    }

    bool assembler::is_jump_call(vm_operator input)
    {
        return input == vm_operator::call || input == vm_operator::jump ||
            input == vm_operator::jump_true || input == vm_operator::jump_false;
    }

} // stack_vm