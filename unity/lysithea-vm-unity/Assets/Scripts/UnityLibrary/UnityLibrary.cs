using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace LysitheaVM.Unity
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
                {"new", new BuiltinFunctionValue((vm, args) =>
                {
                    var red = args.GetIndex<NumberValue>(0).FloatValue;
                    var green = args.GetIndex<NumberValue>(1).FloatValue;
                    var blue = args.GetIndex<NumberValue>(2).FloatValue;
                    var alpha = 1.0f;
                    if (args.Length == 4)
                    {
                        alpha = args.GetIndex<NumberValue>(3).FloatValue;
                    }

                    vm.PushStack(new ColourValue(new Color(red, green, blue, alpha)));
                })},
                {"hsv", new BuiltinFunctionValue((vm, args) =>
                {
                    var hue = args.GetIndex<NumberValue>(0).FloatValue;
                    var saturation = args.GetIndex<NumberValue>(1).FloatValue;
                    var value = args.GetIndex<NumberValue>(2).FloatValue;

                    vm.PushStack(new ColourValue(Color.HSVToRGB(hue, saturation, value)));
                })}
            });
        }

        private static ObjectValue CreateRandomFunctions()
        {
            return new ObjectValue(new Dictionary<string, IValue>
            {
                {"pick", new BuiltinFunctionValue((vm, args) =>
                {
                    if (args.Length == 1 && args.TryGetIndex<ArrayValue>(0, out var list))
                    {
                        var index = Random.Range(0, list.Value.Count);
                        vm.PushStack(list.Value[index]);
                    }
                    else
                    {
                        var index = Random.Range(0, args.Value.Count);
                        vm.PushStack(args.Value[index]);
                    }
                })},
                {"bool", new BuiltinFunctionValue((vm, args) =>
                {
                    var isTrue = Random.value >= 0.5;
                    vm.PushStack(isTrue);
                })},
                {"value", new BuiltinFunctionValue((vm, args) =>
                {
                    vm.PushStack(Random.value);
                })},
                {"range", new BuiltinFunctionValue((vm, args) =>
                {
                    var lower = args.GetIndex<NumberValue>(0);
                    var upper = args.GetIndex<NumberValue>(1);
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
                {"new", new BuiltinFunctionValue((vm, args) =>
                {
                    var x = args.GetIndex<NumberValue>(0).FloatValue;
                    var y = args.GetIndex<NumberValue>(1).FloatValue;
                    var z = args.GetIndex<NumberValue>(2).FloatValue;
                    vm.PushStack(new Vector3Value(new Vector3(x, y, z)));
                })}
            });
        }
        #endregion
    }
}