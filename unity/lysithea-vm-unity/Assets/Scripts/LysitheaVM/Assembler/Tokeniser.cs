using System;
using System.Text;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class Tokeniser
    {
        #region Fields
        public string Current { get; private set; } = "";
        public int LineNumber { get; private set; } = 0;
        public int ColumnNumber { get; private set; } = 0;

        private int startLineNumber = 0;
        private int startColumnNumber = 0;
        private char inQuote = '\0';
        private char returnSymbol = '\0';
        private bool escaped = false;
        private bool inComment = false;
        private readonly StringBuilder accumulator = new StringBuilder();
        public readonly IReadOnlyList<string> Input;

        public CodeLocation CurrentLocation => new CodeLocation(this.startLineNumber, this.startColumnNumber, this.LineNumber, this.ColumnNumber);
        #endregion

        #region Constructor
        public Tokeniser(IReadOnlyList<string> input)
        {
            this.Input = input;
        }
        #endregion

        #region Methods
        public CodeLocation CreateLocation(int startLineNumber, int startColumnNumber)
        {
            return new CodeLocation(startLineNumber, startColumnNumber, this.LineNumber, this.ColumnNumber);
        }

        public bool MoveNext()
        {
            if (this.returnSymbol != '\0')
            {
                this.Current = this.returnSymbol.ToString();
                this.returnSymbol = '\0';
                return true;
            }

            while (this.LineNumber < this.Input.Count)
            {
                var line = this.Input[this.LineNumber];
                if (line.Length == 0)
                {
                    this.LineNumber++;
                    continue;
                }

                var ch = line[this.ColumnNumber++];
                var atEndOfLine = this.ColumnNumber >= line.Length;
                if (atEndOfLine)
                {
                    this.ColumnNumber = 0;
                    this.LineNumber++;
                }

                if (this.inComment)
                {
                    if (atEndOfLine)
                    {
                        this.inComment = false;
                    }
                    continue;
                }

                if (this.inQuote != '\0')
                {
                    if (this.escaped)
                    {
                        switch (ch)
                        {
                            case '"':
                            case '\'':
                            case '\\':
                            {
                                this.AppendChar(ch);
                                break;
                            }
                            case 't':
                            {
                                this.AppendChar('\t');
                                break;
                            }
                            case 'r':
                            {
                                this.AppendChar('\r');
                                break;
                            }
                            case 'n':
                            {
                                this.AppendChar('\n');
                                break;
                            }
                        }
                        this.escaped = false;
                        continue;
                    }
                    else if (ch == '\\')
                    {
                        this.escaped = true;
                        continue;
                    }

                    this.AppendChar(ch);

                    if (ch == this.inQuote)
                    {
                        this.Current = this.accumulator.ToString();
                        this.accumulator.Clear();
                        this.inQuote = '\0';
                        return true;
                    }
                    else if (atEndOfLine)
                    {
                        this.AppendChar('\n');
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case ';':
                        {
                            this.inComment = true;
                            break;
                        }

                        case '"':
                        case '\'':
                        {
                            this.inQuote = ch;
                            this.AppendChar(ch);
                            break;
                        }

                        case '(': case ')':
                        case '[': case ']':
                        case '{': case '}':
                        {
                            if (this.accumulator.Length > 0)
                            {
                                this.returnSymbol = ch;
                                this.Current = this.accumulator.ToString();
                                this.accumulator.Clear();
                            }
                            else
                            {
                                this.Current = ch.ToString();
                            }
                            return true;
                        }

                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                        {
                            if (this.accumulator.Length > 0)
                            {
                                this.Current = accumulator.ToString();
                                accumulator.Clear();
                                return true;
                            }
                            break;
                        }

                        default:
                        {
                            this.AppendChar(ch);
                            break;
                        }
                    }
                }
            }

            this.Current = "";
            return false;
        }

        private void AppendChar(char ch)
        {
            if (this.accumulator.Length == 0)
            {
                this.startLineNumber = this.LineNumber;
                this.startColumnNumber = this.ColumnNumber - 1;
            }
            this.accumulator.Append(ch);
        }
        #endregion
    }
}