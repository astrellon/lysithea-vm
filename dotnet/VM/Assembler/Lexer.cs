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

            return Token.Expression(CodeLocation.Empty, result);
        }

        private static Token ParseList(Tokeniser tokeniser, string endToken)
        {
            var startLineNumber = tokeniser.LineNumber;
            var startColumnNumber = tokeniser.ColumnNumber;
            var list = new List<Token>();
            while (tokeniser.MoveNext())
            {
                if (tokeniser.Current == endToken)
                {
                    break;
                }
                list.Add(ReadFromTokeniser(tokeniser));
            }

            return Token.Expression(tokeniser.CreateLocation(startLineNumber, startColumnNumber), list);
        }

        private static Token ParseMap(Tokeniser tokeniser)
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
                if (value.Type == TokenType.Expression)
                {
                    throw new ParserException(value.Location, "", "Expression found in map literal");
                }

                map[key] = value;
            }

            return Token.Map(tokeniser.CreateLocation(startLineNumber, startColumnNumber), map);
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
                    return ParseList(tokeniser, ")");
                }
                case "[":
                {
                    return ParseList(tokeniser, "]");
                }
                case "{":
                {
                    return ParseMap(tokeniser);
                }

                case ")":
                case "]":
                case "}":
                {
                    throw new ParserException(tokeniser.CurrentLocation, token, $"Unexpected {token}");
                }
                default:
                {
                    return Token.Value(tokeniser.CurrentLocation, ParseConstant(token));
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