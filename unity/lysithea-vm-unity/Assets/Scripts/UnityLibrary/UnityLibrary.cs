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
            result.TryDefine("color", colourFunctions);
            result.TryDefine("colour", colourFunctions);

            result.TryDefine("random", CreateRandomFunctions());
            result.TryDefine("vector3", CreateVector3Functions());

            // Override the default print with a Unity specific one.
            result.TryDefine("print", (vm, args) =>
            {
                Debug.Log(string.Join("", args.Value));
            });

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
                }, "colour.new")},
                {"hsv", new BuiltinFunctionValue((vm, args) =>
                {
                    var hue = args.GetIndex<NumberValue>(0).FloatValue;
                    var saturation = args.GetIndex<NumberValue>(1).FloatValue;
                    var value = args.GetIndex<NumberValue>(2).FloatValue;

                    vm.PushStack(new ColourValue(Color.HSVToRGB(hue, saturation, value)));
                }, "colour.hsv")}
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
                }, "random.pick")},
                {"bool", new BuiltinFunctionValue((vm, args) =>
                {
                    var isTrue = Random.value >= 0.5;
                    vm.PushStack(isTrue);
                }, "random.bool")},
                {"value", new BuiltinFunctionValue((vm, args) =>
                {
                    vm.PushStack(Random.value);
                }, "random.value")},
                {"range", new BuiltinFunctionValue((vm, args) =>
                {
                    var lower = args.GetIndex<NumberValue>(0);
                    var upper = args.GetIndex<NumberValue>(1);
                    var newValue = Random.Range(lower.FloatValue, upper.FloatValue);
                    vm.PushStack(newValue);
                }, "random.range")}
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
                }, "vector3.new")}
            });
        }
        #endregion
    }
}