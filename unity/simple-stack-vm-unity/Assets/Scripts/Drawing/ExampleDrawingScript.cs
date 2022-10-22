using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    [CreateAssetMenu(fileName="ExampleDrawingScript", menuName="SimpleStackVM/ExampleDrawingScript")]
    public class ExampleDrawingScript : ScriptableObject
    {
        #region Fields
        public string OptionTitle;
        public TextAsset CodeText;
        #endregion
    }
}