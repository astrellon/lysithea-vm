using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SimpleStackVM
{
    public class VirtualMachineAssembler
    {
        private class LoopLabels
        {
            public readonly StringValue Start;
            public readonly StringValue End;

            public LoopLabels(StringValue start, StringValue end)
            {
                this.Start = start;
                this.End = end;
            }
        }

        #region Fields
        private const string FunctionKeyword = "function";
        private const string LoopKeyword = "loop";
        private const string ContinueKeyword = "continue";
        private const string BreakKeyword = "break";
        private const string IfKeyword = "if";
        private const string UnlessKeyword = "unless";
        private const string SetKeyword = "set";
        private const string DefineKeyword = "define";

        public readonly Scope BuiltinScope = new Scope();
        private int labelCount = 0;
        private readonly Stack<LoopLabels> loopStack = new Stack<LoopLabels>();
        #endregion

        #region Methods

        #region Parse From Input
        public Script ParseFromText(string input)
        {
            var tokens = VirtualMachineParser.Tokenize(input);
            var parsed = VirtualMachineParser.ReadAllTokens(tokens);

            var code = this.ParseGlobalFunction(parsed);
            var scriptScope = new Scope();
            scriptScope.CombineScope(this.BuiltinScope);

            return new Script(scriptScope, code);
        }

        public Function ParseGlobalFunction(ArrayValue input)
        {
            var tempCodeLines = input.SelectMany(Parse).ToList();
            return VirtualMachineAssembler.ProcessTempFunction(Function.EmptyParameters, tempCodeLines);
        }

        public List<ITempCodeLine> Parse(IValue input)
        {
            if (input is ArrayValue arrayValue)
            {
                if (!arrayValue.Any())
                {
                    return new List<ITempCodeLine>();
                }

                var first = arrayValue.First();
                // If the first item in an array is a symbol we assume that it is a function call or a label
                if (first is VariableValue firstSymbolValue)
                {
                    if (firstSymbolValue.IsLabel)
                    {
                        return new List<ITempCodeLine> { new LabelCodeLine(firstSymbolValue.Value) };
                    }

                    // Check for keywords
                    var keywordParse = ParseKeyword(firstSymbolValue, arrayValue);
                    if (keywordParse.Any())
                    {
                        return keywordParse;
                    }

                    // Attempt to parse as an op code
                    var isOpCode = VirtualMachineAssembler.TryParseOperator(firstSymbolValue.Value, out var opCode);
                    if (isOpCode && VirtualMachineAssembler.IsJumpCall(opCode))
                    {
                        return new List<ITempCodeLine> { new CodeLine(opCode, arrayValue[1]) };
                    }

                    var result = new List<ITempCodeLine>();
                    // Handle general opcode or function call.
                    foreach (var item in arrayValue.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }

                    // If it is not an opcode then it must be a function call
                    if (!isOpCode)
                    {
                        result.AddRange(this.OptimiseCallSymbolValue(firstSymbolValue, arrayValue.Count - 1));
                    }
                    else if (opCode != Operator.Push)
                    {
                        result.Add(new CodeLine(opCode, null));
                    }

                    return result;
                }

                // Any array that doesn't start with a symbol we assume it's a data array.
            }
            else if (input is VariableValue symbolValue)
            {
                if (!symbolValue.IsLabel)
                {
                    return new List<ITempCodeLine> { this.OptimiseGetSymbolValue(symbolValue) };
                }
            }

            return new List<ITempCodeLine> { new CodeLine(Operator.Push, input) };
        }
        #endregion

        #region Keyword Parsing
        public List<ITempCodeLine> ParseSet(ArrayValue input)
        {
            var result = this.Parse(input[2]);
            result.Add(new CodeLine(Operator.Set, input[1]));
            return result;
        }

        public List<ITempCodeLine> ParseDefine(ArrayValue input)
        {
            var result = this.Parse(input[2]);
            result.Add(new CodeLine(Operator.Define, input[1]));

            if (result[0] is CodeLine parsedCodeLine)
            {
                if (parsedCodeLine.Input is FunctionValue procValue)
                {
                    procValue.Value.Name = input[1].ToString();
                }
            }
            return result;
        }

        public List<ITempCodeLine> ParseLoop(ArrayValue input)
        {
            if (input.Count < 3)
            {
                throw new Exception("Loop input has too few inputs");
            }

            var loopLabelNum = this.labelCount++;
            var labelStart = new StringValue($":LoopStart{loopLabelNum}");
            var labelEnd = new StringValue($":LoopEnd{loopLabelNum}");

            this.loopStack.Push(new LoopLabels(labelStart, labelEnd));

            var result = new List<ITempCodeLine> { new LabelCodeLine(labelStart.Value) };
            var comparisonCall = (ArrayValue)input[1];
            result.AddRange(Parse(comparisonCall));

            result.Add(new CodeLine(Operator.JumpFalse, labelEnd));
            for (var i = 2; i < input.Count; i++)
            {
                result.AddRange(Parse(input[i]));
            }
            result.Add(new CodeLine(Operator.Jump, labelStart));
            result.Add(new LabelCodeLine(labelEnd.Value));

            this.loopStack.Pop();

            return result;
        }

        public List<ITempCodeLine> ParseCond(ArrayValue input, bool isIfStatement)
        {
            if (input.Count < 3)
            {
                throw new Exception("Condition input has too few inputs");
            }
            if (input.Count > 4)
            {
                throw new Exception("Condition input has too many inputs!");
            }

            var ifLabelNum = this.labelCount++;
            var labelElse = $":CondElse{ifLabelNum}";
            var labelEnd = $":CondEnd{ifLabelNum}";

            var hasElseCall = input.Count == 4;
            var jumpOperator = isIfStatement ? Operator.JumpFalse : Operator.JumpTrue;

            var comparisonCall = (ArrayValue)input[1];
            var firstBlock = (ArrayValue)input[2];

            var result = Parse(comparisonCall);

            if (hasElseCall)
            {
                // Jump to else if the condition doesn't match
                result.Add(new CodeLine(jumpOperator, new StringValue(labelElse)));

                // First block of code
                result.AddRange(this.ParseFlatten(firstBlock));
                // Jump after the condition, skipping second block of code.
                result.Add(new CodeLine(Operator.Jump, new StringValue(labelEnd)));

                // Jump target for else
                result.Add(new LabelCodeLine(labelElse));

                // Second 'else' block of code
                var secondBlock = (ArrayValue)input[3];
                result.AddRange(this.ParseFlatten(secondBlock));
            }
            else
            {
                // We only have one block, so jump to the end of the block if the condition doesn't match
                result.Add(new CodeLine(jumpOperator, new StringValue(labelEnd)));

                result.AddRange(ParseFlatten(firstBlock));
            }

            // Jump target after the condition
            result.Add(new LabelCodeLine(labelEnd));

            return result;
        }

        public IEnumerable<ITempCodeLine> ParseFlatten(ArrayValue input)
        {
            if (input.All(i => i is ArrayValue))
            {
                return input.SelectMany(Parse);
            }

            return Parse(input);
        }

        public List<ITempCodeLine> ParseLoopJump(string keyword, bool jumpToStart)
        {
            if (!this.loopStack.Any())
            {
                throw new Exception($"Unexpected {keyword} outside of loop");
            }

            var loopLabel = this.loopStack.Last();
            return new List<ITempCodeLine> { new CodeLine(Operator.Jump, jumpToStart ? loopLabel.Start : loopLabel.End) };
        }

        public Function ParseFunction(ArrayValue input)
        {
            var parameters = ((ArrayValue)input[1]).Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.Skip(2).SelectMany(Parse).ToList();

            return VirtualMachineAssembler.ProcessTempFunction(parameters, tempCodeLines);
        }

        public List<ITempCodeLine> ParseKeyword(VariableValue firstSymbol, ArrayValue arrayValue)
        {
            switch (firstSymbol.Value)
            {
                case FunctionKeyword:
                    {
                        var function = ParseFunction(arrayValue);
                        var functionValue = new FunctionValue(function);
                        return new List<ITempCodeLine>{ new CodeLine(Operator.Push, functionValue) };
                    }
                case ContinueKeyword: return this.ParseLoopJump(ContinueKeyword, true);
                case BreakKeyword: return this.ParseLoopJump(BreakKeyword, false);
                case SetKeyword: return ParseSet(arrayValue);
                case DefineKeyword: return ParseDefine(arrayValue);
                case LoopKeyword: return ParseLoop(arrayValue);
                case IfKeyword: return ParseCond(arrayValue, true);
                case UnlessKeyword: return ParseCond(arrayValue, false);
            }

            return new List<ITempCodeLine>();
        }
        #endregion

        #region Helper Methods
        private IEnumerable<ITempCodeLine> OptimiseCallSymbolValue(VariableValue input, int numArgs)
        {
            var numArgsValue = new NumberValue(numArgs);

            var getSymbol = GetSymbolValue(input);
            if (this.BuiltinScope.TryGet(getSymbol, out var value))
            {
                yield return new CodeLine(Operator.CallDirect, new ArrayValue(new IValue[] { value, numArgsValue }));

            }
            else
            {
                yield return this.ParseGet(getSymbol);
                yield return new CodeLine(Operator.Call, numArgsValue);
            }
        }

        private ITempCodeLine OptimiseGetSymbolValue(VariableValue input)
        {
            var getSymbol = GetSymbolValue(input);
            if (this.BuiltinScope.TryGet(getSymbol, out var value))
            {
                return new CodeLine(Operator.Push, value);
            }

            return this.ParseGet(getSymbol);
        }

        private ITempCodeLine ParseGet(IValue input)
        {
            if (input is ArrayValue)
            {
                return new CodeLine(Operator.GetProperty, input);
            }
            return new CodeLine(Operator.Get, new StringValue(input.ToString()));
        }

        private static IValue GetSymbolValue(VariableValue input)
        {
            if (input.Value.Contains('.'))
            {
                var split = input.Value.Split('.').Select(c => new VariableValue(c) as IValue);
                return new ArrayValue(split.ToList());
            }

            return input;
        }

        private static Function ProcessTempFunction(IReadOnlyList<string> parameters, IReadOnlyList<ITempCodeLine> tempCodeLines)
        {
            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();

            foreach (var tempLine in tempCodeLines)
            {
                if (tempLine is LabelCodeLine labelCodeLine)
                {
                    labels.Add(labelCodeLine.Label, code.Count);
                }
                else if (tempLine is CodeLine codeLine)
                {
                    code.Add(codeLine);
                }
            }

            return new Function(code, parameters, labels);
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

        private static bool IsJumpCall(Operator input)
        {
            return input == Operator.Call || input == Operator.Jump ||
                input == Operator.JumpTrue || input == Operator.JumpFalse;
        }
        #endregion

        #endregion
    }
}