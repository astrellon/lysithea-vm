using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SimpleStackVM
{
    public static class VirtualMachineParser
    {
        #region Methods
        public static IEnumerable<string> Tokenize(TextReader input)
        {
            var inQuote = '\0';
            var escaped = false;
            var inComment = false;
            var accumulator = new StringBuilder();
            while (input.Peek() >= 0)
            {
                var ch = (char)input.Read();
                if (inComment)
                {
                    if (ch == '\n' || ch == '\r')
                    {
                        inComment = false;
                    }
                    continue;
                }

                if (inQuote != '\0')
                {
                    if (escaped)
                    {
                        switch (ch)
                        {
                            case '"':
                            case '\'':
                            case '\\':
                            {
                                accumulator.Append(ch);
                                continue;
                            }
                            case 't':
                            {
                                accumulator.Append('\t');
                                continue;
                            }
                            case 'r':
                            {
                                accumulator.Append('\r');
                                continue;
                            }
                            case 'n':
                            {
                                accumulator.Append('\n');
                                continue;
                            }
                        }
                        escaped = false;
                    }
                    else if (ch == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    accumulator.Append(ch);
                    if (ch == inQuote)
                    {
                        yield return accumulator.ToString();
                        accumulator.Clear();
                        inQuote = '\0';
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case ';':
                        {
                            inComment = true;
                            break;
                        }

                        case '"':
                        case '\'':
                        {
                            inQuote = ch;
                            accumulator.Append(ch);
                            break;
                        }

                        case '(':
                        case ')':
                        case '{':
                        case '}':
                        {
                            if (accumulator.Length > 0)
                            {
                                yield return accumulator.ToString();
                                accumulator.Clear();
                            }
                            yield return ch.ToString();
                            break;
                        }

                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                        {
                            if (accumulator.Length > 0)
                            {
                                yield return accumulator.ToString();
                                accumulator.Clear();
                            }
                            break;
                        }
                        default:
                        {
                            accumulator.Append(ch);
                            break;
                        }
                    }
                }
            }
        }

        public static ArrayValue ReadAllTokens(TextReader input)
        {
            var tokens = Tokenize(input).GetEnumerator();
            var result = new List<IValue>();

            while (tokens.MoveNext())
            {
                result.Add(ReadFromTokens(tokens));
            }

            return new ArrayValue(result);
        }

        public static IValue ReadFromTokens(IEnumerator<string> tokens)
        {
            var token = tokens.Current;
            switch (token)
            {
                case null:
                {
                    throw new ArgumentException("Unexpected end of tokens");
                }
                case "(":
                {
                    var list = new List<IValue>();
                    while (tokens.MoveNext())
                    {
                        if (tokens.Current == ")")
                        {
                            break;
                        }
                        list.Add(ReadFromTokens(tokens));
                    }

                    return new ArrayValue(list);
                }
                case ")":
                {
                    throw new ArgumentException("Unexpected )");
                }
                case "{":
                {
                    var map = new Dictionary<string, IValue>();
                    while (tokens.MoveNext())
                    {
                        if (tokens.Current == "}")
                        {
                            break;
                        }
                        var key = ReadFromTokens(tokens).ToString();
                        tokens.MoveNext();
                        var value = ReadFromTokens(tokens);
                        map[key] = value;
                    }

                    return new ObjectValue(map);
                }
                case "}":
                {
                    throw new ArgumentException("Unexpected }");
                }
                default:
                {
                    return Atom(token);
                }
            }
        }

        public static IValue Atom(string input)
        {
            if (input.Length == 0)
            {
                return NullValue.Value;
            }

            if (double.TryParse(input, out var number))
            {
                return new NumberValue(number);
            }
            if (bool.TryParse(input, out var boolean))
            {
                return new BoolValue(boolean);
            }
            if (input == "null")
            {
                return NullValue.Value;
            }

            var first = input.First();
            var last = input.Last();
            if ((first == '"'  && last == '"') ||
                (first == '\'' && last == '\''))
            {
                return new StringValue(input.Substring(1, input.Length - 2));
            }

            return new VariableValue(input);
        }
        #endregion
    }
}