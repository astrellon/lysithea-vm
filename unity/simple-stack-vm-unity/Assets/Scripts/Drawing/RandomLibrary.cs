using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

#nullable enable

namespace SimpleStackVM.Unity
{
    public static class RandomLibrary
    {
        #region Fields
        public const string HandleName = "random";
        #endregion

        #region Methods
        public static void AddHandler(VirtualMachine vm)
        {
            vm.AddRunHandler(HandleName, Handler);
        }

        public static void Handler(string command, VirtualMachine vm)
        {
            switch (command)
            {
                case "pick":
                    {
                        var list = vm.PopStack<ArrayValue>();
                        var index = Random.Range(0, list.Value.Count);
                        vm.PushStack(list.Value[index]);
                        break;
                    }
                case "bool":
                    {
                        var isTrue = Random.value >= 0.5;
                        vm.PushStack((BoolValue)isTrue);
                        break;
                    }
                case "color":
                case "colour":
                    {
                        var colour = RandomColour();
                        vm.PushStack(new AnyValue(colour));
                        break;
                    }
                case "range":
                    {
                        var upper = vm.PopStack<NumberValue>();
                        var lower = vm.PopStack<NumberValue>();
                        var newValue = Random.Range(lower, upper);
                        vm.PushStack(new NumberValue(newValue));
                        break;
                    }
                case "vector":
                    {
                        var vector1Value = vm.PopStack();
                        var vector2Value = vm.PopStack();

                        if (!UnityUtils.TryGetVector(vector1Value, out var vector1) ||
                            !UnityUtils.TryGetVector(vector2Value, out var vector2))
                        {
                            Debug.LogError("Unable to parse vectors for random vector");
                            return;
                        }

                        var x = Random.Range(vector1.x, vector2.x);
                        var y = Random.Range(vector1.y, vector2.y);
                        var z = Random.Range(vector1.z, vector2.z);

                        vm.PushStack(new AnyValue(new Vector3(x, y, z)));
                        break;
                    }
            }
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