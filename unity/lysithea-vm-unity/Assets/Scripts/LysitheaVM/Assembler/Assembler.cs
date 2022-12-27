using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LysitheaVM
{
    public class Assembler
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
        private const string SwitchKeyword = "switch";
        private const string SetKeyword = "set";
        private const string DefineKeyword = "define";
        private const string ConstKeyword = "const";
        private const string JumpKeyword = "jump";
        private const string ReturnKeyword = "return";
        private static readonly string[] NewlineSeparators = new[] { "\r\n", "\n", "\r" };

        public readonly Scope BuiltinScope = new Scope();
        public IReadOnlyList<string> FullText { get; private set; } = new string[1]{""};
        public string SourceName { get; private set; } = "";

        private int labelCount = 0;
        private List<string> keywordParsingStack = new List<string>();
        private Scope ConstScope = new Scope();
        private readonly Stack<LoopLabels> loopStack = new Stack<LoopLabels>();
        #endregion

        #region Methods
        private AssemblerException ThrowException(Token token, string message)
        {
            var fullMessage = $"{this.SourceName}:line {token.Location.StartLineNumber} column: {token.Location.StartColumnNumber}: {message}";
            return new AssemblerException(this, token, fullMessage);
        }

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
            this.FullText = input;
            this.SourceName = sourceName;

            try
            {
                var parsed = Lexer.ReadFromLines(input);

                var code = this.ParseGlobalFunction(parsed);
                var scriptScope = new Scope();
                scriptScope.CombineScope(this.BuiltinScope);
                scriptScope.CombineScope(this.ConstScope);

                return new Script(scriptScope, code);
            }
            catch (ParserException exp)
            {
                throw this.ThrowException(Token.Value(exp.Location, new StringValue(exp.Token)), exp.Message);
            }
        }

        public Function ParseGlobalFunction(Token input)
        {
            var tempCodeLines = input.TokenList.SelectMany(Parse).ToList();
            var result = this.ProcessTempFunction(Function.EmptyParameters, tempCodeLines, "global");
            return result;
        }

        public List<TempCodeLine> Parse(Token input)
        {
            if (input.Type == TokenType.Expression)
            {
                if (!input.TokenList.Any())
                {
                    return new List<TempCodeLine>();
                }

                var first = input.TokenList.First();
                // If the first item in an array is a symbol we assume that it is a function call or a label
                if (first.TokenValue is VariableValue firstSymbolValue)
                {
                    if (firstSymbolValue.IsLabel)
                    {
                        return new List<TempCodeLine> { TempCodeLine.Label(firstSymbolValue.Value, first) };
                    }

                    // Check for keywords
                    var keywordParse = ParseKeyword(firstSymbolValue, input);
                    if (keywordParse.Any())
                    {
                        if (IsNoOperator(keywordParse))
                        {
                            return new List<TempCodeLine>();
                        }
                        return keywordParse;
                    }

                    this.keywordParsingStack.Add("func-call");

                    var result = new List<TempCodeLine>();
                    // Handle general opcode or function call.
                    foreach (var item in input.TokenList.Skip(1))
                    {
                        result.AddRange(Parse(item));
                    }
                    result.AddRange(this.OptimiseCallSymbolValue(first, input.TokenList.Count - 1));

                    this.keywordParsingStack.PopBack();

                    return result;
                }
                else
                {
                    throw this.ThrowException(input, $"Expression needs to start with a function variable");
                }
            }
            else if (input.Type == TokenType.List)
            {
                // Handle parsing of each element and check if they're all compile time values
                var result = new List<IValue>();
                foreach (var item in input.TokenList)
                {
                    var parsed = this.Parse(item);
                    if (!parsed.Any())
                    {
                        continue;
                    }

                    if (parsed.Count == 1 && parsed[0].Operator == Operator.Push)
                    {
                        result.Add(this.GetValue(parsed[0].Token));
                    }
                    else
                    {
                        throw this.ThrowException(parsed[0].Token, "Unexpected token in list literal");
                    }
                }

                return new List<TempCodeLine> { TempCodeLine.Code(Operator.Push, input.KeepLocation(new ArrayValue(result))) };
            }
            else if (input.Type == TokenType.Map)
            {
                // Handle parsing of each element and check if they're all compile time values
                var result = new Dictionary<string, IValue>();
                foreach (var kvp in input.TokenMap)
                {
                    var parsed = this.Parse(kvp.Value);
                    if (!parsed.Any())
                    {
                        continue;
                    }

                    if (parsed.Count == 1 && parsed[0].Operator == Operator.Push)
                    {
                        result[kvp.Key] = this.GetValue(parsed[0].Token);
                    }
                    else
                    {
                        throw this.ThrowException(parsed[0].Token, $"Unexpected token in map literal for key; {kvp.Key}");
                    }
                }

                return new List<TempCodeLine> { TempCodeLine.Code(Operator.Push, input.KeepLocation(new ObjectValue(result))) };
            }
            else if (input.TokenValue is VariableValue varValue)
            {
                if (!varValue.IsLabel)
                {
                    return this.OptimiseGetSymbolValue(input);
                }
            }

            return new List<TempCodeLine> { TempCodeLine.Code(Operator.Push, input) };
        }
        #endregion

        #region Keyword Parsing
        public List<TempCodeLine> ParseFunctionKeyword(Token input)
        {
            var function = ParseFunction(input);
            var functionValue = new FunctionValue(function);
            var functionToken = input.KeepLocation(functionValue);

            if (this.keywordParsingStack.Count == 1 && function.HasName)
            {
                if (!this.ConstScope.TrySetConstant(function.Name, functionValue))
                {
                    throw this.ThrowException(input, "Unable to define function, constant already exists");
                }
                // Special return case
                return new List<TempCodeLine> { TempCodeLine.Code(Operator.Unknown, Token.Empty(CodeLocation.Empty)) };
            }

            var result = new List<TempCodeLine> { TempCodeLine.Code(Operator.Push, functionToken) };
            var currentKeyword = this.keywordParsingStack.Count > 1 ? this.keywordParsingStack[this.keywordParsingStack.Count - 2] : FunctionKeyword;
            if (function.HasName && currentKeyword == FunctionKeyword)
            {
                result.Add(TempCodeLine.Code(Operator.Define, input.KeepLocation(function.Name)));
            }
            return result;
        }

        public List<TempCodeLine> ParseDefineSet(Token input, bool isDefine)
        {
            var opCode = isDefine ? Operator.Define : Operator.Set;
            // Parse the last value as the definable/set-able value.
            var result = this.Parse(input.TokenList.Last());

            // Loop over all the middle inputs as the values to set.
            // Multiple variables can be set when a function returns multiple results.
            for (var i = input.TokenList.Count - 2; i >= 1; i--)
            {
                var key = this.GetValue(input.TokenList[i]).ToString();
                if (this.ConstScope.TryGetKey(key, out var temp))
                {
                    throw this.ThrowException(input.TokenList[i], $"Attempting to {opCode} a constant: {key}");
                }

                result.Add(TempCodeLine.Code(opCode, input.TokenList[i]));
            }
            return result;
        }

        public List<TempCodeLine> ParseConst(Token input)
        {
            if (input.TokenList.Count != 3)
            {
                throw this.ThrowException(input, "Const requires 2 inputs");
            }

            var result = this.Parse(input.TokenList.Last());
            if (result.Count != 1 || result[0].Operator != Operator.Push)
            {
                throw this.ThrowException(input, "Const value is not a compile time constant");
            }

            var key = this.GetValue(input.TokenList[1]).ToString();
            if (!this.ConstScope.TrySetConstant(key, this.GetValue(result[0].Token)))
            {
                throw this.ThrowException(input, "Cannot redefine a constant");
            }

            return result;
        }

        public List<TempCodeLine> ParseLoop(Token input)
        {
            if (input.TokenList.Count < 3)
            {
                throw this.ThrowException(input, "Loop input has too few inputs");
            }

            var loopLabelNum = this.labelCount++;
            var labelStart = new StringValue($":LoopStart{loopLabelNum}");
            var labelEnd = new StringValue($":LoopEnd{loopLabelNum}");

            this.loopStack.Push(new LoopLabels(labelStart, labelEnd));

            var result = new List<TempCodeLine> { TempCodeLine.Label(labelStart.Value, input) };
            var comparisonCall = input.TokenList[1];
            result.AddRange(Parse(comparisonCall));

            result.Add(TempCodeLine.Code(Operator.JumpFalse, comparisonCall.KeepLocation(labelEnd)));
            for (var i = 2; i < input.TokenList.Count; i++)
            {
                result.AddRange(Parse(input.TokenList[i]));
            }
            result.Add(TempCodeLine.Code(Operator.Jump, comparisonCall.KeepLocation(labelStart)));
            result.Add(TempCodeLine.Label(labelEnd.Value, input));

            this.loopStack.Pop();

            return result;
        }

        public List<TempCodeLine> ParseSwitch(Token input)
        {
            var labelNum = this.labelCount++;
            var labelEnd = $":CondNext{input.TokenList.Count}_{labelNum}";

            var result = new List<TempCodeLine>();

            for (var i = 1; i < input.TokenList.Count; i++)
            {
                var expression = input.TokenList[i];

                if (i > 1)
                {
                    var thisLabelJump = $":CondNext{i}_{labelNum}";
                    result.Add(TempCodeLine.Label(thisLabelJump, expression));
                }

                var comparisonCall = expression.TokenList[0];
                if (!comparisonCall.TokenValue.Equals(BoolValue.True))
                {
                    var nextLabelJump = $":CondNext{i + 1}_{labelNum}";
                    result.AddRange(this.Parse(comparisonCall));
                    result.Add(TempCodeLine.Code(Operator.JumpFalse, expression.KeepLocation(nextLabelJump)));
                }

                result.AddRange(expression.TokenList.Skip(1).SelectMany(this.Parse));

                if (i < input.TokenList.Count - 1)
                {
                    result.Add(TempCodeLine.Code(Operator.Jump, expression.KeepLocation(labelEnd)));
                }
            }

            result.Add(TempCodeLine.Label(labelEnd, input));

            return result;
        }

        public List<TempCodeLine> ParseIfUnless(Token input, bool isIfStatement)
        {
            if (input.TokenList.Count < 3)
            {
                throw this.ThrowException(input, "Condition input has too few inputs");
            }
            if (input.TokenList.Count > 4)
            {
                throw this.ThrowException(input, "Condition input has too many inputs!");
            }

            // Turn the an if/unless statement into a switch statement.
            // (if/unless (comparison) (ifComparison) (else))
            // ->
            // (switch ((comparison) comparisonBody) (true elseBody))
            var tempTokens = new List<Token>{
                input.KeepLocation(isIfStatement ? IfKeyword : UnlessKeyword)
            };

            var comparisonToken = input.TokenList[1];
            var firstBlockToken = input.TokenList[2];

            var newComparison = new List<Token>{ comparisonToken };
            AddHandleNested(newComparison, firstBlockToken);
            tempTokens.Add(Token.Expression(comparisonToken.Location, newComparison));

            if (input.TokenList.Count == 4)
            {
                var elseToken = input.TokenList[3];
                var newElse = new List<Token> { elseToken.KeepLocation(BoolValue.True) };
                AddHandleNested(newElse, elseToken);
                tempTokens.Add(Token.Expression(comparisonToken.Location, newElse));
            }

            var transformedToken = Token.Expression(input.Location, tempTokens);
            return this.ParseSwitch(transformedToken);
        }

        public List<TempCodeLine> ParseLoopJump(Token token, string keyword, bool jumpToStart)
        {
            if (!this.loopStack.Any())
            {
                throw this.ThrowException(token, $"Unexpected {keyword} outside of loop");
            }

            var loopLabel = this.loopStack.Last();
            return new List<TempCodeLine> { TempCodeLine.Code(Operator.Jump, token.KeepLocation(jumpToStart ? loopLabel.Start : loopLabel.End)) };
        }

        public Function ParseFunction(Token input)
        {
            this.ConstScope = new Scope(this.ConstScope);

            var name = "";
            var offset = 0;
            if (input.TokenList[1].TokenValue is VariableValue || input.TokenList[1].TokenValue is StringValue)
            {
                name = input.TokenList[1].ToString();
                offset = 1;
            }

            var parameters = (input.TokenList[1 + offset]).TokenList.Select(arg => arg.ToString()).ToList();
            var tempCodeLines = input.TokenList.Skip(2 + offset).SelectMany(Parse).ToList();

            var result = this.ProcessTempFunction(parameters, tempCodeLines, name);

            if (this.ConstScope.Parent == null)
            {
                throw this.ThrowException(input, "Internal exception, const scope parent lost");
            }
            this.ConstScope = this.ConstScope.Parent;

            return result;
        }

        public List<TempCodeLine> ParseJump(Token input)
        {
            var parse = Parse(input.TokenList[1]);
            if (parse.Count == 1 && parse[0].Operator == Operator.Push)
            {
                return new List<TempCodeLine> { TempCodeLine.Code(Operator.Jump, parse[0].Token) };
            }
            parse.Add(TempCodeLine.Code(Operator.Jump, input.ToEmpty()));
            return parse;
        }

        public List<TempCodeLine> ParseReturn(Token input)
        {
            var result = new List<TempCodeLine>();
            foreach (var item in input.TokenList.Skip(1))
            {
                result.AddRange(Parse(item));
            }
            result.Add(TempCodeLine.Code(Operator.Return, input.ToEmpty()));
            return result;
        }

        public List<TempCodeLine> ParseNegative(Token input)
        {
            if (input.TokenList.Count >= 3)
            {
                return this.ParseOperator(Operator.Sub, input);
            }
            else if (input.TokenList.Count == 2)
            {
                var firstToken = input.TokenList[1];
                // If it's a constant already, just push the negative.
                if (firstToken.TokenValue is NumberValue numValue)
                {
                    return new List<TempCodeLine> { TempCodeLine.Code(Operator.Push, firstToken.KeepLocation(new NumberValue(-numValue.Value))) };
                }

                var result = this.Parse(firstToken);
                result.Add(TempCodeLine.Code(Operator.UnaryNegative, firstToken.ToEmpty()));
                return result;
            }
            else
            {
                throw this.ThrowException(input, $"Negative/Sub operator expects at least 1 input");
            }
        }

        public List<TempCodeLine> ParseOnePushInput(Operator opCode, Token input)
        {
            if (input.TokenList.Count < 2)
            {
                throw this.ThrowException(input, $"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<TempCodeLine>();
            foreach (var item in input.TokenList.Skip(1))
            {
                result.AddRange(this.Parse(item));
                result.Add(TempCodeLine.Code(opCode, item.ToEmpty()));
            }

            return result;
        }

        public List<TempCodeLine> ParseOperator(Operator opCode, Token input)
        {
            if (input.TokenList.Count < 3)
            {
                throw this.ThrowException(input, $"Expecting at least 3 inputs for: {opCode}");
            }

            var result = this.Parse(input.TokenList[1]);
            foreach (var item in input.TokenList.Skip(2))
            {
                if (item.TokenValue is NumberValue numValue)
                {
                    result.Add(TempCodeLine.Code(opCode, item));
                }
                else
                {
                    result.AddRange(this.Parse(item));
                    result.Add(TempCodeLine.Code(opCode, input.ToEmpty()));
                }
            }

            return result;
        }

        public List<TempCodeLine> ParseOneVariableUpdate(Operator opCode, Token input)
        {
            if (input.TokenList.Count < 2)
            {
                throw this.ThrowException(input, $"Expecting at least 1 input for: {opCode}");
            }

            var result = new List<TempCodeLine>();
            foreach (var item in input.TokenList.Skip(1))
            {
                var varName = new StringValue(this.GetValue(item).ToString());
                result.Add(TempCodeLine.Code(opCode, item.KeepLocation(varName)));
            }

            return result;
        }

        public List<TempCodeLine> ParseStringConcat(Token input)
        {
            var result = new List<TempCodeLine>();
            foreach (var item in input.TokenList.Skip(1))
            {
                result.AddRange(this.Parse(item));
            }
            result.Add(TempCodeLine.Code(Operator.StringConcat, input.KeepLocation(new NumberValue(input.TokenList.Count - 1))));
            return result;
        }

        public List<TempCodeLine> TransformAssignmentOperator(Token input)
        {
            var opCode = this.GetValue(input.TokenList[0]).ToString();
            opCode = opCode.Substring(0, opCode.Length - 1);

            var varName = this.GetValue(input.TokenList[1]).ToString();
            var newCode = input.TokenList.ToList();
            newCode[0] = input.TokenList[0].KeepLocation(new VariableValue(opCode));

            var wrappedCode = new List<Token>();
            wrappedCode.Add(input.KeepLocation(new VariableValue("set")));
            wrappedCode.Add(input.TokenList[1].KeepLocation(new VariableValue(varName)));
            wrappedCode.Add(Token.Expression(input.Location, newCode));

            return this.Parse(Token.Expression(input.Location, wrappedCode));
        }

        public virtual List<TempCodeLine> ParseKeyword(VariableValue firstSymbol, Token input)
        {
            List<TempCodeLine>? result = null;
            this.keywordParsingStack.Add(firstSymbol.Value);
            switch (firstSymbol.Value)
            {
                // General Operators
                case FunctionKeyword: result = this.ParseFunctionKeyword(input); break;
                case ContinueKeyword: result = this.ParseLoopJump(input, ContinueKeyword, true); break;
                case BreakKeyword: result = this.ParseLoopJump(input, BreakKeyword, false); break;
                case SetKeyword: result = this.ParseDefineSet(input, false); break;
                case DefineKeyword: result = this.ParseDefineSet(input, true); break;
                case ConstKeyword: result = this.ParseConst(input); break;
                case LoopKeyword: result = this.ParseLoop(input); break;
                case IfKeyword: result = this.ParseIfUnless(input, true); break;
                case UnlessKeyword: result = this.ParseIfUnless(input, false); break;
                case SwitchKeyword: result = this.ParseSwitch(input); break;
                case JumpKeyword: result = this.ParseJump(input); break;
                case ReturnKeyword: result = this.ParseReturn(input); break;

                // Math Operators
                case "+":  result = this.ParseOperator(Operator.Add, input); break;
                case "-":  result = this.ParseNegative(input); break;
                case "*":  result = this.ParseOperator(Operator.Multiply, input); break;
                case "/":  result = this.ParseOperator(Operator.Divide, input); break;
                case "++": result = this.ParseOneVariableUpdate(Operator.Inc, input); break;
                case "--": result = this.ParseOneVariableUpdate(Operator.Dec, input); break;

                // Comparison Operators
                case "<":  result = this.ParseOperator(Operator.LessThan, input); break;
                case "<=": result = this.ParseOperator(Operator.LessThanEquals, input); break;
                case "==": result = this.ParseOperator(Operator.Equals, input); break;
                case "!=": result = this.ParseOperator(Operator.NotEquals, input); break;
                case ">":  result = this.ParseOperator(Operator.GreaterThan, input); break;
                case ">=": result = this.ParseOperator(Operator.GreaterThanEquals, input); break;

                // Boolean Operators
                case "&&": result = this.ParseOperator(Operator.And, input); break;
                case "||": result = this.ParseOperator(Operator.Or, input); break;
                case "!":  result = this.ParseOnePushInput(Operator.Not, input); break;

                // Misc Operators
                case "$":  result = this.ParseStringConcat(input); break;

                // Conjoined Operators
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "&&=":
                case "||=":
                case "$=":
                    result = this.TransformAssignmentOperator(input); break;
            }

            this.keywordParsingStack.PopBack();

            return result ?? new List<TempCodeLine>();
        }
        #endregion

        #region Helper Methods
        private List<TempCodeLine> OptimiseCallSymbolValue(Token input, int numArgs)
        {
            if (input.Type != TokenType.Value)
            {
                throw this.ThrowException(input, "Call token must be a value");
            }

            var numArgsValue = new NumberValue(numArgs);

            var result = this.OptimiseGet(input, this.GetValue(input).ToString());
            if (result.Count == 1 && result[0].Operator == Operator.Push)
            {
                var callValue = new IValue[]{ this.GetValue(result[0].Token), numArgsValue };
                return new List<TempCodeLine> {
                    TempCodeLine.Code(Operator.CallDirect, input.KeepLocation(new ArrayValue(callValue)))
                };
            }

            result.Add(TempCodeLine.Code(Operator.Call, input.KeepLocation(numArgsValue)));
            return result;
        }

        private List<TempCodeLine> OptimiseGetSymbolValue(Token input)
        {
            if (input.Type != TokenType.Value)
            {
                throw this.ThrowException(input, "Get symbol token value cannot be null");
            }

            var isArgumentUnpack = false;
            var key = this.GetValue(input).ToString();
            if (key.StartsWith("..."))
            {
                isArgumentUnpack = true;
                key = key.Substring(3);
            }

            var result = this.OptimiseGet(input, key);

            if (isArgumentUnpack)
            {
                result.Add(TempCodeLine.Code(Operator.ToArgument, input.ToEmpty()));
            }

            return result;
        }

        private List<TempCodeLine> OptimiseGet(Token input, string key)
        {
            if (input.Type != TokenType.Value)
            {
                throw this.ThrowException(input, "Get symbol token must be a value");
            }

            var result = new List<TempCodeLine>();

            if (this.ConstScope.TryGetKey(key, out var foundConst))
            {
                result.Add(TempCodeLine.Code(Operator.Push, input.KeepLocation(foundConst)));
                return result;
            }

            var isProperty = IsGetPropertyRequest(key, out var parentKey, out var property);

            // Check if we know about the parent object? (eg: string.length, the parent is the string object)
            if (this.BuiltinScope.TryGetKey(parentKey, out var foundParent))
            {
                // If the get is for a property? (eg: string.length, length is the property)
                if (isProperty)
                {
                    if (ValuePropertyAccess.TryGetProperty(foundParent, property, out var foundProperty))
                    {
                        // If we found the property then we're done and we can just push that known value onto the stack.
                        result.Add(TempCodeLine.Code(Operator.Push, input.KeepLocation(foundProperty)));
                    }
                    else
                    {
                        // We didn't find the property at compile time, so look it up at run time.
                        result.Add(TempCodeLine.Code(Operator.Push, input.KeepLocation(foundParent)));
                        result.Add(TempCodeLine.Code(Operator.GetProperty, input.KeepLocation(property)));
                    }
                }
                else
                {
                    // This was not a property request but we found the parent so just push onto the stack.
                    result.Add(TempCodeLine.Code(Operator.Push, input.KeepLocation(foundParent)));
                }
            }
            else
            {
                // Could not find the parent right now, so look for the parent at runtime.
                result.Add(TempCodeLine.Code(Operator.Get, input.KeepLocation(parentKey)));

                // If this was also a property check also look up the property at runtime.
                if (isProperty)
                {
                    result.Add(TempCodeLine.Code(Operator.GetProperty, input.KeepLocation(property)));
                }
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

        private Function ProcessTempFunction(IReadOnlyList<string> parameters, IReadOnlyList<TempCodeLine> tempCodeLines, string name)
        {
            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();
            var locations = new List<CodeLocation>();

            foreach (var tempLine in tempCodeLines)
            {
                if (tempLine.IsLabel)
                {
                    labels.Add(tempLine.LabelLine, code.Count);
                }
                else
                {
                    locations.Add(tempLine.Token.Location);
                    code.Add(new CodeLine(tempLine.Operator, this.GetValueCanBeEmpty(tempLine.Token)));
                }
            }

            var debugSymbols = new DebugSymbols(this.SourceName, this.FullText, locations);

            return new Function(code, parameters, labels, name, debugSymbols);
        }

        private IValue? GetValueCanBeEmpty(Token input)
        {
            if (input.Type == TokenType.Empty)
            {
                return null;
            }

            return this.GetValue(input);
        }
        private IValue GetValue(Token input)
        {
            if (input.Type == TokenType.Value)
            {
                return input.TokenValue;
            }

            throw this.ThrowException(input, "Unable to get value of non value token");
        }

        private static bool IsNoOperator(IReadOnlyList<TempCodeLine> input)
        {
            if (input.Count == 1)
            {
                return input[0].Operator == Operator.Unknown;
            }

            return false;
        }

        private static void AddHandleNested(List<Token> target, Token input)
        {
            if (input.IsNestedExpression())
            {
                target.AddRange(input.TokenList);
            }
            else
            {
                target.Add(input);
            }
        }
        #endregion

        #endregion
    }
}