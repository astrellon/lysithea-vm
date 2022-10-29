
using System;

namespace SimpleStackVM
{
    public static class StandardOperators
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            result.Define(">", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) > 0);
            });

            result.Define(">=", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) >= 0);
            });

            result.Define("==", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) == 0);
            });

            result.Define("!=", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) != 0);
            });

            result.Define("<", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) < 0);
            });

            result.Define("<=", (vm, args) =>
            {
                vm.PushStack(args[0].CompareTo(args[1]) <= 0);
            });

            result.Define("!", (vm, args) =>
            {
                var value = args.GetIndex<BoolValue>(0).Value;
                vm.PushStack(!value);
            });

            result.Define("+", (vm, args) =>
            {
                if (args.Length == 0)
                {
                    return;
                }

                if (args.TryGetIndex<StringValue>(0, out var firstString))
                {
                    var result = string.Join("", args.Value);
                    vm.PushStack(result);
                }
                else
                {
                    var result = 0.0;
                    foreach (NumberValue num in args.Value)
                    {
                        result += num.Value;
                    }
                    vm.PushStack(result);
                }
            });

            result.Define("-", (vm, args) =>
            {
                vm.PushStack(args.GetIndex<NumberValue>(0).Value - args.GetIndex<NumberValue>(1).Value);
            });

            result.Define("*", (vm, args) =>
            {
                if (args.Length < 2)
                {
                    throw new Exception("Multiply operator expects more than 1 input");
                }

                var total = 1.0;
                foreach (NumberValue num in args.ArrayValues)
                {
                    total *= num.Value;
                }

                vm.PushStack(total);
            });

            result.Define("/", (vm, args) =>
            {
                vm.PushStack(args.GetIndex<NumberValue>(0).Value / args.GetIndex<NumberValue>(1).Value);
            });

            result.Define("%", (vm, args) =>
            {
                vm.PushStack(args.GetIndex<NumberValue>(0).Value % args.GetIndex<NumberValue>(1).Value);
            });

            return result;
        }
        #endregion
    }
}