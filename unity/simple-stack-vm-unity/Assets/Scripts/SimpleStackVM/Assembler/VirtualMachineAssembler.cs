using System;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;

#nullable enable

namespace SimpleStackVM
{
    public static class VirtualMachineAssembler
    {
        private interface ITempCodeLine { }

        private class LabelCodeLine : ITempCodeLine
        {
            public readonly string Label;

            public LabelCodeLine(string label) { this.Label = label; }
        }

        private class TempCodeLine : ITempCodeLine
        {
            public readonly Operator Operator;
            public readonly IValue? Argument;

            public TempCodeLine(Operator op, IValue? argument)
            {
                this.Operator = op;
                this.Argument = argument;
            }
        }
        #region Methods
        public static List<Scope> ParseScopes(JSONArray input)
        {
            var result = new List<Scope>();
            foreach (var child in input.Children)
            {
                if (child.IsObject)
                {
                    var scope = ParseScope(child.AsObject);
                    result.Add(scope);
                }
            }

            return result;
        }

        public static Scope ParseScope(JSONObject input)
        {
            var scopeName = input["name"].Value;
            var data = input["data"].AsArray;
            var tempCodeLines = new List<ITempCodeLine>();

            foreach (var child in data.Children)
            {
                if (child.IsString)
                {
                    var list = new JSONNode[] { child };
                    tempCodeLines.AddRange(ParseCodeLine(list));
                }
                else if (child.IsArray)
                {
                    tempCodeLines.AddRange(ParseCodeLine(child.AsArray));
                }
                else
                {
                    throw new Exception($"Invalid Json node: {input.ToString()}");
                }
            }

            return ProcessScope(scopeName, tempCodeLines);
        }

        private static Scope ProcessScope(string scopeName, IReadOnlyList<ITempCodeLine> tempCode)
        {
            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();

            foreach (var tempLine in tempCode)
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

            return new Scope(scopeName, code, labels);
        }

        private static IEnumerable<ITempCodeLine> ParseCodeLine(IReadOnlyList<JSONNode> input)
        {
            if (!input.Any())
            {
                yield break;
            }

            var first = input.First().Value;
            if (first[0] == ':')
            {
                yield return new LabelCodeLine(first);
                yield break;
            }

            var opCode = Operator.Unknown;
            var pushChildOffset = 1;
            IValue? codeLineInput = null;
            if (first[0] == '$')
            {
                opCode = Operator.Run;
                codeLineInput = new StringValue(first.Substring(1));
                pushChildOffset = 0;
            }
            else
            {
                if (!Enum.TryParse<Operator>(first, true, out opCode))
                {
                    throw new Exception($"Unknown operator: {first}");
                }

                if (input.Count > 1)
                {
                    if (!TryParseJson(input.Last(), out codeLineInput))
                    {
                        throw new Exception($"Error parsing input for: {input.ToString()}");
                    }
                }
            }

            for (var i = 1; i < input.Count - pushChildOffset; i++)
            {
                var child = input[i];
                if (TryParseJson(child, out var pushValue))
                {
                    yield return new TempCodeLine(Operator.Push, pushValue);
                }
                else
                {
                    throw new Exception($"Error parsing child for value: {child.ToString()}");
                }
            }

            yield return new TempCodeLine(opCode, codeLineInput);
        }

        private static bool TryParseJson(JSONNode input, out IValue result)
        {
            if (input.IsString)
            {
                result = new StringValue(input.Value);
                return true;
            }
            if (input.IsBoolean)
            {
                result = new BoolValue(input.AsBool);
                return true;
            }
            if (input.IsNumber)
            {
                result = new NumberValue(input.AsDouble);
                return true;
            }

            result = NullValue.Value;

            if (input.IsObject)
            {
                var objectDictionary = new Dictionary<string, IValue>();
                foreach (var kvp in input)
                {
                    if (TryParseJson(kvp.Value, out var dictionaryValue))
                    {
                        objectDictionary[kvp.Key] = dictionaryValue;
                    }
                    else
                    {
                        return false;
                    }
                }

                result = new ObjectValue(objectDictionary);
                return true;
            }

            if (input.IsArray)
            {
                var array = new List<IValue>();

                foreach (var child in input.Children)
                {
                    if (TryParseJson(child, out var arrayValue))
                    {
                        array.Add(arrayValue);
                    }
                    else
                    {
                        return false;
                    }
                }

                result = new ArrayValue(array);
                return true;
            }

            if (input.IsNull)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}