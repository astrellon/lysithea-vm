using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace SimpleStackVM.Unity
{
    public static class UnityLibrary
    {
        #region Fields
        public static readonly IReadOnlyScope Scope = CreateScope();
        #endregion

        #region Methods
        public static Scope CreateScope()
        {
            var result = new Scope();

            var colourFunctions = CreateColourFunctions();
            result.Define("color", colourFunctions);
            result.Define("colour", colourFunctions);

            result.Define("random", CreateRandomFunctions());
            result.Define("vector3", CreateVector3Functions());

            return result;
        }

        private static ObjectValue CreateColourFunctions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
            {
                {"new", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var alpha = 1.0f;
                    if (numArgs == 4)
                    {
                        alpha = vm.PopStack<NumberValue>().FloatValue;
                    }
                    var blue = vm.PopStack<NumberValue>().FloatValue;
                    var green = vm.PopStack<NumberValue>().FloatValue;
                    var red = vm.PopStack<NumberValue>().FloatValue;

                    vm.PushStack(new ColourValue(new Color(red, green, blue, alpha)));
                })},
                {"hsv", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var value = vm.PopStack<NumberValue>().FloatValue;
                    var saturation = vm.PopStack<NumberValue>().FloatValue;
                    var hue = vm.PopStack<NumberValue>().FloatValue;

                    vm.PushStack(new ColourValue(Color.HSVToRGB(hue, saturation, value)));
                })}
            });
        }

        private static ObjectValue CreateRandomFunctions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
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
            });
        }

        private static readonly Vector3Value Vector3Zero = new Vector3Value(Vector3.zero);
        private static ObjectValue CreateVector3Functions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
            {
                {"zero", Vector3Zero},
                {"new", new BuiltinFunctionValue((vm, numArgs) =>
                {
                    var z = vm.PopStack<NumberValue>().FloatValue;
                    var y = vm.PopStack<NumberValue>().FloatValue;
                    var x = vm.PopStack<NumberValue>().FloatValue;
                    vm.PushStack(new Vector3Value(new Vector3(x, y, z)));
                })}
            });
        }
        #endregion
    }
}