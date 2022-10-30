#include "parser.hpp"

namespace stack_vm
{
    parser::parser(std::istream &input) : in_quote('\0'), return_symbol('\0'),
        escaped(false), in_comment(false), accumulator(), input(input)
    {

    }

    bool parser::move_next(std::string &output)
    {
        if (return_symbol != '\0')
        {
            output = std::string(1, return_symbol);
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
                    output = accumulator.str();
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
                        output = accumulator.str();
                        if (output.size() > 0)
                        {
                            return_symbol = ch;
                            accumulator.str("");
                            accumulator.clear();
                        }
                        else
                        {
                            output = std::string(1, ch);
                        }
                        return true;
                    }

                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                    {
                        output = accumulator.str();
                        if (output.size() > 0)
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
} // stack_vm