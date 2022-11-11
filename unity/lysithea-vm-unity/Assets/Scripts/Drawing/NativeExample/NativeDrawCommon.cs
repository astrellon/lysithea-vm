using System.Linq;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public static class NativeDrawCommon
    {
        #region Methods
        public static Color RandomColour()
        {
            var hue = Random.Range(0.0f, 1.0f);
            var saturation = Random.Range(0.5f, 1.0f);
            var value = Random.Range(0.5f, 1.0f);

            return Color.HSVToRGB(hue, saturation, value);
        }

        public static Vector3 RandomVector3(Vector3 from, Vector3 to)
        {
            var x = Random.Range(from.x, to.x);
            var y = Random.Range(from.y, to.y);
            var z = Random.Range(from.z, to.z);

            return new Vector3(x, y, z);
        }

        public static void MakeGround(DrawingContext context)
        {
            context.DrawElement("Plane", Vector3.zero, RandomColour(), Vector3.one * 100.0f);
        }
        #endregion
    }
}