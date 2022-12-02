using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class VirtualMachineParser
    {
        #region Fields
        public string Current { get; private set; } = "";
        public int LineNumber { get; private set; } = 0;
        public int ColumnNumber { get; private set; } = 0;

        private char inQuote = '\0';
        private char returnSymbol = '\0';
        private bool escaped = false;
        private bool inComment = false;
        private readonly StringBuilder accumulator = new StringBuilder();
        private readonly IReadOnlyList<string> input;
        #endregion

        #region Constructor
        public VirtualMachineParser(IReadOnlyList<string> input)
        {
            this.input = input;
        }
        #endregion

        #region Methods
        public bool MoveNext()
        {
            if (this.returnSymbol != '\0')
            {
                this.Current = this.returnSymbol.ToString();
                this.returnSymbol = '\0';
                return true;
            }

            while (this.LineNumber < this.input.Count && this.ColumnNumber < this.input[this.LineNumber].Length)
            {
                var ch = this.input[this.LineNumber][this.ColumnNumber++];
                if (ch == '\n' || ch == '\r')
                {
                    this.ColumnNumber = 0;
                    this.LineNumber++;
                }

                if (this.inComment)
                {
                    if (ch == '\n' || ch == '\r')
                    {
                        this.inComment = false;
                    }
                    continue;
                }

                if (this.inQuote != '\0')
                {
                    if (this.escaped)
                    {
                        switch (ch)
                        {
                            case '"':
                            case '\'':
                            case '\\':
                            {
                                this.accumulator.Append(ch);
                                break;
                            }
                            case 't':
                            {
                                this.accumulator.Append('\t');
                                break;
                            }
                            case 'r':
                            {
                                this.accumulator.Append('\r');
                                break;
                            }
                            case 'n':
                            {
                                this.accumulator.Append('\n');
                                break;
                            }
                        }
                        this.escaped = false;
                        continue;
                    }
                    else if (ch == '\\')
                    {
                        this.escaped = true;
                        continue;
                    }

                    this.accumulator.Append(ch);
                    if (ch == this.inQuote)
                    {
                        this.Current = this.accumulator.ToString();
                        this.accumulator.Clear();
                        this.inQuote = '\0';
                        return true;
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case ';':
                        {
                            this.inComment = true;
                            break;
                        }

                        case '"':
                        case '\'':
                        {
                            this.inQuote = ch;
                            this.accumulator.Append(ch);
                            break;
                        }

                        case '(':
                        case ')':
                        case '{':
                        case '}':
                        {
                            if (this.accumulator.Length > 0)
                            {
                                this.returnSymbol = ch;
                                this.Current = this.accumulator.ToString();
                                this.accumulator.Clear();
                            }
                            else
                            {
                                this.Current = ch.ToString();
                            }
                            return true;
                        }

                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                        {
                            if (this.accumulator.Length > 0)
                            {
                                this.Current = accumulator.ToString();
                                accumulator.Clear();
                                return true;
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

            return false;
        }

        public static TokenList ReadAllTokens(IReadOnlyList<string> input)
        {
            var parser = new VirtualMachineParser(input);
            var result = new List<IToken>();

            while (parser.MoveNext())
            {
                result.Add(ReadFromParser(parser));
            }

            return new TokenList(0, 0, result);
        }

        public static IToken ReadFromParser(VirtualMachineParser parser)
        {
            var token = parser.Current;
            switch (token)
            {
                case null:
                {
                    throw new ArgumentException("Unexpected end of tokens");
                }
                case "(":
                {
                    var lineNumber = parser.LineNumber;
                    var columnNumber = parser.ColumnNumber;
                    var list = new List<IToken>();
                    while (parser.MoveNext())
                    {
                        if (parser.Current == ")")
                        {
                            break;
                        }
                        list.Add(ReadFromParser(parser));
                    }

                    return new TokenList(lineNumber, columnNumber, list);
                }
                case ")":
                {
                    throw new ArgumentException("Unexpected )");
                }
                case "{":
                {
                    var lineNumber = parser.LineNumber;
                    var columnNumber = parser.ColumnNumber;
                    var map = new Dictionary<string, IToken>();
                    while (parser.MoveNext())
                    {
                        if (parser.Current == "}")
                        {
                            break;
                        }
                        var key = ReadFromParser(parser).ToString();
                        parser.MoveNext();
                        var value = ReadFromParser(parser);
                        map[key] = value;
                    }

                    return new TokenMap(lineNumber, columnNumber, map);
                }
                case "}":
                {
                    throw new ArgumentException("Unexpected }");
                }
                default:
                {
                    return new Token(parser.LineNumber, parser.ColumnNumber, Atom(token));
                }
            }
        }

        public static IValue Atom(string input)
        {
            if (input.Length == 0 || input == "null")
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