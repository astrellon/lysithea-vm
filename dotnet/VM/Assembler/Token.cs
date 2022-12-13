using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace LysitheaVM
{
    public enum TokenType
    {
        Empty, Value, List, Map
    }

    public class Token
    {
        #region Fields
        private static readonly IReadOnlyList<Token> EmptyList = new Token[0];
        private static readonly IReadOnlyDictionary<string, Token> EmptyMap = new Dictionary<string, Token>();

        public readonly CodeLocation Location;
        public readonly TokenType Type;

        public readonly IValue Value = NullValue.Value;
        public readonly IReadOnlyList<Token> TokenList = EmptyList;
        public readonly IReadOnlyDictionary<string, Token> TokenMap = EmptyMap;
        #endregion

        #region Constructor
        public Token(CodeLocation location)
        {
            this.Location = location;
            this.Type = TokenType.Empty;
        }
        public Token(CodeLocation location, IValue value)
        {
            this.Location = location;
            this.Type = TokenType.Value;
            this.Value = value;
        }
        public Token(CodeLocation location, IReadOnlyList<Token> data)
        {
            this.Location = location;
            this.Type = TokenType.List;
            this.TokenList = data;
        }
        public Token(CodeLocation location, IReadOnlyDictionary<string, Token> data)
        {
            this.Location = location;
            this.Type = TokenType.Map;
            this.TokenMap = data;
        }
        #endregion

        #region Methods
        public IValue? GetValueCanBeEmpty()
        {
            if (this.Type == TokenType.Empty)
            {
                return null;
            }
            return this.GetValue();
        }

        public IValue GetValue()
        {
            switch (this.Type)
            {
                case TokenType.Empty:
                case TokenType.Value:
                {
                    return this.Value;
                }
                case TokenType.List:
                {
                    return new ArrayValue(this.TokenList.Select(t => t.GetValue()).ToList());
                }
                case TokenType.Map:
                {
                    return new ObjectValue(this.TokenMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue()));
                }
            }
            return this.Value;
        }

        public Token KeepLocation(IValue? input)
        {
            if (input == null)
            {
                return this.ToEmpty();
            }
            return new Token(this.Location, input);
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                default:
                case TokenType.Empty: return "<empty>";
                case TokenType.Value: return this.Value.ToString();
                case TokenType.List: return "<TokenList>";
                case TokenType.Map: return "<TokenMap>";
            }
        }

        public Token ToEmpty()
        {
            return new Token(this.Location);
        }
        #endregion
    }
}