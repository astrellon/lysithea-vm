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
        public int StartLineNumber { get; private set; } = 0;
        public int StartColumnNumber { get; private set; } = 0;

        private char inQuote = '\0';
        private char returnSymbol = '\0';
        private bool escaped = false;
        private bool inComment = false;
        private readonly StringBuilder accumulator = new StringBuilder();
        private readonly IReadOnlyList<string> input;

        public CodeLocation CurrentLocation => new CodeLocation(this.StartLineNumber, this.StartColumnNumber, this.LineNumber, this.ColumnNumber);
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

            while (this.LineNumber < this.input.Count)
            {
                var line = this.input[this.LineNumber];
                if (line.Length == 0)
                {
                    this.LineNumber++;
                    continue;
                }

                var ch = line[this.ColumnNumber++];
                var atEndOfLine = this.ColumnNumber >= line.Length;
                if (atEndOfLine)
                {
                    this.ColumnNumber = 0;
                    this.LineNumber++;
                }

                if (this.inComment)
                {
                    if (atEndOfLine)
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
                                this.AppendChar(ch);
                                break;
                            }
                            case 't':
                            {
                                this.AppendChar('\t');
                                break;
                            }
                            case 'r':
                            {
                                this.AppendChar('\r');
                                break;
                            }
                            case 'n':
                            {
                                this.AppendChar('\n');
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

                    this.AppendChar(ch);
                    if (atEndOfLine)
                    {
                        this.AppendChar('\n');
                    }

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
                            this.AppendChar(ch);
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
                            this.AppendChar(ch);
                            break;
                        }
                    }
                }
            }

            return false;
        }

        private void AppendChar(char ch)
        {
            if (this.accumulator.Length == 0)
            {
                this.StartLineNumber = this.LineNumber;
                this.StartColumnNumber = this.ColumnNumber - 1;
            }
            this.accumulator.Append(ch);
        }

        public static Token ReadAllTokens(IReadOnlyList<string> input)
        {
            var parser = new VirtualMachineParser(input);
            var result = new List<Token>();

            while (parser.MoveNext())
            {
                result.Add(ReadFromParser(parser));
            }

            return new Token(CodeLocation.Empty, result);
        }

        public static Token ReadFromParser(VirtualMachineParser parser)
        {
            var token = parser.Current;
            switch (token)
            {
                case null:
                {
                    throw new ParserException(parser.CurrentLocation, token, "Unexpected end of tokens");
                }
                case "(":
                {
                    var lineNumber = parser.LineNumber;
                    var columnNumber = parser.ColumnNumber;
                    var list = new List<Token>();
                    while (parser.MoveNext())
                    {
                        if (parser.Current == ")")
                        {
                            break;
                        }
                        list.Add(ReadFromParser(parser));
                    }

                    return new Token(new CodeLocation(lineNumber, columnNumber, parser.LineNumber, parser.ColumnNumber), list);
                }
                case ")":
                {
                    throw new ParserException(parser.CurrentLocation, token, "Unexpected )");
                }
                case "{":
                {
                    var lineNumber = parser.LineNumber;
                    var columnNumber = parser.ColumnNumber;
                    var map = new Dictionary<string, Token>();
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

                    return new Token(new CodeLocation(lineNumber, columnNumber, parser.LineNumber, parser.ColumnNumber), map);
                }
                case "}":
                {
                    throw new ParserException(parser.CurrentLocation, token, "Unexpected }");
                }
                default:
                {
                    return new Token(parser.CurrentLocation, Atom(token));
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