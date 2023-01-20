using System;
using System.Linq;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class Lexer
    {
        #region Methods
        public static Token ReadFromLines(string sourceName, IReadOnlyList<string> input)
        {
            var parser = new Tokeniser(input);
            var result = new List<Token>();

            while (parser.MoveNext())
            {
                result.Add(ReadFromTokeniser(sourceName, parser));
            }

            return Token.Expression(CodeLocation.Empty, result);
        }

        private static ParserException MakeError(string sourceName, CodeLocation location, Tokeniser tokeniser, string atToken, string message)
        {
            var trace = ExceptionsCommon.CreateErrorLogAt(sourceName, location, tokeniser.Input);
            return new ParserException(location, atToken, trace, message);
        }

        private static ParserException MakeError(string sourceName, Tokeniser tokeniser, string atToken, string message)
        {
            var location = tokeniser.CurrentLocation;
            return MakeError(sourceName, location, tokeniser, atToken, message);
        }

        public static Token ReadFromTokeniser(string sourceName, Tokeniser tokeniser)
        {
            var token = tokeniser.Current;
            switch (token)
            {
                case null:
                {
                    throw MakeError(sourceName, tokeniser, token, "Unexpected end of tokens");
                }

                case "(":
                {
                    return ParseList(sourceName, tokeniser, true, ")");
                }
                case "[":
                {
                    return ParseList(sourceName, tokeniser, false, "]");
                }
                case "{":
                {
                    return ParseMap(sourceName, tokeniser);
                }

                case ")":
                case "]":
                case "}":
                {
                    throw MakeError(sourceName, tokeniser, token, $"Unexpected {token}");
                }
                default:
                {
                    return Token.Value(tokeniser.CurrentLocation, ParseConstant(token));
                }
            }
        }

        public static Token ParseList(string sourceName, Tokeniser tokeniser, bool isExpression, string endToken)
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
                list.Add(ReadFromTokeniser(sourceName, tokeniser));
            }

            var tokenType = isExpression ? TokenType.Expression : TokenType.List;
            return new Token(tokeniser.CreateLocation(startLineNumber, startColumnNumber), tokenType, list: list);
        }

        public static Token ParseMap(string sourceName, Tokeniser tokeniser)
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

                var key = ReadFromTokeniser(sourceName, tokeniser).ToString();
                tokeniser.MoveNext();

                var value = ReadFromTokeniser(sourceName, tokeniser);
                if (value.Type == TokenType.Expression)
                {
                    throw MakeError(sourceName, value.Location, tokeniser, value.ToString(), "Expression found in map literal");
                }

                map[key] = value;
            }

            return Token.Map(tokeniser.CreateLocation(startLineNumber, startColumnNumber), map);
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