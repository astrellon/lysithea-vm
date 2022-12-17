using System;
using System.Linq;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class Lexer
    {
        #region Methods
        public static Token ReadFromLines(IReadOnlyList<string> input)
        {
            var parser = new Tokeniser(input);
            var result = new List<Token>();

            while (parser.MoveNext())
            {
                result.Add(ReadFromTokeniser(parser));
            }

            return new Token(CodeLocation.Empty, result);
        }

        public static Token ReadFromTokeniser(Tokeniser tokeniser)
        {
            var token = tokeniser.Current;
            switch (token)
            {
                case null:
                {
                    throw new ParserException(tokeniser.CurrentLocation, token, "Unexpected end of tokens");
                }
                case "(":
                {
                    var startLineNumber = tokeniser.LineNumber;
                    var startColumnNumber = tokeniser.ColumnNumber;
                    var list = new List<Token>();
                    while (tokeniser.MoveNext())
                    {
                        if (tokeniser.Current == ")")
                        {
                            break;
                        }
                        list.Add(ReadFromTokeniser(tokeniser));
                    }

                    return new Token(tokeniser.CreateLocation(startLineNumber, startColumnNumber), list);
                }
                case ")":
                {
                    throw new ParserException(tokeniser.CurrentLocation, token, "Unexpected )");
                }
                case "{":
                {
                    var startLineNumber = tokeniser.LineNumber;
                    var startColumnNumber = tokeniser.ColumnNumber;
                    var map = new Dictionary<string, Token>();
                    while (tokeniser.MoveNext())
                    {
                        if (tokeniser.Current == "}")
                        {
                            break;
                        }
                        var key = ReadFromTokeniser(tokeniser).ToString();
                        tokeniser.MoveNext();
                        var value = ReadFromTokeniser(tokeniser);
                        map[key] = value;
                    }

                    return new Token(tokeniser.CreateLocation(startLineNumber, startColumnNumber), map);
                }
                case "}":
                {
                    throw new ParserException(tokeniser.CurrentLocation, token, "Unexpected }");
                }
                default:
                {
                    return new Token(tokeniser.CurrentLocation, ParseConstant(token));
                }
            }
        }

        public static IValue ParseConstant(string input)
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