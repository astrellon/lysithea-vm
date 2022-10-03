using System;

namespace SimpleStackVM
{
    public static class StandardValueLibrary
    {
        #region Fields
        public const string HandleName = "value";
        #endregion

        #region Methods
        public static void AddHandler(VirtualMachine vm)
        {
            vm.AddBuiltinHandler(HandleName, Handler);
        }

        public static void Handler(string command, VirtualMachine vm)
        {
            switch (command)
            {
                case "toString":
                    {
                        var top = vm.PeekStack();
                        if (top is StringValue)
                        {
                            break;
                        }

                        top = vm.PopStack();
                        vm.PushStack(new StringValue(top.ToString()));
                        break;
                    }
                case "typeof":
                    {
                        var top = vm.PopStack();
                        vm.PushStack(new StringValue(GetTypeOf(top)));
                        break;
                    }
                case "compareTo":
                    {
                        var right = vm.PopStack();
                        var left = vm.PopStack();
                        vm.PushStack(new NumberValue(left.CompareTo(right)));
                        break;
                    }
            }
        }

        public static string GetTypeOf(IValue input)
        {
            if (input is StringValue)
            {
                return "string";
            }
            if (input is NumberValue)
            {
                return "number";
            }
            if (input is BoolValue)
            {
                return "bool";
            }
            if (input is ArrayValue)
            {
                return "array";
            }
            if (input is ObjectValue)
            {
                return "object";
            }
            if (input is NullValue)
            {
                return "null";
            }
            if (input is AnyValue)
            {
                return "any";
            }

            return "unknown";
        }
        #endregion
    }
}
