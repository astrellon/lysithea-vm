using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public static class NativeDrawCircle
    {
        #region Methods
        public static Vector3 CalcPosition(int index)
        {
            var angle = (float)index * 5.0f * (float)StandardMathLibrary.DegToRad;

            var x = Mathf.Cos(angle) * 20.0f;
            var y = (float)index * 0.15f;
            var z = Mathf.Sin(angle) * 20.0f;

            return new Vector3(x, y, z);
        }

        public static Color CalcColour(int index)
        {
            var hue = (float)index / 100.0f;
            return Color.HSVToRGB(hue, 0.8f, 0.75f);
        }
        #endregion
    }
}
