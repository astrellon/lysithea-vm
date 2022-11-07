using UnityEngine;
using TMPro;

namespace SimpleStackVM.Unity
{
    public class UIDrawingCodeEdit : MonoBehaviour
    {
        public TMP_InputField CodeText;

        public IDrawingScript CreateScript()
        {
            var script = DrawingVM.Instance.AssembleScript(this.CodeText.text);
            return new DynamicDrawingScript(script);
        }
    }
}
