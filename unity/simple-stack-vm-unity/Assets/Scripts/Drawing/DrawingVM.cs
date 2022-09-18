using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DrawingVM : MonoBehaviour
    {
        public static DrawingVM Instance;

        public List<GameObject> Prefabs;
        public Transform ElementParent;
        public GameObject AppearParticlesPrefab;

        public VMRunner VMRunner;

        private VirtualMachine vm => this.VMRunner.VM;

        void Awake()
        {
            Instance = this;
            this.VMRunner.Init(32, this.OnRunHandler);
        }

        public void StartDrawing(IEnumerable<DrawingScript> drawingScripts, string startScope)
        {
            this.vm.ClearScopes();
            foreach (var drawingScript in drawingScripts)
            {
                drawingScript.Awake();
                this.vm.AddScopes(drawingScript.Scopes);
            }
            this.vm.SetCurrentScope(startScope);
            this.vm.Restart();
            this.VMRunner.Running = true;
        }

        private void OnRunHandler(IValue value, VirtualMachine vm)
        {
            var command = value.ToString();

            if (command == "drawComplex")
            {
                var details = vm.PopStack<ObjectValue>();
                var positionValue = vm.PopStack();
                var elementName = vm.PopStack<StringValue>();

                if (!TryGetVector(positionValue, out var position))
                {
                    Debug.LogError("Unable to get vector from stack");
                    return;
                }

                this.DrawElement(elementName.Value, position, details);
            }
            else if (command == "drawSimple")
            {
                var scaleValue = vm.PopStack<AnyValue>();
                var colourValue = vm.PopStack<AnyValue>();
                var positionValue = vm.PopStack();
                var elementName = vm.PopStack<StringValue>();

                if (!TryGetVector(positionValue, out var position))
                {
                    Debug.LogError("Unable to get vector from stack");
                    return;
                }

                if (!TryGetVector(scaleValue, out var scale))
                {
                    Debug.LogError("Unable to get scale vector from stack");
                    return;
                }

                if (!TryGetColour(colourValue, out var colour))
                {
                    Debug.LogError("Unable to get colour from stack");
                    return;
                }

                this.DrawElement(elementName.Value, position, colour, scale);
            }
            else if (command == "setKey")
            {
                var key = vm.PopStack<StringValue>();
                var top = vm.PopStack();
                var onto = vm.PopStack<ObjectValue>();

                vm.PushStack(onto.Set(key.Value, top));
            }
            else if (command == "randomPick")
            {
                var list = vm.PopStack<ArrayValue>();
                var index = Random.Range(0, list.Value.Count);
                vm.PushStack(list.Value[index]);
            }
            else if (command == "randomTrue")
            {
                var isTrue = Random.value >= 0.5;
                vm.PushStack((BoolValue)isTrue);
            }
            else if (command == "randomColour")
            {
                var colour = RandomColour();
                vm.PushStack(new AnyValue(colour));
            }
            else if (command == "randomRange")
            {
                var upper = vm.PopStack<NumberValue>();
                var lower = vm.PopStack<NumberValue>();
                var newValue = Random.Range(lower, upper);
                vm.PushStack(new NumberValue(newValue));
            }
            else if (command == "randomVector")
            {
                var vector1Value = vm.PopStack();
                var vector2Value = vm.PopStack();

                if (!TryGetVector(vector1Value, out var vector1) ||
                    !TryGetVector(vector2Value, out var vector2))
                {
                    Debug.LogError("Unable to parse vectors for random vector");
                    return;
                }

                var x = Random.Range(vector1.x, vector2.x);
                var y = Random.Range(vector1.y, vector2.y);
                var z = Random.Range(vector1.z, vector2.z);

                vm.PushStack(new AnyValue(new Vector3(x, y, z)));
            }
            else if (command == "decrement")
            {
                var num = vm.PopStack<NumberValue>();
                vm.PushStack(new NumberValue(num - 1));
            }
            else if (command == "isZero")
            {
                var top = vm.PeekStack<NumberValue>();
                vm.PushStack(new BoolValue(top.Value == 0));
            }
            else if (command == "clearScene")
            {
                var children = new List<GameObject>();
                foreach (Transform child in this.ElementParent) children.Add(child.gameObject);
                children.ForEach(child => Destroy(child));
            }
        }

        private void DrawElement(string elementName, Vector3 position, ObjectValue details)
        {
            if (!this.TryFindPrefab(elementName, out var prefab))
            {
                Debug.Log($"Unknown element name: {elementName}");
                return;
            }

            Color? colour = null;
            if (details.TryGetValue("colour", out var colourValue))
            {
                if (TryGetColour(colourValue, out var unityColour))
                {
                    colour = unityColour;
                }
            }

            var newElement = Instantiate(prefab, position, Quaternion.identity, this.ElementParent);
            ApplyTransform(details, newElement);
            if (colour.HasValue)
            {
                newElement.GetComponent<Renderer>().material.color = colour.Value;
            }

            this.CreateParticles(position, colour);
        }

        private void DrawElement(string elementName, Vector3 position, Color colour, Vector3 scale)
        {
            if (!this.TryFindPrefab(elementName, out var prefab))
            {
                Debug.Log($"Unknown element name: {elementName}");
                return;
            }

            var newElement = Instantiate(prefab, position, Quaternion.identity, this.ElementParent);
            newElement.GetComponent<Renderer>().material.color = colour;
            newElement.transform.localScale = scale;
            this.CreateParticles(position, colour);
        }

        private void CreateParticles(Vector3 position, Color? colour)
        {
            var newParticles = Instantiate(this.AppearParticlesPrefab, position, Quaternion.identity, this.ElementParent);
            var ps = newParticles.GetComponent<ParticleSystem>();

            if (colour.HasValue)
            {
                Color.RGBToHSV(colour.Value, out var hue, out var saturation, out var value);
                var lowerHue = LoopClamp01(hue - 0.075f);
                var upperHue = LoopClamp01(hue + 0.075f);

                var lowerColour = Color.HSVToRGB(lowerHue, 0.2f, 0.9f);
                var upperColour = Color.HSVToRGB(upperHue, 0.2f, 0.9f);

                var main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(lowerColour, upperColour);
            }
        }

        private static float LoopClamp01(float input)
        {
            while (input < 0.0f) input += 1.0f;
            while (input > 1.0f) input -= 1.0f;
            return input;
        }

        private bool TryFindPrefab(string element, out GameObject prefab)
        {
            foreach (var item in this.Prefabs)
            {
                if (item.name.Equals(element, System.StringComparison.OrdinalIgnoreCase))
                {
                    prefab = item;
                    return true;
                }
            }

            prefab = null;
            return false;
        }

        private static bool TryGetColour(IValue input, out Color result)
        {
            if (input is AnyValue colourAny)
            {
                if (colourAny.Value is Color unityColour)
                {
                    result = unityColour;
                    return true;
                }
            }

            result = Color.black;
            return false;
        }

        private static bool TryGetVector(IValue input, out Vector3 result)
        {
            if (input is AnyValue anyInput && anyInput.Value is Vector3 inputVector)
            {
                result = inputVector;
                return true;
            }
            else if (input is ObjectValue objectValue)
            {
                result = Vector3.zero;
                if (objectValue.TryGetValue<NumberValue>("x", out var x))
                {
                    result.x = x;
                }
                if (objectValue.TryGetValue<NumberValue>("y", out var y))
                {
                    result.y = y;
                }
                if (objectValue.TryGetValue<NumberValue>("z", out var z))
                {
                    result.z = z;
                }

                return true;
            }

            result = Vector3.zero;
            return false;
        }

        private static void ApplyTransform(ObjectValue details, GameObject target)
        {
            if (!details.TryGetValue("scale", out var scaleValue))
            {
                return;
            }

            var scale = Vector3.one;
            if (scaleValue is NumberValue numberValue)
            {
                scale *= numberValue;
            }
            else if (scaleValue is ObjectValue objectValue)
            {
                if (objectValue.TryGetValue<NumberValue>("width", out var width))
                {
                    scale.x = width;
                }
                if (objectValue.TryGetValue<NumberValue>("length", out var length))
                {
                    scale.z = length;
                }
                if (objectValue.TryGetValue<NumberValue>("height", out var height))
                {
                    scale.y = height;
                }
            }

            target.transform.localScale = scale;
        }


        private static Color RandomColour()
        {
            var colourHue = Random.value;
            var colourSaturation = Random.Range(0.5f, 1.0f);
            var colourValue = Random.Range(0.5f, 1.0f);
            return Color.HSVToRGB(colourHue, colourSaturation, colourValue);
        }
    }
}
