#include "lexer.hpp"

#include <iostream>

#include "../values/array_value.hpp"
#include "../values/object_value.hpp"
#include "../values/string_value.hpp"
#include "../values/variable_value.hpp"
#include "../errors/parser_error.hpp"
#include "./tokeniser.hpp"

namespace lysithea_vm
{
    token lexer::read_from_text(const std::vector<std::string> &input_lines)
    {
        tokeniser input_parser(input_lines);

        std::vector<token_ptr> result;
        while (input_parser.move_next())
        {
            result.emplace_back(std::make_shared<token>(read_from_parser(input_parser)));
        }

        return token(code_location(), token_type::expression, result);
    }

    token lexer::read_from_parser(tokeniser &input)
    {
        const auto &input_token = input.current;
        if (input_token.size() == 0)
        {
            throw std::runtime_error("Unexpected end of tokens");
        }
        if (input_token == "(")
        {
            return parse_list(input, true, ")");
        }
        if (input_token == "[")
        {
            return parse_list(input, false, "]");
        }
        if (input_token == "{")
        {
            return parse_map(input);
        }

        if (input_token == ")" || input_token == "}" || input_token == "]")
        {
            throw parser_error(input.current_location(), input_token, "Unexpected " + input_token);
        }

        return token(input.current_location(), parse_constant(input_token));
    }

    token lexer::parse_list(tokeniser &input, bool is_expression, const std::string &end_token)
    {
        auto line_number = input.end_line_number();
        auto column_number = input.end_column_number();

        std::vector<token_ptr> list;
        while (input.move_next())
        {
            if (input.current == end_token)
            {
                break;
            }

            list.emplace_back(std::make_shared<token>(read_from_parser(input)));
        }

        auto type = is_expression ? token_type::expression : token_type::list;
        code_location location(line_number, column_number, input.end_line_number(), input.end_column_number());
        return token(location, type, list);
    }

    token lexer::parse_map(tokeniser &input)
    {
        auto line_number = input.end_line_number();
        auto column_number = input.end_column_number();

        std::unordered_map<std::string, token_ptr> map;
        while (input.move_next())
        {
            if (input.current == "}")
            {
                break;
            }

            auto key = read_from_parser(input);
            input.move_next();

            map.emplace(key.get_value().to_string(), std::make_shared<token>(read_from_parser(input)));
        }

        code_location location(line_number, column_number, input.end_line_number(), input.end_column_number());
        return token(location, map);
    }

    value lexer::parse_constant(const std::string &input)
    {
        if (input.size() == 0 || input == "null")
        {
            return value::make_null();
        }

        double num;
        if (std::sscanf(input.c_str(), "%lf", &num) == 1)
        {
            return value(num);
        }
        if (input == "true")
        {
            return value(true);
        }
        if (input == "false")
        {
            return value(false);
        }

        auto first = input.front();
        auto last = input.back();
        if ((first == '"' && last == '"') ||
            (first == '\'' && last == '\''))
        {
            return value(std::make_shared<string_value>(input.substr(1, input.size() - 2)));
        }

        return value(std::make_shared<variable_value>(input));
    }

} // lysithea_vm