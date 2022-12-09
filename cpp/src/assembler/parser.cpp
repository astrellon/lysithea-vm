#include "parser.hpp"

#include "../values/array_value.hpp"
#include "../values/object_value.hpp"
#include "../values/string_value.hpp"
#include "../values/variable_value.hpp"

namespace lysithea_vm
{
    parser::parser(std::shared_ptr<std::vector<std::string>> input) : in_quote('\0'), return_symbol('\0'),
        escaped(false), in_comment(false), line_number(0), column_number(0),
        start_line_number(0), start_column_number(0),
        accumulator(), input(input)
    {

    }

    bool parser::move_next()
    {
        if (return_symbol != '\0')
        {
            current = std::string(1, return_symbol);
            return_symbol = '\0';
            return true;
        }

        while (line_number < input->size())
        {
            const auto &line = input->at(line_number);
            if (line.size() == 0)
            {
                line_number++;
                continue;
            }

            auto ch = line[column_number++];
            auto at_end_of_line = column_number >= line.size();

            if (at_end_of_line)
            {
                column_number = 0;
                line_number++;
            }

            if (in_comment)
            {
                if (at_end_of_line)
                {
                    in_comment = false;
                }
                continue;
            }

            if (in_quote != '\0')
            {
                if (escaped)
                {
                    switch (ch)
                    {
                        case '"':
                        case '\'':
                        case '\\':
                        {
                            append_char(ch);
                            break;
                        }
                        case 't':
                        {
                            append_char('\t');
                            break;
                        }
                        case 'r':
                        {
                            append_char('\r');
                            break;
                        }
                        case 'n':
                        {
                            append_char('\n');
                            break;
                        }
                    }
                    escaped = false;
                    continue;
                }
                else if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                accumulator << ch;
                append_char(ch);
                if (ch == in_quote)
                {
                    current = accumulator.str();
                    reset_accumulator();
                    in_quote = '\0';
                    return true;
                }
            }
            else
            {
                switch (ch)
                {
                    case ';':
                    {
                        in_comment = true;
                        break;
                    }

                    case '"':
                    case '\'':
                    {
                        in_quote = ch;
                        append_char(ch);
                        break;
                    }

                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    {
                        if (accumulator_size > 0)
                        {
                            current = accumulator.str();
                            return_symbol = ch;
                            reset_accumulator();
                        }
                        else
                        {
                            current = std::string(1, ch);
                        }
                        return true;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    {
                        if (accumulator_size > 0)
                        {
                            current = accumulator.str();
                            reset_accumulator();
                            return true;
                        }

                        break;
                    }

                    default:
                    {
                        append_char(ch);
                        break;
                    }
                }
            }
        }

        return false;
    }

    code_location parser::current_location() const
    {
        return code_location(start_line_number, start_column_number, line_number, column_number);
    }

    std::shared_ptr<token_list> parser::read_from_stream(std::istream &input)
    {
        auto input_lines = std::make_shared<std::vector<std::string>>();

        char ch;
        std::stringstream accumulator;
        while (input.get(ch))
        {
            if (ch == '\r')
            {
                auto next = input.peek();
                if (next == '\n')
                {
                    input.get(ch);
                }

                input_lines->emplace_back(accumulator.str());
                accumulator.str(std::string());
            }
            else if (ch == '\n')
            {
                input_lines->emplace_back(accumulator.str());
                accumulator.str(std::string());
            }
            else
            {
                accumulator << ch;
            }
        }
        input_lines->emplace_back(accumulator.str());

        parser input_parser(input_lines);

        std::vector<std::shared_ptr<itoken>> result;
        while (input_parser.move_next())
        {
            result.emplace_back(read_from_parser(input_parser));
        }

        return std::make_shared<token_list>(code_location(), result);
    }

    std::shared_ptr<token_list> parser::read_from_text(const std::string &input)
    {
        std::stringstream stream(input);
        return read_from_stream(stream);
    }

    std::shared_ptr<itoken> parser::read_from_parser(parser &input)
    {
        const auto &token = input.current;
        if (token.size() == 0)
        {
            throw std::runtime_error("Unexpected end of tokens");
        }
        if (token == "(")
        {
            auto line_number = input.line_number;
            auto column_number = input.column_number;

            std::vector<std::shared_ptr<itoken>> list;
            while (input.move_next())
            {
                if (input.current == ")")
                {
                    break;
                }

                list.emplace_back(read_from_parser(input));
            }

            code_location location(line_number, column_number, input.line_number, input.column_number);
            return std::make_shared<token_list>(location, list);
        }
        if (token == ")")
        {
            throw std::runtime_error("Unexpected )");
        }
        if (token == "{")
        {
            auto line_number = input.line_number;
            auto column_number = input.column_number;

            std::unordered_map<std::string, std::shared_ptr<itoken>> map;
            while (input.move_next())
            {
                if (input.current == "}")
                {
                    break;
                }

                auto key = read_from_parser(input);
                input.move_next();

                map.emplace(key->get_value().to_string(), read_from_parser(input));
            }

            code_location location(line_number, column_number, input.line_number, input.column_number);
            return std::make_shared<token_map>(location, map);
        }
        if (token == "}")
        {
            throw std::runtime_error("Unexpected }");
        }

        return std::make_shared<lysithea_vm::token>(input.current_location(), atom(token));
    }

    value parser::atom(const std::string &input)
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

    void parser::append_char(char ch)
    {
        if (accumulator_size == 0)
        {
            start_line_number = line_number;
            start_column_number = column_number - 1;
        }

        accumulator << ch;
        accumulator_size++;
    }

    void parser::reset_accumulator()
    {
        accumulator_size = 0;
        accumulator.str("");
        accumulator.clear();
    }
} // lysithea_vm