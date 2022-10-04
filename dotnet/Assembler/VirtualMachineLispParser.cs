using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleStackVM
{
    public static class VirtualMachineLispParser
    {
        #region Fields
        private static Regex TokenRegex = new Regex("[^\\s\"']+|\"([^\"]*)\"|'([^']*)'");
        #endregion

        #region Methods
        public static List<string> Tokenize(string input)
        {
            var cleaned = input.Replace("(", " ( ")
                .Replace(")", " ) ")
                .Trim();

            return TokenRegex.Matches(cleaned).Select(m => m.Value).ToList();
        }

        public static ArrayValue ReadAllTokens(List<string> tokens)
        {
            var result = new List<IValue>();

            while (tokens.Any())
            {
                result.Add(ReadFromTokens(tokens));
            }

            return new ArrayValue(result);
        }

        public static IValue ReadFromTokens(List<string> tokens)
        {
            if (tokens.Count == 0)
            {
                throw new ArgumentException("Unexpected end of tokens");
            }

            var token = tokens.PopFront();
            if (token == "(")
            {
                var list = new List<IValue>();
                while (tokens.First() != ")")
                {
                    list.Add(ReadFromTokens(tokens));
                }
                tokens.PopFront();
                return new ArrayValue(list);
            }
            else if (token == ")")
            {
                throw new ArgumentException("Unexpected )");
            }
            else
            {
                return Atom(token);
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
            if (input.First() == '"' && input.Last() == '"')
            {
                return new StringValue(input.Substring(1, input.Length - 2));
            }

            return new SymbolValue(input);
        }

        private static bool TryParseOperator(string input, out Operator result)
        {
            if (!Enum.TryParse<Operator>(input, true, out result))
            {
                result = Operator.Unknown;
                return false;
            }

            return true;
        }
        #endregion
    }

    public static class ListExtensions
    {
        #region Methods
        public static T PopBack<T>(this List<T> input)
        {
            if (input.Any())
            {
                var result = input.Last();
                input.RemoveAt(input.Count - 1);
                return result;
            }

            throw new ArgumentException("Unable to pop empty list");
        }

        public static T PopFront<T>(this List<T> input)
        {
            if (input.Any())
            {
                var result = input.First();
                input.RemoveAt(0);
                return result;
            }

            throw new ArgumentException("Unable to pop empty list");
        }
        #endregion
    }
}