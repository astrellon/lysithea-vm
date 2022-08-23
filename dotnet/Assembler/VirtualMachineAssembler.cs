using System;
using System.Linq;
using SimpleJSON;

namespace SimpleStackVM
{
    public class VirtualMachineAssembler
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
            public readonly IValue Argument;

            public TempCodeLine(Operator op, IValue argument)
            {
                this.Operator = op;
                this.Argument = argument;
            }
        }
        #region Fields
        #endregion

        #region Constructor
        public VirtualMachineAssembler()
        {

        }
        #endregion

        #region Methods
        public List<Scope> ParseScopes(JSONArray input)
        {
            var result = new List<Scope>();
            foreach (var child in input.Children)
            {
                if (child.IsObject)
                {
                    var scope = this.ParseScope(child.AsObject);
                    result.Add(scope);
                }
            }

            return result;
        }

        public Scope ParseScope(JSONObject input)
        {
            var scopeName = input["scopeName"].Value;
            var data = input["data"].AsArray;
            var tempCodeLines = new List<ITempCodeLine>();

            foreach (var child in data.Children)
            {
                if (child.IsString)
                {

                }
            }

            return new Scope(scopeName, null);
        }

        private IEnumerable<ITempCodeLine> ParseCodeLine(IReadOnlyList<JSONNode> input)
        {
            if (!input.Any())
            {
                yield break;
            }

            var first = input.First().Value;
            if (first[0] == ':')
            {
                yield return new LabelCodeLine(first);
            }

            var opCode = Operator.Unknown;
            IValue codeLineInput = NullValue.Value;
            if (first[0] == '$')
            {
                opCode = Operator.Run;
                codeLineInput = new StringValue(first.Substring(1));
            }
            else
            {
                if (!Enum.TryParse<Operator>(first, true, out opCode))
                {
                    throw new Exception($"Unknown operator: {first}");
                }
            }

            foreach (var child in input.Skip(1))
            {
                if (this.TryParseJson(child, out var pushValue))
                {
                    yield return new TempCodeLine(Operator.Push, pushValue);
                }
                else
                {
                    throw new Exception($"Error parsing child for value: {child.ToString()}");
                }
            }

            yield return new TempCodeLine(opCode);
        }

        private bool TryParseJson(JSONNode input, out IValue result)
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
                    if (this.TryParseJson(kvp.Value, out var dictionaryValue))
                    {
                        objectDictionary[kvp.Key] = dictionaryValue;
                    }
                    else
                    {
                        // throw new Exception($"Error turning JSON into ObjectValue: {kvp.Value.ToString()}");
                        return false;
                    }
                }
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