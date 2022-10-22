using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace SimpleStackVM.Unity
{
    public static class RandomLibrary
    {
        #region Fields
        public const string HandleName = "random";
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var randomFunctions = new Dictionary<string, IValue>
            {
                {"pick", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var list = vm.PopStack<ArrayValue>();
                    var index = Random.Range(0, list.Value.Count);
                    vm.PushStack(list.Value[index]);
                })},
                {"bool", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var isTrue = Random.value >= 0.5;
                    vm.PushStack(isTrue);
                })},
                {"value", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    vm.PushStack(Random.value);
                })},
                {"range", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var upper = vm.PopStack<NumberValue>();
                    var lower = vm.PopStack<NumberValue>();
                    var newValue = Random.Range(lower.FloatValue, upper.FloatValue);
                    vm.PushStack(newValue);
                })}
            };

            result.Define("random", new ObjectValue(randomFunctions));

            return result;
        }

        private static Color RandomColour()
        {
            var colourHue = Random.value;
            var colourSaturation = Random.Range(0.5f, 1.0f);
            var colourValue = Random.Range(0.5f, 1.0f);
            return Color.HSVToRGB(colourHue, colourSaturation, colourValue);
        }
        #endregion
    }
}