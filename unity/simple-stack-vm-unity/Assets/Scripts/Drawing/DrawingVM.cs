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
            StandardLibrary.AddToVirtualMachine(this.vm);
            RandomLibrary.AddHandler(this.vm);
        }

        public void StartDrawing(IEnumerable<IDrawingScript> drawingScripts, string startScope)
        {
            this.vm.ClearScopes();
            foreach (var drawingScript in drawingScripts)
            {
                drawingScript.Awake();
                this.vm.AddScopes(drawingScript.Scopes);
            }
            this.vm.SetCurrentScope(startScope);
            this.vm.Reset();
            this.vm.Running = true;
            this.VMRunner.Running = true;
        }

        private void OnRunHandler(string command, VirtualMachine vm)
        {
            if (command == "drawComplex")
            {
                var details = vm.PopStack<ObjectValue>();
                var positionValue = vm.PopStack();
                var elementName = vm.PopStack<StringValue>();

                if (!UnityUtils.TryGetVector(positionValue, out var position))
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

                if (!UnityUtils.TryGetVector(positionValue, out var position))
                {
                    Debug.LogError("Unable to get vector from stack");
                    return;
                }

                if (!UnityUtils.TryGetVector(scaleValue, out var scale))
                {
                    Debug.LogError("Unable to get scale vector from stack");
                    return;
                }

                if (!UnityUtils.TryGetColour(colourValue, out var colour))
                {
                    Debug.LogError("Unable to get colour from stack");
                    return;
                }

                this.DrawElement(elementName.Value, position, colour, scale);
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
                if (UnityUtils.TryGetColour(colourValue, out var unityColour))
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
    }
}
