using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public class VirtualMachineLispAssembler
    {
        private const string FunctionKeyword = "function";
        private const string LoopKeyword = "loop";
        private const string IfKeyword = "if";
        private const string UnlessKeyword = "unless";
        private const string SetKeyword = "set";
        private const string DefineKeyword = "define";

        private int labelCount = 0;

        public readonly Scope BuiltinScope = new Scope();

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

            if (result[0] is TempCodeLine parsedCodeLine)
            {
                if (parsedCodeLine.Argument is FunctionValue procValue)
                {
                    procValue.Value.Name = input[1].ToString();
                }
            }
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

        public IEnumerable<ITempCodeLine> ParseCond(ArrayValue input, bool isIfStatement)
        {
            var ifLabelNum = this.labelCount++;
            var labelElse = $":CondElse{ifLabelNum}";
            var labelEnd = $":CondEnd{ifLabelNum}";

            var comparisonCall = (ArrayValue)input[1];
            var result = Parse(comparisonCall).ToList();

            var hasElseCall = input.Count >= 4;

            var jumpOperator = isIfStatement ? Operator.JumpFalse : Operator.JumpTrue;

            if (hasElseCall)
            {
                result.Add(new TempCodeLine(jumpOperator, new StringValue(labelElse)));

                var ifTrueCall = (ArrayValue)input[2];
                if (ifTrueCall.All(i => i is ArrayValue))
                {
                    result.AddRange(ifTrueCall.SelectMany(Parse));
                }
                else
                {
                    result.AddRange(Parse(ifTrueCall));
                }
                result.Add(new TempCodeLine(Operator.Jump, new StringValue(labelEnd)));

                result.Add(new LabelCodeLine(labelElse));
                var ifFalseCall = (ArrayValue)input[3];
                result.AddRange(Parse(ifFalseCall));
                result.Add(new LabelCodeLine(labelEnd));
            }
            else
            {
                result.Add(new TempCodeLine(jumpOperator, new StringValue(labelEnd)));

                var ifTrueCall = (ArrayValue)input[2];
                result.AddRange(Parse(ifTrueCall));

                result.Add(new LabelCodeLine(labelEnd));
            }

            return result;
        }

        public IEnumerable<ITempCodeLine> ParseJump(Operator jumpOpCode, ArrayValue input)
        {
            return new[] { new TempCodeLine(jumpOpCode, input[1]) };
        }


        public IEnumerable<ITempCodeLine> Parse(IValue input)
        {
            if (input is NumberValue ||
                input is StringValue ||
                input is ObjectValue ||
                input is NullValue ||
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

                    switch (firstSymbolValue.Value)
                    {
                        case FunctionKeyword:
                            {
                                var function = ParseFunction(arrayValue);
                                var functionValue = new FunctionValue(function);
                                return new[] { new TempCodeLine(Operator.Push, functionValue) };
                            }
                        case SetKeyword: return ParseSet(arrayValue);
                        case DefineKeyword: return ParseDefine(arrayValue);
                        case LoopKeyword: return ParseLoop(arrayValue);
                        case IfKeyword: return ParseCond(arrayValue, true);
                        case UnlessKeyword: return ParseCond(arrayValue, false);
                    }

                    var isOpCode = VirtualMachineAssembler.TryParseOperator(firstSymbolValue.Value, out var opCode);
                    if (isOpCode && VirtualMachineAssembler.IsJumpCall(opCode))
                    {
                        return ParseJump(opCode, arrayValue);
                    }

                    // Handle general opcode or function call.
                    foreach (var item in arrayValue.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }

                    IValue? codeLineInput = null;

                    if (!isOpCode)
                    {
                        // result.Add(this.OptimiseGetSymbolValue(firstSymbolValue));
                        // result.Add(new TempCodeLine(Operator.Call, (NumberValue)(arrayValue.Count - 1)));
                        result.AddRange(this.OptimiseCallSymbolValue(firstSymbolValue, arrayValue.Count - 1));
                    }
                    else
                    {
                        codeLineInput = arrayValue.Last();

                        if (opCode != Operator.Push)
                        {
                            result.Add(new TempCodeLine(opCode, codeLineInput));
                        }
                    }
                }
                else
                {
                    result.Add(new TempCodeLine(Operator.Push, input));
                }
                return result;
            }

            if (input is SymbolValue symbolValue)
            {
                if (symbolValue.Value[0] != ':')
                {
                    return new [] { this.OptimiseGetSymbolValue(symbolValue) };
                }
                else
                {
                    return new[] { new TempCodeLine(Operator.Push, input) };
                }
            }

            throw new Exception("Unknown Lisp value");
        }

        public Function ParseFromText(string input)
        {
            var tokens = VirtualMachineLispParser.Tokenize(input);
            var parsed = VirtualMachineLispParser.ReadAllTokens(tokens);
            return this.ParseGlobalFunction(parsed);
        }

        public Function ParseFunction(ArrayValue input)
        {
            var parameters = ((ArrayValue)input[1]).Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.Skip(2).SelectMany(Parse).ToList();

            return VirtualMachineAssembler.ProcessTempFunction(parameters, tempCodeLines);
        }

        public Function ParseGlobalFunction(ArrayValue input)
        {
            var tempCodeLines = input.SelectMany(Parse).ToList();
            return VirtualMachineAssembler.ProcessTempFunction(Function.EmptyParameters, tempCodeLines);
        }

        private IEnumerable<TempCodeLine> OptimiseCallSymbolValue(SymbolValue input, int numArgs)
        {
            var getSymbol = GetSymbolValue(input);
            if (this.BuiltinScope.TryGet(getSymbol, out var value))
            {
                yield return new TempCodeLine(Operator.CallDirect, new ArrayValue(new IValue[] { value, (NumberValue)numArgs }));
            }
            else
            {
                yield return this.ParseGet(getSymbol);
                yield return new TempCodeLine(Operator.Call, (NumberValue)(numArgs));
            }
        }

        private TempCodeLine OptimiseGetSymbolValue(SymbolValue input)
        {
            var getSymbol = GetSymbolValue(input);
            if (this.BuiltinScope.TryGet(getSymbol, out var value))
            {
                return new TempCodeLine(Operator.Push, value);
            }

            return this.ParseGet(getSymbol);
        }

        private TempCodeLine ParseGet(IValue input)
        {
            var opCode = Operator.Get;
            if (input is ArrayValue)
            {
                opCode = Operator.GetProperty;
            }
            return new TempCodeLine(opCode, input);
        }

        private static IValue GetSymbolValue(SymbolValue input)
        {
            if (input.Value.Contains('.'))
            {
                var split = input.Value.Split('.').Select(c => new SymbolValue(c) as IValue);
                return new ArrayValue(split.ToList());
            }

            return input;
        }
        #endregion
    }
}