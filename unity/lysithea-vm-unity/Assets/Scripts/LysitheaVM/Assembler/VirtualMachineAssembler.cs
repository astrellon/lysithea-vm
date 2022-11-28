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

        public readonly Scope BuiltinScope = new Scope();
        private int labelCount = 0;
        private List<string> keywordParsingStack = new List<string>();
        private readonly Stack<LoopLabels> loopStack = new Stack<LoopLabels>();
        #endregion

        #region Methods

        #region Parse From Input
        public Script ParseFromReader(TextReader reader)
        {
            var parsed = VirtualMachineParser.ReadAllTokens(reader);

            var code = this.ParseGlobalFunction(parsed);
            var scriptScope = new Scope();
            scriptScope.CombineScope(this.BuiltinScope);

            return new Script(scriptScope, code);
        }

        public Script ParseFromStream(Stream input)
        {
            using var reader = new StreamReader(input);
            return ParseFromReader(reader);
        }

        public Script ParseFromFile(string filePath)
        {
            using var file = File.OpenRead(filePath);
            return ParseFromStream(file);
        }

        public Script ParseFromText(string input)
        {
            using var reader = new StringReader(input);
            return ParseFromReader(reader);
        }

        public Function ParseGlobalFunction(ArrayValue input)
        {
            var tempCodeLines = input.Value.SelectMany(Parse).ToList();
            var result = VirtualMachineAssembler.ProcessTempFunction(Function.EmptyParameters, tempCodeLines, "global");
            return result;
        }

        public List<ITempCodeLine> Parse(IValue input)
        {
            if (input is ArrayValue arrayValue)
            {
                if (!arrayValue.Value.Any())
                {
                    return new List<ITempCodeLine>();
                }

                var first = arrayValue.Value.First();
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

                    this.keywordParsingStack.Add("func-call");

                    var result = new List<ITempCodeLine>();
                    // Handle general opcode or function call.
                    foreach (var item in arrayValue.Value.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }
                    result.AddRange(this.OptimiseCallSymbolValue(firstSymbolValue.Value, arrayValue.Length - 1));

                    this.keywordParsingStack.PopBack();

                    return result;
                }

                // Any array that doesn't start with a symbol we assume it's a data array.
            }
            else if (input is VariableValue varValue)
            {
                if (!varValue.IsLabel)
                {
                    return this.OptimiseGetSymbolValue(varValue.Value);
                }
            }

            return new List<ITempCodeLine> { new CodeLine(Operator.Push, input) };
        }
        #endregion

        #region Keyword Parsing
        public List<ITempCodeLine> ParseFunctionKeyword(ArrayValue arrayValue)
        {
            var function = ParseFunction(arrayValue);
            var functionValue = new FunctionValue(function);
            var result = new List<ITempCodeLine> { new CodeLine(Operator.Push, functionValue) };
            var currentKeyword = this.keywordParsingStack.Count > 1 ? this.keywordParsingStack[this.keywordParsingStack.Count - 2] : FunctionKeyword;
            if (function.HasName && currentKeyword == FunctionKeyword)
            {
                result.Add(new CodeLine(Operator.Define, new StringValue(function.Name)));
            }
            return result;
        }

        public List<ITempCodeLine> ParseDefineSet(ArrayValue input, bool isDefine)
        {
            var opCode = isDefine ? Operator.Define : Operator.Set;
            // Parse the last value as the definable/set-able value.
            var result = this.Parse(input.Value.Last());

            // Loop over all the middle inputs as the values to set.
            // Multiple variables can be set when a function returns multiple results.
            for (var i = input.Length - 2; i >= 1; i--)
            {
                result.Add(new CodeLine(opCode, input[i]));
            }
            return result;
        }

        public List<ITempCodeLine> ParseLoop(ArrayValue input)
        {
            if (input.Length < 3)
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
            for (var i = 2; i < input.Length; i++)
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
            if (input.Length < 3)
            {
                throw new Exception("Condition input has too few inputs");
            }
            if (input.Length > 4)
            {
                throw new Exception("Condition input has too many inputs!");
            }

            var ifLabelNum = this.labelCount++;
            var labelElse = $":CondElse{ifLabelNum}";
            var labelEnd = $":CondEnd{ifLabelNum}";

            var hasElseCall = input.Length == 4;
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
            if (input.Value.All(i => i is ArrayValue))
            {
                return input.Value.SelectMany(Parse);
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
            var name = "";
            var offset = 0;
            if (input[1] is VariableValue || input[1] is StringValue)
            {
                name = input[1].ToString();
                offset = 1;
            }

            var parameters = ((ArrayValue)input[1 + offset]).Value.Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.Value.Skip(2 + offset).SelectMany(Parse).ToList();

            return VirtualMachineAssembler.ProcessTempFunction(parameters, tempCodeLines, name);
        }

        public List<ITempCodeLine> ParseJump(ArrayValue input)
        {
            var parse = Parse(input.Value[1]);
            if (parse.Count == 1 && parse[0] is CodeLine codeLine && codeLine.Operator == Operator.Push)
            {
                return new List<ITempCodeLine> { new CodeLine(Operator.Jump, codeLine.Input) };
            }
            parse.Add(new CodeLine(Operator.Jump, null));
            return parse;
        }

        public List<ITempCodeLine> ParseReturn(ArrayValue input)
        {
            var result = new List<ITempCodeLine>();
            foreach (var item in input.Value.Skip(1))
            {
                result.AddRange(Parse(item));
            }
            result.Add(new CodeLine(Operator.Return, null));
            return result;
        }

        public List<ITempCodeLine> ParseNegative(ArrayValue input)
        {
            if (input.Length >= 3)
            {
                return this.ParseMathOperator(Operator.Sub, input);
            }
            else if (input.Length == 2)
            {
                // If it's a constant already, just push the negative.
                if (input[1] is NumberValue numValue)
                {
                    return new List<ITempCodeLine> { new CodeLine(Operator.Push, new NumberValue(-numValue.Value)) };
                }

                var result = this.Parse(input[1]);
                result.Add(new CodeLine(Operator.UnaryNegative, null));
                return result;
            }
            else
            {
                throw new Exception($"Negative/Sub operator expects either 1 or 2 inputs");
            }
        }

        public List<ITempCodeLine> ParseOnePushInput(Operator opCode, ArrayValue input)
        {
            if (input.Value.Count < 2)
            {
                throw new Exception($"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<ITempCodeLine>();
            foreach (var item in input.Value.Skip(1))
            {
                result.AddRange(this.Parse(item));
                result.Add(new CodeLine(opCode, null));
            }

            return result;
        }

        public List<ITempCodeLine> ParseMathOperator(Operator opCode, ArrayValue input)
        {
            if (input.Value.Count < 3)
            {
                throw new Exception($"Expecting at least 3 inputs for: {opCode}");
            }

            var result = this.Parse(input.Value[1]);
            foreach (var item in input.Value.Skip(2))
            {
                if (item is NumberValue)
                {
                    result.Add(new CodeLine(opCode, item));
                }
                else
                {
                    result.AddRange(this.Parse(item));
                    result.Add(new CodeLine(opCode, null));
                }
            }

            return result;
        }

        public List<ITempCodeLine> ParseOneVariableUpdate(Operator opCode, ArrayValue input)
        {
            if (input.Value.Count < 2)
            {
                throw new Exception($"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<ITempCodeLine>();
            foreach (var item in input.Value.Skip(1))
            {
                var varName = new StringValue(item.ToString());
                result.Add(new CodeLine(opCode, varName));
            }

            return result;
        }

        public List<ITempCodeLine> ParseStringConcat(ArrayValue input)
        {
            var result = new List<ITempCodeLine>();
            foreach (var item in input.Value.Skip(1))
            {
                result.AddRange(this.Parse(item));
            }
            result.Add(new CodeLine(Operator.StringConcat, new NumberValue(input.Value.Count - 1)));
            return result;
        }

        public List<ITempCodeLine> TransformSetOperator(ArrayValue arrayValue)
        {
            var opCode = arrayValue.Value[0].ToString();
            opCode = opCode.Substring(0, opCode.Length - 1);

            var varName = arrayValue.Value[1].ToString();
            var newCode = arrayValue.Value.ToList();
            newCode[0] = new VariableValue(opCode);

            var wrappedCode = new List<IValue>();
            wrappedCode.Add(new VariableValue("set"));
            wrappedCode.Add(new VariableValue(varName));
            wrappedCode.Add(new ArrayValue(newCode));

            return this.Parse(new ArrayValue((wrappedCode)));
        }

        public virtual List<ITempCodeLine> ParseKeyword(VariableValue firstSymbol, ArrayValue arrayValue)
        {
            List<ITempCodeLine>? result = null;
            this.keywordParsingStack.Add(firstSymbol.Value);
            switch (firstSymbol.Value)
            {
                case FunctionKeyword: result = this.ParseFunctionKeyword(arrayValue); break;
                case ContinueKeyword: result = this.ParseLoopJump(ContinueKeyword, true); break;
                case BreakKeyword: result = this.ParseLoopJump(BreakKeyword, false); break;
                case SetKeyword: result = this.ParseDefineSet(arrayValue, false); break;
                case DefineKeyword: result = this.ParseDefineSet(arrayValue, true); break;
                case LoopKeyword: result = this.ParseLoop(arrayValue); break;
                case IfKeyword: result = this.ParseCond(arrayValue, true); break;
                case UnlessKeyword: result = this.ParseCond(arrayValue, false); break;
                case JumpKeyword: result = this.ParseJump(arrayValue); break;
                case ReturnKeyword: result = this.ParseReturn(arrayValue); break;

                // Operators
                case "+":  result = this.ParseMathOperator(Operator.Add, arrayValue); break;
                case "-":  result = this.ParseNegative(arrayValue); break;
                case "*":  result = this.ParseMathOperator(Operator.Multiply, arrayValue); break;
                case "/":  result = this.ParseMathOperator(Operator.Divide, arrayValue); break;
                case "<":  result = this.ParseMathOperator(Operator.LessThan, arrayValue); break;
                case "<=": result = this.ParseMathOperator(Operator.LessThanEquals, arrayValue); break;
                case "==": result = this.ParseMathOperator(Operator.Equals, arrayValue); break;
                case "!=": result = this.ParseMathOperator(Operator.NotEquals, arrayValue); break;
                case ">":  result = this.ParseMathOperator(Operator.GreaterThan, arrayValue); break;
                case ">=": result = this.ParseMathOperator(Operator.GreaterThanEquals, arrayValue); break;
                case "&&": result = this.ParseMathOperator(Operator.And, arrayValue); break;
                case "||": result = this.ParseMathOperator(Operator.Or, arrayValue); break;
                case "!":  result = this.ParseOnePushInput(Operator.Not, arrayValue); break;

                case "++": result = this.ParseOneVariableUpdate(Operator.Inc, arrayValue); break;
                case "--": result = this.ParseOneVariableUpdate(Operator.Dec, arrayValue); break;

                case "$":  result = this.ParseStringConcat(arrayValue); break;

                // Conjoined Operators
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "&&=":
                case "||=":
                case "$=":
                    result = this.TransformSetOperator(arrayValue); break;
            }

            this.keywordParsingStack.PopBack();

            return result ?? new List<ITempCodeLine>();
        }
        #endregion

        #region Helper Methods
        private List<ITempCodeLine> OptimiseCallSymbolValue(string input, int numArgs)
        {
            var numArgsValue = new NumberValue(numArgs);
            var isProperty = IsGetPropertyRequest(input, out var parentKey, out var property);

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
                            new CodeLine(Operator.CallDirect, new ArrayValue(callValue))
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
                            new CodeLine(Operator.CallDirect, new ArrayValue(callValue))
                        };
                    }

                    throw new Exception($"Attempting to call a value that is not a function: {input.ToString()} = {foundParent.ToString()}");
                }
            }

            // Could not find the parent right now, so look for the parent at runtime.
            var result = new List<ITempCodeLine> { new CodeLine(Operator.Get, new StringValue(parentKey)) };

            // If this was also a property check also look up the property at runtime.
            if (isProperty)
            {
                result.Add(new CodeLine(Operator.GetProperty, property));
            }

            result.Add(new CodeLine(Operator.Call, numArgsValue));
            return result;
        }

        private List<ITempCodeLine> OptimiseGetSymbolValue(string input)
        {
            var isArgumentUnpack = false;
            if (input.StartsWith("..."))
            {
                isArgumentUnpack = true;
                input = input.Substring(3);
            }

            var result = new List<ITempCodeLine>();

            var isProperty = IsGetPropertyRequest(input, out var parentKey, out var property);
            // Check if we know about the parent object? (eg: string.length, the parent is the string object)
            if (this.BuiltinScope.TryGetKey(parentKey, out var foundParent))
            {
                // If the get is for a property? (eg: string.length, length is the property)
                if (isProperty)
                {
                    if (ValuePropertyAccess.TryGetProperty(foundParent, property, out var foundProperty))
                    {
                        // If we found the property then we're done and we can just push that known value onto the stack.
                        result.Add(new CodeLine(Operator.Push, foundProperty));
                    }
                    else
                    {
                        // We didn't find the property at compile time, so look it up at run time.
                        result.Add(new CodeLine(Operator.Push, foundParent));
                        result.Add(new CodeLine(Operator.GetProperty, property));
                    }
                }
                else
                {
                    // This was not a property request but we found the parent so just push onto the stack.
                    result.Add(new CodeLine(Operator.Push, foundParent));
                }
            }
            else
            {
                // Could not find the parent right now, so look for the parent at runtime.
                result.Add(new CodeLine(Operator.Get, new StringValue(parentKey)));

                // If this was also a property check also look up the property at runtime.
                if (isProperty)
                {
                    result.Add(new CodeLine(Operator.GetProperty, property));
                }
            }

            if (isArgumentUnpack)
            {
                result.Add(new CodeLine(Operator.ToArgument, null));
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

        private static Function ProcessTempFunction(IReadOnlyList<string> parameters, IReadOnlyList<ITempCodeLine> tempCodeLines, string name)
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

            return new Function(code, parameters, labels, name);
        }
        #endregion

        #endregion
    }
}