using System;
using System.Collections.Generic;

#nullable enable

namespace SimpleStackVM
{
    public static class VirtualMachineAssembler
    {
        public static Function ProcessTempFunction(IReadOnlyList<string> parameters, IReadOnlyList<ITempCodeLine> tempCodeLines)
        {
            var labels = new Dictionary<string, int>();
            var code = new List<CodeLine>();

            foreach (var tempLine in tempCodeLines)
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

            return new Function(code, parameters, labels);
        }

        public static bool TryParseOperator(string input, out Operator result)
        {
            if (!Enum.TryParse<Operator>(input, true, out result))
            {
                result = Operator.Unknown;
                return false;
            }

            return true;
        }

        public static bool IsJumpCall(Operator input)
        {
            return input == Operator.Call || input == Operator.Jump ||
                input == Operator.JumpTrue || input == Operator.JumpFalse;
        }
    }
}