using System;
using System.Collections.Generic;
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

        #region Methods
        public IEnumerable<ITempCodeLine> ParseSet(ArrayValue input)
        {
            var result = Parse(input[2]).ToList();
            result.Add(new TempCodeLine(Operator.Set, input[1]));
            return result;
        }

        public IEnumerable<ITempCodeLine> ParseDefine(ArrayValue input)
        {
            var result = Parse(input[2]).ToList();
            result.Add(new TempCodeLine(Operator.Define, input[1]));
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

                    if (firstSymbolValue.Value == "procedure")
                    {
                        var procedure = ParseProcedure(arrayValue);
                        var procedureValue = new ProcedureValue(procedure);
                        return new[] { new TempCodeLine(Operator.Push, procedureValue) };
                    }
                    if (firstSymbolValue.Value == "set")
                    {
                        return ParseSet(arrayValue);
                    }
                    if (firstSymbolValue.Value == "define")
                    {
                        return ParseDefine(arrayValue);
                    }
                    if (firstSymbolValue.Value == "loop")
                    {
                        return ParseLoop(arrayValue);
                    }
                    if (firstSymbolValue.Value == "if")
                    {
                        return ParseIf(arrayValue);
                    }

                    // Handle general opcode or procedure call.
                    foreach (var item in arrayValue.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }

                    IValue? codeLineInput = null;

                    if (!TryParseOperator(firstSymbolValue.Value, out var opCode))
                    {
                        opCode = Operator.Call;
                        result.Add(new TempCodeLine(Operator.Call, firstSymbolValue));
                    }
                    else
                    {
                        if (VirtualMachineAssembler.IsJumpCall(opCode))
                        {
                            if (!TryParseJumpLabel(arrayValue.Last(), out codeLineInput))
                            {
                                throw new Exception($"Error parsing {opCode} input: {input.ToString()}");
                            }
                        }
                        else
                        {
                            codeLineInput = arrayValue.Last();
                        }

                        if (opCode != Operator.Push)
                        {
                            result.Add(new TempCodeLine(opCode, codeLineInput));
                        }
                    }
                }
                else
                {
                    // result.AddRange(arrayValue.SelectMany(Parse));
                    result.Add(new TempCodeLine(Operator.Push, input));
                }
                return result;
            }

            if (input is SymbolValue symbolValue)
            {
                if (symbolValue.Value[0] != ':')
                {
                    return new[] { new TempCodeLine(Operator.Get, symbolValue) };
                }
                else
                {
                    var empty = new ITempCodeLine[0];
                    return empty;
                }
            }

            throw new Exception("Unknown Lisp value");
        }

        public Procedure ParseFromText(string input)
        {
            var tokens = VirtualMachineLispParser.Tokenize(input);
            var parsed = VirtualMachineLispParser.ReadAllTokens(tokens);
            return this.ParseGlobalProcedure(parsed);
        }

        public Procedure ParseProcedure(ArrayValue input)
        {
            var parameters = ((ArrayValue)input[1]).Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.Skip(2).SelectMany(Parse).ToList();

            return VirtualMachineAssembler.ProcessTempProcedure(parameters, tempCodeLines);
        }

        public Procedure ParseGlobalProcedure(ArrayValue input)
        {
            var tempCodeLines = input.SelectMany(Parse).ToList();
            return VirtualMachineAssembler.ProcessTempProcedure(Procedure.EmptyParameters, tempCodeLines);
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