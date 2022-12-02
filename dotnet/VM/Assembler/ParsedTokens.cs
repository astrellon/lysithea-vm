using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace LysitheaVM
{
    public interface IToken
    {
        bool TryGetValue<T>([NotNullWhen(true)] out T? result);
        IValue GetValue();
    }

    public class Token : IToken
    {
        #region Fields
        public readonly int LineNumber;
        public readonly int ColumnNumber;
        public readonly IValue Value;
        #endregion

        #region Constructor
        public Token(int lineNumber, int columnNumber, IValue value)
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

        public IValue GetValue()
        {
            return this.Value;
        }
        #endregion
    }

    public class TokenList : IToken
    {
        #region Fields
        public readonly IReadOnlyList<IToken> Data;
        #endregion

        #region Constructor
        public TokenList(IReadOnlyList<IToken> data)
        {
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
        #endregion
    }

    public class TokenMap : IToken
    {
        #region Fields
        public readonly IReadOnlyDictionary<string, IToken> Data;
        #endregion

        #region Constructor
        public TokenMap(IReadOnlyDictionary<string, IToken> data)
        {
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
        #endregion
    }
}