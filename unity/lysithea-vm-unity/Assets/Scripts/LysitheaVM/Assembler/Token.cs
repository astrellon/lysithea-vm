using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IToken
    {
        CodeLocation Location { get; }

        bool TryGetValue<T>([NotNullWhen(true)] out T? result);
        IValue GetValue();
        IValue? GetValueCanBeEmpty();
        Token Copy(IValue? input);
        Token ToEmpty();
        string ToString();
    }

    public class Token : IToken
    {
        #region Fields
        public CodeLocation Location { get; private set; }
        public readonly IValue? Value;
        #endregion

        #region Constructor
        public Token(CodeLocation location, IValue? value)
        {
            this.Location = location;
            this.Value = value;
        }
        #endregion

        #region Methods
        public bool TryGetValue<T>([NotNullWhen(true)] out T? result)
        {
            if (this.Value is T temp)
            {
                result = temp;
                return true;
            }

            result = default(T);
            return false;
        }

        public IValue GetValue()
        {
            if (this.Value == null)
            {
                throw new System.Exception("Cannot get value of empty token");
            }
            return this.Value;
        }

        public IValue? GetValueCanBeEmpty()
        {
            return this.Value;
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.Location, input);
        }

        public override string ToString()
        {
            if (this.Value != null)
            {
                return this.Value.ToString();
            }
            return "null";
        }

        public Token ToEmpty()
        {
            return new Token(this.Location, null);
        }
        #endregion
    }

    public class TokenList : IToken
    {
        #region Fields
        public CodeLocation Location { get; private set; }
        public readonly IReadOnlyList<IToken> Data;
        #endregion

        #region Constructor
        public TokenList(CodeLocation location, IReadOnlyList<IToken> data)
        {
            this.Location = location;
            this.Data = data;
        }
        #endregion

        #region Methods
        public bool TryGetValue<T>([NotNullWhen(true)] out T? result)
        {
            result = default(T);
            return false;
        }

        public IValue GetValue()
        {
            return new ArrayValue(this.Data.Select(t => t.GetValue()).ToList());
        }

        public IValue? GetValueCanBeEmpty()
        {
            return this.GetValue();
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.Location, input);
        }

        public override string ToString()
        {
            return "TokenList";
        }

        public Token ToEmpty()
        {
            return new Token(this.Location, null);
        }
        #endregion
    }

    public class TokenMap : IToken
    {
        #region Fields
        public CodeLocation Location { get; private set; }
        public readonly IReadOnlyDictionary<string, IToken> Data;
        #endregion

        #region Constructor
        public TokenMap(CodeLocation location, IReadOnlyDictionary<string, IToken> data)
        {
            this.Location = location;
            this.Data = data;
        }
        #endregion

        #region Methods
        public bool TryGetValue<T>([NotNullWhen(true)] out T? result)
        {
            result = default(T);
            return false;
        }

        public IValue GetValue()
        {
            return new ObjectValue(this.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue()));
        }

        public IValue? GetValueCanBeEmpty()
        {
            return this.GetValue();
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.Location, input);
        }

        public override string ToString()
        {
            return "TokenMap";
        }

        public Token ToEmpty()
        {
            return new Token(this.Location, null);
        }
        #endregion
    }
}