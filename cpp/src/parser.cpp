#include "parser.hpp"

#include "./values/array_value.hpp"
#include "./values/object_value.hpp"
#include "./values/string_value.hpp"
#include "./values/variable_value.hpp"

namespace lysithea_vm
{
    parser::parser(std::istream &input) : in_quote('\0'), return_symbol('\0'),
        escaped(false), in_comment(false), accumulator(), input(input)
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

        char ch;
        while (input.get(ch))
        {
            if (in_comment)
            {
                if (ch == '\n' || ch == '\r')
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
                            accumulator << ch;
                            break;
                        }
                        case 't':
                        {
                            accumulator << '\t';
                            break;
                        }
                        case 'r':
                        {
                            accumulator << '\r';
                            break;
                        }
                        case 'n':
                        {
                            accumulator << '\n';
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
                if (ch == in_quote)
                {
                    current = accumulator.str();
                    accumulator.str("");
                    accumulator.clear();
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
                        accumulator << ch;
                        break;
                    }

                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    {
                        current = accumulator.str();
                        if (current.size() > 0)
                        {
                            return_symbol = ch;
                            accumulator.str("");
                            accumulator.clear();
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
                        current = accumulator.str();
                        if (current.size() > 0)
                        {
                            accumulator.str("");
                            accumulator.clear();
                            return true;
                        }

                        break;
                    }

                    default:
                    {
                        accumulator << ch;
                        break;
                    }
                }
            }
        }

        return false;
    }

    array_value parser::read_from_stream(std::istream &input)
    {
        parser input_parser(input);

        array_vector result;
        while (input_parser.move_next())
        {
            result.emplace_back(read_from_parser(input_parser));
        }

        return array_value(result, false);
    }

    array_value parser::read_from_text(const std::string &input)
    {
        std::stringstream stream(input);
        return read_from_stream(stream);
    }

    value parser::read_from_parser(parser &input)
    {
        const auto &token = input.current;
        if (token.size() == 0)
        {
            throw std::runtime_error("Unexpected end of tokens");
        }
        if (token == "(")
        {
            array_vector list;
            while (input.move_next())
            {
                if (input.current == ")")
                {
                    break;
                }

                list.push_back(read_from_parser(input));
            }

            return value(std::make_shared<array_value>(list, false));
        }
        if (token == ")")
        {
            throw std::runtime_error("Unexpected )");
        }
        if (token == "{")
        {
            object_map map;
            while (input.move_next())
            {
                if (input.current == "}")
                {
                    break;
                }

                auto key = read_from_parser(input);
                input.move_next();

                map.emplace(key.to_string(), read_from_parser(input));
            }

            return value(std::make_shared<object_value>(map));
        }
        if (token == "}")
        {
            throw std::runtime_error("Unexpected }");
        }

        return atom(token);
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
} // lysithea_vm