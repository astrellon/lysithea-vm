using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LysitheaVM
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
        private const string JumpKeyword = "jump";
        private const string ReturnKeyword = "return";
        private static readonly string[] NewlineSeparators = new[] { "\n", "\r", "\r\n" };

        public readonly Scope BuiltinScope = new Scope();
        private int labelCount = 0;
        private List<string> keywordParsingStack = new List<string>();
        private IReadOnlyList<string> fullText = new string[1]{""};
        private string sourceName = "";
        private readonly Stack<LoopLabels> loopStack = new Stack<LoopLabels>();
        #endregion

        #region Methods

        #region Parse From Input
        public Script ParseFromFile(string filePath)
        {
            return this.ParseFromText(filePath, File.ReadAllLines(filePath));
        }

        public Script ParseFromText(string sourceName, string input)
        {
            return this.ParseFromText(sourceName, input.Split(NewlineSeparators, StringSplitOptions.None));
        }

        public Script ParseFromText(string sourceName, IReadOnlyList<string> input)
        {
            this.fullText = input;
            this.sourceName = sourceName;
            var parsed = VirtualMachineParser.ReadAllTokens(input);

            var code = this.ParseGlobalFunction(parsed);
            var scriptScope = new Scope();
            scriptScope.CombineScope(this.BuiltinScope);

            return new Script(scriptScope, code);
        }

        public Function ParseGlobalFunction(TokenList input)
        {
            var tempCodeLines = input.Data.SelectMany(Parse).ToList();
            var result = this.ProcessTempFunction(Function.EmptyParameters, tempCodeLines, "global");
            return result;
        }

        public List<ITempCodeLine> Parse(IToken input)
        {
            if (input is TokenList arrayValue)
            {
                if (!arrayValue.Data.Any())
                {
                    return new List<ITempCodeLine>();
                }

                var first = arrayValue.Data.First();
                // If the first item in an array is a symbol we assume that it is a function call or a label
                if (first is Token firstToken && first.TryGetValue<VariableValue>(out var firstSymbolValue))
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

                    this.keywordParsingStack.Add("func-call");

                    var result = new List<ITempCodeLine>();
                    // Handle general opcode or function call.
                    foreach (var item in arrayValue.Data.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }
                    result.AddRange(this.OptimiseCallSymbolValue(firstToken, arrayValue.Data.Count - 1));

                    this.keywordParsingStack.PopBack();

                    return result;
                }

                // Any array that doesn't start with a symbol we assume it's a data array.
            }
            else if (input is Token inputToken && input.TryGetValue<VariableValue>(out var varValue))
            {
                if (!varValue.IsLabel)
                {
                    return this.OptimiseGetSymbolValue(inputToken);
                }
            }

            return new List<ITempCodeLine> { new TempCodeLine(Operator.Push, input) };
        }
        #endregion

        #region Keyword Parsing
        public List<ITempCodeLine> ParseFunctionKeyword(TokenList arrayValue)
        {
            var function = ParseFunction(arrayValue);
            var functionValue = new FunctionValue(function);
            var functionToken = arrayValue.Copy(functionValue);
            var result = new List<ITempCodeLine> { new TempCodeLine(Operator.Push, functionToken) };
            var currentKeyword = this.keywordParsingStack.Count > 1 ? this.keywordParsingStack[this.keywordParsingStack.Count - 2] : FunctionKeyword;
            if (function.HasName && currentKeyword == FunctionKeyword)
            {
                result.Add(new TempCodeLine(Operator.Define, arrayValue.Copy(new StringValue(function.Name))));
            }
            return result;
        }

        public List<ITempCodeLine> ParseDefineSet(TokenList input, bool isDefine)
        {
            var opCode = isDefine ? Operator.Define : Operator.Set;
            // Parse the last value as the definable/set-able value.
            var result = this.Parse(input.Data.Last());

            // Loop over all the middle inputs as the values to set.
            // Multiple variables can be set when a function returns multiple results.
            for (var i = input.Data.Count - 2; i >= 1; i--)
            {
                result.Add(new TempCodeLine(opCode, input.Data[i]));
            }
            return result;
        }

        public List<ITempCodeLine> ParseLoop(TokenList input)
        {
            if (input.Data.Count < 3)
            {
                throw new Exception("Loop input has too few inputs");
            }

            var loopLabelNum = this.labelCount++;
            var labelStart = new StringValue($":LoopStart{loopLabelNum}");
            var labelEnd = new StringValue($":LoopEnd{loopLabelNum}");

            this.loopStack.Push(new LoopLabels(labelStart, labelEnd));

            var result = new List<ITempCodeLine> { new LabelCodeLine(labelStart.Value) };
            var comparisonCall = (TokenList)input.Data[1];
            result.AddRange(Parse(comparisonCall));

            result.Add(new TempCodeLine(Operator.JumpFalse, comparisonCall.Copy(labelEnd)));
            for (var i = 2; i < input.Data.Count; i++)
            {
                result.AddRange(Parse(input.Data[i]));
            }
            result.Add(new TempCodeLine(Operator.Jump, comparisonCall.Copy(labelStart)));
            result.Add(new LabelCodeLine(labelEnd.Value));

            this.loopStack.Pop();

            return result;
        }

        public List<ITempCodeLine> ParseCond(TokenList input, bool isIfStatement)
        {
            if (input.Data.Count < 3)
            {
                throw new Exception("Condition input has too few inputs");
            }
            if (input.Data.Count > 4)
            {
                throw new Exception("Condition input has too many inputs!");
            }

            var ifLabelNum = this.labelCount++;
            var labelElse = $":CondElse{ifLabelNum}";
            var labelEnd = $":CondEnd{ifLabelNum}";

            var hasElseCall = input.Data.Count == 4;
            var jumpOperator = isIfStatement ? Operator.JumpFalse : Operator.JumpTrue;

            var comparisonCall = (TokenList)input.Data[1];
            var firstBlock = (TokenList)input.Data[2];

            var result = Parse(comparisonCall);

            if (hasElseCall)
            {
                // Jump to else if the condition doesn't match
                result.Add(new TempCodeLine(jumpOperator, comparisonCall.Copy(new StringValue(labelElse))));

                // First block of code
                result.AddRange(this.ParseFlatten(firstBlock));
                // Jump after the condition, skipping second block of code.
                result.Add(new TempCodeLine(Operator.Jump, firstBlock.Copy(new StringValue(labelEnd))));

                // Jump target for else
                result.Add(new LabelCodeLine(labelElse));

                // Second 'else' block of code
                var secondBlock = (TokenList)input.Data[3];
                result.AddRange(this.ParseFlatten(secondBlock));
            }
            else
            {
                // We only have one block, so jump to the end of the block if the condition doesn't match
                result.Add(new TempCodeLine(jumpOperator, comparisonCall.Copy(new StringValue(labelEnd))));

                result.AddRange(ParseFlatten(firstBlock));
            }

            // Jump target after the condition
            result.Add(new LabelCodeLine(labelEnd));

            return result;
        }

        public IEnumerable<ITempCodeLine> ParseFlatten(TokenList input)
        {
            if (input.Data.All(i => i is TokenList))
            {
                return input.Data.SelectMany(Parse);
            }

            return Parse(input);
        }

        public List<ITempCodeLine> ParseLoopJump(IToken token, string keyword, bool jumpToStart)
        {
            if (!this.loopStack.Any())
            {
                throw new Exception($"Unexpected {keyword} outside of loop");
            }

            var loopLabel = this.loopStack.Last();
            return new List<ITempCodeLine> { new TempCodeLine(Operator.Jump, token.Copy(jumpToStart ? loopLabel.Start : loopLabel.End)) };
        }

        public Function ParseFunction(TokenList input)
        {
            var name = "";
            var offset = 0;
            if (input.Data[1].TryGetValue<VariableValue>(out var varValue) || input.Data[1].TryGetValue<StringValue>(out var strValue))
            {
                name = input.Data[1].ToString();
                offset = 1;
            }

            var parameters = ((TokenList)input.Data[1 + offset]).Data.Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.Data.Skip(2 + offset).SelectMany(Parse).ToList();

            return this.ProcessTempFunction(parameters, tempCodeLines, name);
        }

        public List<ITempCodeLine> ParseJump(TokenList input)
        {
            var parse = Parse(input.Data[1]);
            if (parse.Count == 1 && parse[0] is CodeLine codeLine && codeLine.Operator == Operator.Push)
            {
                return new List<ITempCodeLine> { new TempCodeLine(Operator.Jump, input.Copy(codeLine.Input)) };
            }
            parse.Add(new TempCodeLine(Operator.Jump, input.ToEmpty()));
            return parse;
        }

        public List<ITempCodeLine> ParseReturn(TokenList input)
        {
            var result = new List<ITempCodeLine>();
            foreach (var item in input.Data.Skip(1))
            {
                result.AddRange(Parse(item));
            }
            result.Add(new TempCodeLine(Operator.Return, input.ToEmpty()));
            return result;
        }

        public List<ITempCodeLine> ParseNegative(TokenList input)
        {
            if (input.Data.Count >= 3)
            {
                return this.ParseOperator(Operator.Sub, input);
            }
            else if (input.Data.Count == 2)
            {
                // If it's a constant already, just push the negative.
                if (input.Data[1].TryGetValue<NumberValue>(out var numValue))
                {
                    return new List<ITempCodeLine> { new TempCodeLine(Operator.Push, input.Data[1].Copy(new NumberValue(-numValue.Value))) };
                }

                var result = this.Parse(input.Data[1]);
                result.Add(new TempCodeLine(Operator.UnaryNegative, input.Data[1].ToEmpty()));
                return result;
            }
            else
            {
                throw new Exception($"Negative/Sub operator expects at least 1 input");
            }
        }

        public List<ITempCodeLine> ParseOnePushInput(Operator opCode, TokenList input)
        {
            if (input.Data.Count < 2)
            {
                throw new Exception($"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<ITempCodeLine>();
            foreach (var item in input.Data.Skip(1))
            {
                result.AddRange(this.Parse(item));
                result.Add(new TempCodeLine(opCode, item.ToEmpty()));
            }

            return result;
        }

        public List<ITempCodeLine> ParseOperator(Operator opCode, TokenList input)
        {
            if (input.Data.Count < 3)
            {
                throw new Exception($"Expecting at least 3 inputs for: {opCode}");
            }

            var result = this.Parse(input.Data[1]);
            foreach (var item in input.Data.Skip(2))
            {
                if (item.TryGetValue<NumberValue>(out var numValue))
                {
                    result.Add(new TempCodeLine(opCode, item));
                }
                else
                {
                    result.AddRange(this.Parse(item));
                    result.Add(new TempCodeLine(opCode, input.ToEmpty()));
                }
            }

            return result;
        }

        public List<ITempCodeLine> ParseOneVariableUpdate(Operator opCode, TokenList input)
        {
            if (input.Data.Count < 2)
            {
                throw new Exception($"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<ITempCodeLine>();
            foreach (var item in input.Data.Skip(1))
            {
                var varName = new StringValue(item.ToString());
                result.Add(new TempCodeLine(opCode, item.Copy(varName)));
            }

            return result;
        }

        public List<ITempCodeLine> ParseStringConcat(TokenList input)
        {
            var result = new List<ITempCodeLine>();
            foreach (var item in input.Data.Skip(1))
            {
                result.AddRange(this.Parse(item));
            }
            result.Add(new TempCodeLine(Operator.StringConcat, input.Copy(new NumberValue(input.Data.Count - 1))));
            return result;
        }

        public List<ITempCodeLine> TransformAssignmentOperator(TokenList arrayValue)
        {
            var opCode = arrayValue.Data[0].ToString();
            opCode = opCode.Substring(0, opCode.Length - 1);

            var varName = arrayValue.Data[1].ToString();
            var newCode = arrayValue.Data.ToList();
            newCode[0] = arrayValue.Data[0].Copy(new VariableValue(opCode));

            var wrappedCode = new List<IToken>();
            wrappedCode.Add(arrayValue.Copy(new VariableValue("set")));
            wrappedCode.Add(arrayValue.Data[1].Copy(new VariableValue(varName)));
            wrappedCode.Add(new TokenList(arrayValue.Location, newCode));

            return this.Parse(new TokenList(arrayValue.Location, wrappedCode));
        }

        public virtual List<ITempCodeLine> ParseKeyword(VariableValue firstSymbol, TokenList arrayValue)
        {
            List<ITempCodeLine>? result = null;
            this.keywordParsingStack.Add(firstSymbol.Value);
            switch (firstSymbol.Value)
            {
                // General Operators
                case FunctionKeyword: result = this.ParseFunctionKeyword(arrayValue); break;
                case ContinueKeyword: result = this.ParseLoopJump(arrayValue, ContinueKeyword, true); break;
                case BreakKeyword: result = this.ParseLoopJump(arrayValue, BreakKeyword, false); break;
                case SetKeyword: result = this.ParseDefineSet(arrayValue, false); break;
                case DefineKeyword: result = this.ParseDefineSet(arrayValue, true); break;
                case LoopKeyword: result = this.ParseLoop(arrayValue); break;
                case IfKeyword: result = this.ParseCond(arrayValue, true); break;
                case UnlessKeyword: result = this.ParseCond(arrayValue, false); break;
                case JumpKeyword: result = this.ParseJump(arrayValue); break;
                case ReturnKeyword: result = this.ParseReturn(arrayValue); break;

                // Math Operators
                case "+":  result = this.ParseOperator(Operator.Add, arrayValue); break;
                case "-":  result = this.ParseNegative(arrayValue); break;
                case "*":  result = this.ParseOperator(Operator.Multiply, arrayValue); break;
                case "/":  result = this.ParseOperator(Operator.Divide, arrayValue); break;
                case "++": result = this.ParseOneVariableUpdate(Operator.Inc, arrayValue); break;
                case "--": result = this.ParseOneVariableUpdate(Operator.Dec, arrayValue); break;

                // Comparison Operators
                case "<":  result = this.ParseOperator(Operator.LessThan, arrayValue); break;
                case "<=": result = this.ParseOperator(Operator.LessThanEquals, arrayValue); break;
                case "==": result = this.ParseOperator(Operator.Equals, arrayValue); break;
                case "!=": result = this.ParseOperator(Operator.NotEquals, arrayValue); break;
                case ">":  result = this.ParseOperator(Operator.GreaterThan, arrayValue); break;
                case ">=": result = this.ParseOperator(Operator.GreaterThanEquals, arrayValue); break;

                // Boolean Operators
                case "&&": result = this.ParseOperator(Operator.And, arrayValue); break;
                case "||": result = this.ParseOperator(Operator.Or, arrayValue); break;
                case "!":  result = this.ParseOnePushInput(Operator.Not, arrayValue); break;

                // Misc Operators
                case "$":  result = this.ParseStringConcat(arrayValue); break;

                // Conjoined Operators
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "&&=":
                case "||=":
                case "$=":
                    result = this.TransformAssignmentOperator(arrayValue); break;
            }

            this.keywordParsingStack.PopBack();

            return result ?? new List<ITempCodeLine>();
        }
        #endregion

        #region Helper Methods
        private List<ITempCodeLine> OptimiseCallSymbolValue(Token input, int numArgs)
        {
            if (input.Value == null)
            {
                throw new Exception("Call token cannot be null");
            }

            var numArgsValue = new NumberValue(numArgs);
            var isProperty = IsGetPropertyRequest(input.Value.ToString(), out var parentKey, out var property);

            // Check if we know about the parent object? (eg: string.length, the parent is the string object)
            if (this.BuiltinScope.TryGetKey(parentKey, out var foundParent))
            {
                // If the get is for a property? (eg: string.length, length is the property)
                if (isProperty && ValuePropertyAccess.TryGetProperty(foundParent, property, out var foundProperty))
                {
                    if (foundProperty is IFunctionValue)
                    {
                        // If we found the property then we're done and we can just push that known value onto the stack.
                        var callValue = new IValue[]{ foundProperty, numArgsValue };
                        return new List<ITempCodeLine> {
                            new TempCodeLine(Operator.CallDirect, input.Copy(new ArrayValue(callValue)))
                        };
                    }

                    throw new Exception($"Attempting to call a value that is not a function: {input.ToString()} = {foundProperty.ToString()}");
                }
                else if (!isProperty)
                {
                    // This was not a property request but we found the parent so just push onto the stack.
                    if (foundParent is IFunctionValue)
                    {
                        var callValue = new IValue[]{ foundParent, numArgsValue };
                        return new List<ITempCodeLine> {
                            new TempCodeLine(Operator.CallDirect, input.Copy(new ArrayValue(callValue)))
                        };
                    }

                    throw new Exception($"Attempting to call a value that is not a function: {input.ToString()} = {foundParent.ToString()}");
                }
            }

            // Could not find the parent right now, so look for the parent at runtime.
            var result = new List<ITempCodeLine> { new TempCodeLine(Operator.Get, input.Copy(new StringValue(parentKey))) };

            // If this was also a property check also look up the property at runtime.
            if (isProperty)
            {
                result.Add(new TempCodeLine(Operator.GetProperty, input.Copy(property)));
            }

            result.Add(new TempCodeLine(Operator.Call, input.Copy(numArgsValue)));
            return result;
        }

        private List<ITempCodeLine> OptimiseGetSymbolValue(Token input)
        {
            if (input.Value == null)
            {
                throw new Exception("Get symbol token value cannot be null");
            }

            var isArgumentUnpack = false;
            var inputStr = input.Value.ToString();
            if (inputStr.StartsWith("..."))
            {
                isArgumentUnpack = true;
                inputStr = inputStr.Substring(3);
            }

            var result = new List<ITempCodeLine>();

            var isProperty = IsGetPropertyRequest(inputStr, out var parentKey, out var property);
            // Check if we know about the parent object? (eg: string.length, the parent is the string object)
            if (this.BuiltinScope.TryGetKey(parentKey, out var foundParent))
            {
                // If the get is for a property? (eg: string.length, length is the property)
                if (isProperty)
                {
                    if (ValuePropertyAccess.TryGetProperty(foundParent, property, out var foundProperty))
                    {
                        // If we found the property then we're done and we can just push that known value onto the stack.
                        result.Add(new TempCodeLine(Operator.Push, input.Copy(foundProperty)));
                    }
                    else
                    {
                        // We didn't find the property at compile time, so look it up at run time.
                        result.Add(new TempCodeLine(Operator.Push, input.Copy(foundParent)));
                        result.Add(new TempCodeLine(Operator.GetProperty, input.Copy(property)));
                    }
                }
                else
                {
                    // This was not a property request but we found the parent so just push onto the stack.
                    result.Add(new TempCodeLine(Operator.Push, input.Copy(foundParent)));
                }
            }
            else
            {
                // Could not find the parent right now, so look for the parent at runtime.
                result.Add(new TempCodeLine(Operator.Get, input.Copy(new StringValue(parentKey))));

                // If this was also a property check also look up the property at runtime.
                if (isProperty)
                {
                    result.Add(new TempCodeLine(Operator.GetProperty, input.Copy(property)));
                }
            }

            if (isArgumentUnpack)
            {
                result.Add(new TempCodeLine(Operator.ToArgument, input.ToEmpty()));
            }

            return result;
        }

        private static bool IsGetPropertyRequest(string input, out string parentKey, out ArrayValue property)
        {
            if (input.Contains('.'))
            {
                var split = input.Split('.');
                parentKey = split.First();
                property = new ArrayValue(split.Skip(1).Select(c => new VariableValue(c) as IValue).ToList());
                return true;
            }

            parentKey = input;
            property = ArrayValue.Empty;
            return false;
        }

        private Function ProcessTempFunction(IReadOnlyList<string> parameters, IReadOnlyList<ITempCodeLine> tempCodeLines, string name)
        {
            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();
            var locations = new List<CodeLocation>();

            foreach (var tempLine in tempCodeLines)
            {
                if (tempLine is LabelCodeLine labelCodeLine)
                {
                    labels.Add(labelCodeLine.Label, code.Count);
                }
                else if (tempLine is TempCodeLine codeLine)
                {
                    locations.Add(codeLine.Token.Location);
                    code.Add(new CodeLine(codeLine.Operator, codeLine.Token.GetValueCanBeEmpty()));
                }
            }

            var debugSymbols = new DebugSymbols(this.sourceName, this.fullText, locations);

            return new Function(code, parameters, labels, name, debugSymbols);
        }
        #endregion

        #endregion
    }
}