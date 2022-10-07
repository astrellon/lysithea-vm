using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public class VirtualMachineLispAssembler
    {
        private const string ProcedureKeyword = "procedure";
        private const string LoopKeyword = "loop";
        private const string IfKeyword = "if";

        private int labelCount = 0;

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
                if (argument != null && argument is SymbolValue symbol)
                {
                    this.Argument = new StringValue(symbol.Value);
                }
                else
                {
                    this.Argument = argument;
                }
            }
        }
        #region Methods
        public IEnumerable<ITempCodeLine> ParseSetOperator(ArrayValue input)
        {
            var result = Parse(input[2]).ToList();
            result.Add(new TempCodeLine(Operator.Set, input[1]));
            return result;
        }

        public IEnumerable<ITempCodeLine> ParseLoop(ArrayValue input)
        {
            var loopLabelNum = this.labelCount++;
            var labelStart = $":LoopStart{loopLabelNum}";
            var labelEnd = $":LoopEnd{loopLabelNum}";

            var result = new List<ITempCodeLine> { new LabelCodeLine(labelStart) };
            var comparisonCall = (ArrayValue)input[1];
            result.AddRange(Parse(comparisonCall));

            result.Add(new TempCodeLine(Operator.JumpFalse, new StringValue(labelEnd)));

            for (var i = 2; i < input.Count; i++)
            {
                result.AddRange(Parse(input[i]));
            }
            result.Add(new TempCodeLine(Operator.Jump, new StringValue(labelStart)));
            result.Add(new LabelCodeLine(labelEnd));

            return result;
        }

        public IEnumerable<ITempCodeLine> ParseIf(ArrayValue input)
        {
            var ifLabelNum = this.labelCount++;
            var labelElse = $":IfElse{ifLabelNum}";
            var labelEnd = $":IfEnd{ifLabelNum}";

            var comparisonCall = (ArrayValue)input[1];
            var result = Parse(comparisonCall).ToList();

            var hasElseCall = input.Count == 4;

            if (hasElseCall)
            {
                result.Add(new TempCodeLine(Operator.JumpFalse, new StringValue(labelElse)));

                var ifTrueCall = (ArrayValue)input[2];
                result.AddRange(Parse(ifTrueCall));
                result.Add(new TempCodeLine(Operator.Jump, new StringValue(labelEnd)));

                result.Add(new LabelCodeLine(labelElse));
                var ifFalseCall = (ArrayValue)input[3];
                result.AddRange(Parse(ifFalseCall));
                result.Add(new LabelCodeLine(labelEnd));
            }
            else
            {
                result.Add(new TempCodeLine(Operator.JumpFalse, new StringValue(labelEnd)));

                var ifTrueCall = (ArrayValue)input[2];
                result.AddRange(Parse(ifTrueCall));

                result.Add(new LabelCodeLine(labelEnd));
            }

            return result;
        }

        public IEnumerable<ITempCodeLine> Parse(IValue input)
        {
            if (input is NumberValue ||
                input is StringValue ||
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

                    var isOpCode = TryParseOperator(firstSymbolValue.Value, out var opCode);
                    if (isOpCode && opCode == Operator.Set)
                    {
                        return ParseSetOperator(arrayValue);
                    }
                    if (!isOpCode && firstSymbolValue.Value == "loop")
                    {
                        return ParseLoop(arrayValue);
                    }
                    if (!isOpCode && firstSymbolValue.Value == "if")
                    {
                        return ParseIf(arrayValue);
                    }

                    // Handle general opcode or procedure call.
                    foreach (var item in arrayValue.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }

                    if (isOpCode)
                    {
                        result.Add(new TempCodeLine(opCode, first));
                    }
                    else
                    {
                        if (!TryParseRunCommand(firstSymbolValue, out var callCodeLine))
                        {
                            throw new Exception($"Unable to parse call code: {firstSymbolValue.ToString()}");
                        }
                        result.Add(new TempCodeLine(Operator.Call, callCodeLine));
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

        public IEnumerable<Procedure> ParseProcedures(ArrayValue input)
        {
            foreach (ArrayValue item in input)
            {
                yield return ParseProcedure(item);
            }
        }

        public Procedure ParseProcedure(ArrayValue input)
        {
            if (!(input[0] is SymbolValue firstSymbol) || firstSymbol.Value != ProcedureKeyword)
            {
                throw new Exception($"Expected procedure token: {input[0].ToString()}");
            }

            var procedureName = input[1].ToString();
            var parameters = ((ArrayValue)input[2]).Select(arg => arg.ToString()).ToList();
            var tempCodeLines = new List<ITempCodeLine>();
            for (var i = 3; i < input.Count; i++)
            {
                tempCodeLines.AddRange(Parse(input[i]));
            }

            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();

            foreach (var tempLine in tempCodeLines)
            {
                if (tempLine is LabelCodeLine labelCodeLine)
                {
                    labels.Add(labelCodeLine.Label, code.Count);
                }
                else if (tempLine is TempCodeLine tempCodeLine)
                {
                    code.Add(new CodeLine(tempCodeLine.Operator, tempCodeLine.Argument));
                }
            }

            return new Procedure(procedureName, code, parameters, labels);
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

        private static bool TryParseJumpLabel(IValue input, out IValue result)
        {
            return TryParseTwoStringInput(input, ':', true, out result);
        }

        private static bool TryParseRunCommand(IValue input, out IValue result)
        {
            return TryParseTwoStringInput(input, '.', false, out result);
        }

        private static bool TryParseTwoStringInput(IValue input, char delimiter, bool includeDelimiter, out IValue result)
        {
            if (input is SymbolValue symbolValue)
            {
                var delimiterIndex = symbolValue.Value.IndexOf(delimiter);
                if (delimiterIndex > 0)
                {
                    var array = new List<IValue>(2);
                    array.Add(new StringValue(symbolValue.Value.Substring(0, delimiterIndex)));
                    if (includeDelimiter)
                    {
                        array.Add(new StringValue(symbolValue.Value.Substring(delimiterIndex)));
                    }
                    else
                    {
                        array.Add(new StringValue(symbolValue.Value.Substring(delimiterIndex + 1)));
                    }

                    result = new ArrayValue(array);
                    return true;
                }

                result = input;
                return true;
            }
            else if (input is ArrayValue arrayValue)
            {
                if (arrayValue.Count == 1)
                {
                    return TryParseTwoStringInput(arrayValue[0], delimiter, includeDelimiter, out result);
                }

                result = input;
                return true;
            }

            result = NullValue.Value;
            return false;
        }
        #endregion
    }
}