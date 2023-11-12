using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LysitheaVM
{
    public static class ToStringFormatted
    {
        #region Methods
        public static string Indented(IValue value, int indentSize)
        {
            var rawStr = value.ToString();
            var split = rawStr.Split(Assembler.NewlineSeparators, StringSplitOptions.None);
            var token = Lexer.ReadFromLines("tostring", split);

            var builder = new StringBuilder();

            Indented(token.TokenList[0], builder, "", 0, indentSize);

            return builder.ToString();
        }

        private static void Indented(Token token, StringBuilder builder, string indent, int depth, int indentSize)
        {
            switch (token.Type)
            {
                case TokenType.Value:
                {
                    builder.Append(token.TokenValue.ToString());
                    break;
                }
                case TokenType.List:
                {
                    if (token.TokenList.Count < 32 && IsAllPrimitive(token.TokenList))
                    {
                        builder.Append(SingleLineArray(token.TokenList));
                    }
                    else
                    {
                        builder.Append("[\n");
                        depth++;
                        indent = " ".Repeat(depth * indentSize);

                        foreach (var item in token.TokenList)
                        {
                            builder.Append(indent);
                            Indented(item, builder, indent, depth, indentSize);
                            builder.Append('\n');
                        }

                        depth--;
                        indent = " ".Repeat(depth * indentSize);
                        builder.Append(indent);
                        builder.Append("]");
                    }
                    break;
                }
                case TokenType.Map:
                {
                    builder.Append("{\n");
                    depth++;
                    indent = " ".Repeat(depth * indentSize);

                    foreach (var kvp in token.TokenMap)
                    {
                        builder.Append(indent);

                        if (kvp.Key.HasWhiteSpace())
                        {
                            builder.Append('"');
                            builder.Append(StandardStringLibrary.EscapedString(kvp.Key));
                            builder.Append('"');
                        }
                        else
                        {
                            builder.Append(kvp.Key);
                        }

                        builder.Append(' ');

                        Indented(kvp.Value, builder, indent, depth, indentSize);

                        builder.Append('\n');
                    }

                    depth--;
                    indent = " ".Repeat(depth * indentSize);
                    builder.Append(indent);
                    builder.Append("}");
                    break;
                }
            }
        }

        private static bool IsAllPrimitive(IEnumerable<Token> input)
        {
            foreach (var item in input)
            {
                if (!(item.TokenValue is BoolValue ||
                    item.TokenValue is NumberValue ||
                    item.TokenValue is StringValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static string SingleLineArray(IEnumerable<Token> input)
        {
            var result = new StringBuilder();
            result.Append('[');
            var first = true;
            foreach (var value in input)
            {
                if (!first)
                {
                    result.Append(' ');
                }
                result.Append(value.ToString());
                first = false;
            }
            result.Append(']');
            return result.ToString();
        }
        #endregion
    }
}