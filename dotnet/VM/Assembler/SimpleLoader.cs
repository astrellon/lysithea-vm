using System;
using System.IO;
using System.Collections.Generic;

namespace LysitheaVM
{
    public static class SimpleLoader
    {
        #region Methods
        public static IValue Parse(string sourceName, Stream input)
        {
            using var reader = new StreamReader(input);
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            return Parse(sourceName, lines);
        }

        public static IValue Parse(string sourceName, IReadOnlyList<string> lines)
        {
            var tokeniser = new Tokeniser(lines);
            if (!tokeniser.MoveNext())
            {
                return NullValue.Value;
            }

            var token = Lexer.ReadFromTokeniser(sourceName, tokeniser);

            return ToValue(token);
        }

        public static IValue ToValue(Token input)
        {
            if (input.Type == TokenType.Value)
            {
                return input.TokenValue;
            }
            else if (input.Type == TokenType.List)
            {
                var result = new List<IValue>();
                foreach (var item in input.TokenList)
                {
                    result.Add(ToValue(item));
                }
                return new ArrayValue(result);
            }
            else if (input.Type == TokenType.Map)
            {
                var result = new Dictionary<string, IValue>();
                foreach (var kvp in input.TokenMap)
                {
                    result[kvp.Key] = ToValue(kvp.Value);
                }
                return new ObjectValue(result);
            }

            throw new Exception($"Unable to turn token into value: {input.ToString()} {input.Location}");
        }
        #endregion
    }
}