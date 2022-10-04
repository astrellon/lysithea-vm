using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public static class VirtualMachineLispAssembler
    {
        public interface ITempCodeLine { }

        [DebuggerDisplay("{Description}")]
        public class LabelCodeLine : ITempCodeLine
        {
            public readonly string Label;

            public string Description => $"Label: {this.Label}";

            public LabelCodeLine(string label) { this.Label = label; }
        }

        [DebuggerDisplay("{Description}")]
        public class TempCodeLine : ITempCodeLine
        {
            public readonly Operator Operator;
            public readonly IValue? Argument;

            public string Description
            {
                get
                {
                    if (this.Argument is StringValue stringInput)
                    {
                        return $"{this.Operator} \"{stringInput.Value}\"";
                    }
                    if (this.Argument != null)
                    {
                        return $"{this.Operator} {this.Argument.ToString()}";
                    }

                    return $"{this.Operator} <no arg>";
                }
            }

            public TempCodeLine(Operator op, IValue? argument)
            {
                this.Operator = op;
                this.Argument = argument;
            }
        }
        #region Methods
        public static IEnumerable<ITempCodeLine> Parse(IValue input)
        {
            if (input is NumberValue ||
                input is StringValue ||
                input is SymbolValue ||
                input is BoolValue)
            {
                return new[] { new TempCodeLine(Operator.Push, input) };
            }

            if (input is ArrayValue arrayValue)
            {
                var result = new List<ITempCodeLine>();
                if (!arrayValue.Any())
                {
                    return result;
                }

                var first = arrayValue.First();
                if (first is SymbolValue firstSymbolValue)
                {
                    if (firstSymbolValue.IsLabel)
                    {
                        return new[] { new LabelCodeLine(firstSymbolValue.Value) };
                    }

                    foreach (var item in arrayValue.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }

                    if (TryParseOperator(firstSymbolValue.Value, out var opCode))
                    {
                        result.Add(new TempCodeLine(opCode, first));
                    }
                    else
                    {
                        result.Add(new TempCodeLine(Operator.Call, first));
                    }
                }
                else
                {
                    result.AddRange(arrayValue.SelectMany(Parse));
                }
                return result;
            }

            if (input is SymbolValue symbolValue)
            {
                return new[] { new TempCodeLine(Operator.Get, symbolValue) };
            }

            throw new Exception("Unknown Lisp value");
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
}