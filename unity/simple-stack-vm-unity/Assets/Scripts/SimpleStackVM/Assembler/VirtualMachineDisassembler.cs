using System;
using System.Linq;
using SimpleJSON;

namespace SimpleStackVM
{
    public static class VirtualMachineDisassembler
    {
        #region Methods
        public static JSONNode Disassemble(VirtualMachine vm)
        {
            var result = new JSONArray();
            foreach (var scope in vm.Procedures.Values)
            {
                result.Add(Disassemble(scope));
            }
            return result;
        }

        public static JSONNode Disassemble(Procedure procedure)
        {
            var result = new JSONObject();
            result["name"] = new JSONString(procedure.Name);

            var data = new JSONArray();
            result["data"] = data;

            foreach (var codeLine in procedure.Code)
            {
                data.Add(Disassemble(codeLine));
            }

            var sortedLabels = procedure.Labels.ToList();
            // Sort from last label to first
            sortedLabels.Sort((x, y) => y.Value.CompareTo(x.Value));

            foreach (var kvp in sortedLabels)
            {
                data[kvp.Value] = new JSONString(kvp.Key);
            }

            return result;
        }

        public static JSONNode Disassemble(CodeLine codeLine)
        {
            if (codeLine.Input == null || codeLine.Input.Equals(NullValue.Value))
            {
                return new JSONString(codeLine.Operator.ToString());
            }

            var result = new JSONArray();
            result.Add(new JSONString(codeLine.Operator.ToString()));
            result.Add(Convert(codeLine.Input));
            return result;
        }

        public static JSONNode Convert(IValue input)
        {
            if (input is StringValue stringValue)
            {
                return new JSONString(stringValue.Value);
            }
            if (input is BoolValue boolValue)
            {
                return new JSONBool(boolValue.Value);
            }
            if (input is NumberValue numberValue)
            {
                return new JSONNumber(numberValue.Value);
            }
            if (input is ObjectValue objectValue)
            {
                var result = new JSONObject();
                foreach (var kvp in objectValue.Value)
                {
                    result[kvp.Key] = Convert(kvp.Value);
                }
                return result;
            }
            if (input is ArrayValue arrayValue)
            {
                var result = new JSONArray();
                foreach (var item in arrayValue.Value)
                {
                    result.Add(Convert(item));
                }
                return result;
            }

            throw new Exception($"Unknown IValue to convert: {input.ToString()}");
        }
        #endregion
    }
}