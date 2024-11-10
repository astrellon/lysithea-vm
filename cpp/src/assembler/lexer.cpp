#include "lexer.hpp"

#include <iostream>

#include "../values/array_value.hpp"
#include "../values/object_value.hpp"
#include "../values/string_value.hpp"
#include "../values/variable_value.hpp"
#include "../errors/parser_error.hpp"
#include "../errors/error_common.hpp"
#include "./tokeniser.hpp"

namespace lysithea_vm
{
    token lexer::read_from_text(const std::string &source_name, const std::vector<std::string> &input_lines)
    {
        tokeniser input_parser(input_lines);

        std::vector<token_ptr> result;
        while (input_parser.move_next())
        {
            result.emplace_back(std::make_shared<token>(read_from_parser(source_name, input_parser)));
        }

        return token(code_location(), token_type::expression, result);
    }

    token lexer::read_from_parser(const std::string &source_name, tokeniser &input)
    {
        const auto &input_token = input.current;
        if (input_token.size() == 0)
        {
            throw make_error(source_name, input, "", "Unexpected end of tokens");
        }
        if (input_token == "(")
        {
            return parse_list(source_name, input, true, ")");
        }
        if (input_token == "[")
        {
            return parse_list(source_name, input, false, "]");
        }
        if (input_token == "{")
        {
            return parse_map(source_name, input);
        }

        if (input_token == ")" || input_token == "}" || input_token == "]")
        {
            throw make_error(source_name, input, input_token, "Unexpected " + input_token);
        }

        return token(input.current_location(), parse_constant(input_token));
    }

    parser_error lexer::make_error(const std::string &source_name, const tokeniser &tokeniser, const std::string &at_token, const std::string &message)
    {
        auto location = tokeniser.current_location();
        auto trace = create_error_log_at(source_name, location, tokeniser.input_data());
        return parser_error(location, at_token, trace, "Unexpected " + at_token);
    }

    token lexer::parse_list(const std::string &source_name, tokeniser &input, bool is_expression, const std::string &end_token)
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

            list.emplace_back(std::make_shared<token>(read_from_parser(source_name, input)));
        }

        auto type = is_expression ? token_type::expression : token_type::list;
        code_location location(line_number, column_number, input.end_line_number(), input.end_column_number());
        return token(location, type, list);
    }

    token lexer::parse_map(const std::string &source_name, tokeniser &input)
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

            auto key = read_from_parser(source_name, input);
            input.move_next();

            auto value = std::make_shared<token>(read_from_parser(source_name, input));
            if (value->type == token_type::expression)
            {
                throw make_error(source_name, input, value->to_string(0), "Expression found in map literal");
            }

            map.emplace(key.token_value.to_string(), value);
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