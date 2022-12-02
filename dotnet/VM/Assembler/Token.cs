using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IToken
    {
        int LineNumber { get; }
        int ColumnNumber { get; }

        bool TryGetValue<T>([NotNullWhen(true)] out T? result);
        IValue? GetValue();
        Token Copy(IValue? input);
        Token ToEmpty();
        string ToString();
    }

    public class Token : IToken
    {
        #region Fields
        public int LineNumber { get; init; }
        public int ColumnNumber { get; init; }
        public readonly IValue? Value;
        #endregion

        #region Constructor
        public Token(int lineNumber, int columnNumber, IValue? value)
        {
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
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

        public IValue? GetValue()
        {
            return this.Value;
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.LineNumber, this.ColumnNumber, input);
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
            return new Token(this.LineNumber, this.ColumnNumber, null);
        }
        #endregion
    }

    public class TokenList : IToken
    {
        #region Fields
        public int LineNumber { get; init; }
        public int ColumnNumber { get; init; }
        public readonly IReadOnlyList<IToken> Data;
        #endregion

        #region Constructor
        public TokenList(int lineNumber, int columnNumber, IReadOnlyList<IToken> data)
        {
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
            this.Data = data;
        }
        #endregion

        #region Methods
        public bool TryGetValue<T>([NotNullWhen(true)] out T? result)
        {
            result = default(T);
            return false;
        }

        public IValue? GetValue()
        {
            return new ArrayValue(this.Data.Select(t => t.GetValue()).ToList());
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.LineNumber, this.ColumnNumber, input);
        }

        public override string ToString()
        {
            return "TokenList";
        }

        public Token ToEmpty()
        {
            return new Token(this.LineNumber, this.ColumnNumber, null);
        }
        #endregion
    }

    public class TokenMap : IToken
    {
        #region Fields
        public int LineNumber { get; init; }
        public int ColumnNumber { get; init; }
        public readonly IReadOnlyDictionary<string, IToken> Data;
        #endregion

        #region Constructor
        public TokenMap(int lineNumber, int columnNumber, IReadOnlyDictionary<string, IToken> data)
        {
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
            this.Data = data;
        }
        #endregion

        #region Methods
        public bool TryGetValue<T>([NotNullWhen(true)] out T? result)
        {
            result = default(T);
            return false;
        }

        public IValue? GetValue()
        {
            return new ObjectValue(this.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetValue()));
        }

        public Token Copy(IValue? input)
        {
            return new Token(this.LineNumber, this.ColumnNumber, input);
        }

        public override string ToString()
        {
            return "TokenMap";
        }

        public Token ToEmpty()
        {
            return new Token(this.LineNumber, this.ColumnNumber, null);
        }
        #endregion
    }
}