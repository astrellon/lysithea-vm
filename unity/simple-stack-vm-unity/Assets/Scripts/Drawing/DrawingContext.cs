using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class DrawingContext : MonoBehaviour
    {
        public static DrawingContext Instance { get; private set; }

        public List<GameObject> Prefabs;
        public Transform ElementParent;
        public GameObject AppearParticlesPrefab;

        void Awake()
        {
            Instance = this;
        }

        public void ClearScene()
        {
            var children = new List<GameObject>();
            foreach (Transform child in this.ElementParent) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));
        }

        public void DrawElement(string elementName, Vector3 position, Color colour, Vector3 scale)
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
    }
}
