#include "tokeniser.hpp"

namespace lysithea_vm
{
    tokeniser::tokeniser(const std::vector<std::string> &input) : in_quote('\0'), return_symbol('\0'),
        escaped(false), in_comment(false), line_number(0), column_number(0),
        start_line_number(0), start_column_number(0),
        accumulator(), accumulator_size(0), input(input)
    {

    }

    bool tokeniser::move_next()
    {
        if (return_symbol != '\0')
        {
            current = std::string(1, return_symbol);
            return_symbol = '\0';
            return true;
        }

        while (line_number < input.size())
        {
            const auto &line = input[line_number];
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

                    case '(': case ')':
                    case '[': case ']':
                    case '{': case '}':
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

    code_location tokeniser::current_location() const
    {
        return code_location(start_line_number, start_column_number, line_number, column_number);
    }

    void tokeniser::append_char(char ch)
    {
        if (accumulator_size == 0)
        {
            start_line_number = line_number;
            start_column_number = column_number - 1;
        }

        accumulator << ch;
        accumulator_size++;
    }

    void tokeniser::reset_accumulator()
    {
        accumulator_size = 0;
        accumulator.str("");
        accumulator.clear();
    }

    std::shared_ptr<std::vector<std::string>> tokeniser::split_stream(std::istream &input)
    {
        auto result = std::make_shared<std::vector<std::string>>();

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

                result->emplace_back(accumulator.str());
                accumulator.str(std::string());
            }
            else if (ch == '\n')
            {
                result->emplace_back(accumulator.str());
                accumulator.str(std::string());
            }
            else
            {
                accumulator << ch;
            }
        }
        result->emplace_back(accumulator.str());

        return result;
    }

    std::shared_ptr<std::vector<std::string>> tokeniser::split_text(const std::string &input)
    {
        std::stringstream stream(input);
        return split_stream(stream);
    }

} // lysithea_vm