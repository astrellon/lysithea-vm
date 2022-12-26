using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace LysitheaVM
{
    public enum TokenType
    {
        Empty, Value, Expression, List, Map
    }

    public class Token
    {
        #region Fields
        private static readonly IReadOnlyList<Token> EmptyList = new Token[0];
        private static readonly IReadOnlyDictionary<string, Token> EmptyMap = new Dictionary<string, Token>();

        public readonly CodeLocation Location;
        public readonly TokenType Type;

        public readonly IValue TokenValue;
        public readonly IReadOnlyList<Token> TokenList;
        public readonly IReadOnlyDictionary<string, Token> TokenMap;
        #endregion

        #region Constructor
        public Token(CodeLocation location, TokenType type, IValue? value = null, IReadOnlyList<Token>? list = null, IReadOnlyDictionary<string, Token>? map = null)
        {
            this.Location = location;
            this.Type = type;
            this.TokenValue = value ?? NullValue.Value;
            this.TokenList = list ?? EmptyList;
            this.TokenMap = map ?? EmptyMap;
        }
        #endregion

        #region Methods
        public Token KeepLocation(string input)
        {
            return this.KeepLocation(new StringValue(input));
        }

        public Token KeepLocation(IValue? input)
        {
            if (input == null)
            {
                return this.ToEmpty();
            }
            return Token.Value(this.Location, input);
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                default:
                case TokenType.Empty: return "<empty>";
                case TokenType.Value: return this.TokenValue.ToString();
                case TokenType.List: return "<TokenList>";
                case TokenType.Map: return "<TokenMap>";
                case TokenType.Expression: return "<TokenExpression>";
            }
        }

        public bool IsNestedExpression()
        {
            return this.TokenList.Any() && this.TokenList.All(i => i.Type == TokenType.Expression);
        }

        public Token ToEmpty()
        {
            return new Token(this.Location, TokenType.Empty);
        }

        public static Token Empty(CodeLocation location)
        {
            return new Token(location, TokenType.Empty);
        }

        public static Token Value(CodeLocation location, IValue value)
        {
            return new Token(location, TokenType.Value, value: value);
        }

        public static Token List(CodeLocation location, IReadOnlyList<Token> list)
        {
            return new Token(location, TokenType.List, list: list);
        }

        public static Token Map(CodeLocation location, IReadOnlyDictionary<string, Token> map)
        {
            return new Token(location, TokenType.Map, map: map);
        }

        public static Token Expression(CodeLocation location, IReadOnlyList<Token> expression)
        {
            return new Token(location, TokenType.Expression, list: expression);
        }
        #endregion
    }
}