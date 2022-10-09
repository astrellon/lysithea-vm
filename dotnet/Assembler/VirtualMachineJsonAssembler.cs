using System;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;

#nullable enable

namespace SimpleStackVM
{
    public static class VirtualMachineJsonAssembler
    {
        #region Methods
        public static List<Function> ParseProcedures(JSONArray input)
        {
            var result = new List<Function>();
            foreach (var child in input.Children)
            {
                if (child.IsObject)
                {
                    var procedure = ParseProcedure(child.AsObject);
                    result.Add(procedure);
                }
            }

            return result;
        }

        public static Function ParseProcedure(JSONObject input)
        {
            var name = input["name"].Value;
            var data = input["data"].AsArray;
            var tempCodeLines = new List<ITempCodeLine>();

            foreach (var child in data.Children)
            {
                if (child.IsArray)
                {
                    tempCodeLines.AddRange(ParseCodeLine(child.AsArray));
                }
                else
                {
                    var list = new JSONNode[] { child };
                    tempCodeLines.AddRange(ParseCodeLine(list));
                }
            }

            var parameters = Function.EmptyParameters;
            if (input.HasKey("args"))
            {
                parameters = input["args"].Children.Select(s => s.Value).ToList();
            }

            return VirtualMachineAssembler.ProcessTempFunction(parameters, tempCodeLines);
        }

        private static IEnumerable<ITempCodeLine> ParseCodeLine(IReadOnlyList<JSONNode> input)
        {
            if (!input.Any())
            {
                yield break;
            }

            var first = input.First();
            if (first.IsString && first.Value[0] == ':')
            {
                yield return new LabelCodeLine(first);
                yield break;
            }

            var opCode = Operator.Unknown;
            var pushChildOffset = 1;
            IValue? codeLineInput = null;

            if (!VirtualMachineAssembler.TryParseOperator(first.Value, out opCode))
            {
                opCode = Operator.Call;
                if (!TryParseRunCommand(first, out codeLineInput))
                {
                    throw new Exception($"Error parsing run input for: {input.ToString()}");
                }
                pushChildOffset = 0;
            }
            else if (input.Count > 1)
            {
                if (VirtualMachineAssembler.IsJumpCall(opCode))
                {
                    if (!TryParseJumpLabel(input.Last(), out codeLineInput))
                    {
                        throw new Exception($"Error parsing {opCode} input: {input.ToString()}");
                    }
                }
                else
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

        private static bool TryParseJumpLabel(JSONNode input, out IValue result)
        {
            return TryParseTwoStringInput(input, ':', true, out result);
        }

        private static bool TryParseRunCommand(JSONNode input, out IValue result)
        {
            return TryParseTwoStringInput(input, '.', false, out result);
        }

        private static bool TryParseTwoStringInput(JSONNode input, char delimiter, bool includeDelimiter, out IValue result)
        {
            if (input.IsString)
            {
                var delimiterIndex = input.Value.IndexOf(delimiter);
                if (delimiterIndex > 0)
                {
                    var array = new List<IValue>(2);
                    array.Add(new StringValue(input.Value.Substring(0, delimiterIndex)));
                    if (includeDelimiter)
                    {
                        array.Add(new StringValue(input.Value.Substring(delimiterIndex)));
                    }
                    else
                    {
                        array.Add(new StringValue(input.Value.Substring(delimiterIndex + 1)));
                    }

                    result = new ArrayValue(array);
                    return true;
                }

                result = new StringValue(input.Value);
                return true;
            }
            else if (input.IsArray)
            {
                var jsonArray = input.AsArray;
                if (jsonArray.Count == 1)
                {
                    return TryParseTwoStringInput(jsonArray[0], delimiter, includeDelimiter, out result);
                }

                if (TryParseJson(jsonArray, out var parsed))
                {
                    if (parsed is ArrayValue parsedArray && parsedArray.Value.Count == 2)
                    {
                        if (parsedArray.Value[0] is StringValue &&
                            parsedArray.Value[1] is StringValue)
                        {
                            result = parsedArray;
                            return true;
                        }
                    }
                }
            }

            result = NullValue.Value;
            return false;
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